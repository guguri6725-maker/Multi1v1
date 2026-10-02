using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.AssetImporters;
using UnityEditor.Importer.NodeDeclaration.Validation;
using UnityEditor.Importer.Validation;
using UnityEngine;
using UnityEngine.Importer;
using Object = UnityEngine.Object;

namespace UnityEditor.Importer
{
    /// <summary>
    /// Abstract base class for custom asset importer using an <see cref="ImporterGraph"/>.
    ///
    /// Modular importers are scripts that are associated with specific file extensions. They are invoked by Unity's Asset pipeline to convert the contents of associated files into Assets.
    /// Use the <see cref="ScriptedImporterAttribute"/> class to register custom importers with the Asset pipeline.
    /// </summary>
    public abstract class ModularImporter : ScriptedImporter
    {
        private const string DependencyKey = "com.unity.importer/ModularImporter/Version";
        private static readonly int DependencyVersion = 1;

        /// <summary>
        /// A set of ids to track for analytics.
        /// <remarks>
        /// Error id 0 is reserved for exception, and error id 1 is reserved for errors from <see cref="GetAnalyticsData"/>.
        /// </remarks>
        /// </summary>
        protected virtual HashSet<int> ErrorIdsToTrack => new();

        /// <summary>
        /// A set of warning ids to track for analytics.
        /// </summary>
        protected virtual HashSet<int> WarningIdsToTrack => new();

        [SerializeField] private LazyLoadReference<ImporterGraph> m_Graph;
        [SerializeReference] private List<IGraphValue> m_ImportSettingOverrides = new();

        [InitializeOnLoadMethod]
        private static void InitializeModularImporterVersion()
        {
            AssetDatabase.RegisterCustomDependency(DependencyKey, Hash128.Compute(DependencyVersion));
        }

        /// <summary>
        /// A project path where the default <see cref="ImporterGraph"/> to use when the asset is imported for the first time and no graph is set yet.
        /// </summary>
        /// <example>
        /// <code>
        /// using UnityEditor.Importer;
        /// [ModularImporter(version: 1, ext: "sphere")]
        /// public class SphereImporter : ModularImporter
        /// {
        ///     protected override ImporterGraph DefaultGraphPath => "Assets/DefaultImporters/SphereImporter.asset";
        /// }
        /// </code>
        /// </example>
        protected virtual string DefaultGraphPath => null;

        /// <summary>
        /// The <see cref="ImporterGraph"/> used by the <see cref="ModularImporter"/> to convert the content of this file into Unity's assets.
        /// </summary>
        public LazyLoadReference<ImporterGraph> Graph
        {
            get => m_Graph;
            set => m_Graph = value;
        }

        /// <summary>
        /// The import setting overrides declared for this importer.
        /// </summary>
        public IReadOnlyList<IGraphValue> ImportSettingOverrides => m_ImportSettingOverrides;

        private void Reset()
        {
            m_Graph = AssetDatabase.LoadAssetAtPath<ImporterGraph>(DefaultGraphPath);
        }

        /// <summary>
        /// This method is called by the Asset pipeline to import files.
        /// </summary>
        /// <remarks>
        /// It is already implemented by the <see cref="ModularImporter"/> class to validate and execute the <see cref="Graph"/>.
        /// If the import is successful, this method will call <see cref="ProcessImportResult"/> with the result of the graph execution.
        /// </remarks>
        /// <param name="ctx">This argument contains all the contextual information needed to process the import event and is also used by the custom importer to store the resulting Unity Asset.</param>
        public override void OnImportAsset(AssetImportContext ctx)
        {
            ctx.DependsOnCustomDependency(DependencyKey);

            var importerGraph = m_Graph.asset;
            if (importSettingsMissing && importerGraph == null)
            {
                importerGraph = AssetDatabase.LoadAssetAtPath<ImporterGraph>(DefaultGraphPath);
                m_Graph = importerGraph;
            }

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m_Graph, out var stringGuid, out _)
                && GUID.TryParse(stringGuid, out var guid)
                && !guid.Empty())
            {
                ctx.DependsOnArtifact(guid);
                // Force loading the asset here because the AssetDatabase will refuse the dependency otherwise.
                AssetDatabase.LoadAssetAtPath<ImporterGraph>(AssetDatabase.GUIDToAssetPath(guid));
            }

            if (importerGraph == null)
            {
                ctx.LogImportError("Unable to find an ImporterGraph to import this asset.");
                return;
            }

            if (!Validate().IsValid)
            {
                return;
            }

            if (!importerGraph.Validate().IsValid)
            {
                return;
            }
            // start the timer to measure the import duration
            var durationTimer = Stopwatch.StartNew();

            AddGraphNodeDependencies(importerGraph, ctx);

            var tempGraph = Instantiate(importerGraph);

            var overrideSettings = m_ImportSettingOverrides.ConvertLazyLoadObjectToObject(out var overrideIds, out var instanceIds);
            RegisterReferencesDependencies(ctx, tempGraph, overrideIds, instanceIds);

            var graphLogger = new EditorGraphLogger(ctx);
            graphLogger.SetErrorLogsToTrack(ErrorIdsToTrack);
            graphLogger.SetWarningLogsToTrack(WarningIdsToTrack);

            var result = GraphRunner.Run(tempGraph, new EditorAssetLoading(ctx), graphLogger, overrideSettings);
            if (result.IsSuccess)
            {
                ProcessImportResult(result.Results, ctx);
            }
            else
            {
                graphLogger.SetException(result.ExecutionException);
            }
            // store the elapsed import time in milliseconds in the graph logger
            graphLogger.RegisterAnalyticsData("importDurationMs", durationTimer.Elapsed.TotalMilliseconds);
            OnFinishingAssetImport(graphLogger);

            DestroyImmediate(tempGraph);
        }

        private static void RegisterReferencesDependencies(AssetImportContext ctx, ImporterGraph tempGraph, HashSet<string> overrideIds,
            HashSet<UnityEngine.Object> instanceIds)
        {
            using var so = new SerializedObject(tempGraph);
            using var prop = so.FindProperty("m_ImportSettings");
            using var end = prop.Copy();
            end.Next(false);
            var match = new Regex(@"^m_ImportSettings.Array.data\[[0-9]+\]");
            while (prop.Next(true) && prop.propertyPath != end.propertyPath)
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    var path = match.Match(prop.propertyPath).Value;
                    using var graphValueProp = so.FindProperty(path);
                    var id = ((IGraphValue)graphValueProp.managedReferenceValue).Id;
                    if (overrideIds.Contains(id))
                        continue;

                    // Unity 6.6: retain full object identity instead of a legacy integer ID.
                    instanceIds.Add(prop.objectReferenceValue);
                }
            }

            foreach (var instanceId in instanceIds)
            {
                ctx.DependsOnArtifact(instanceId);
            }
        }

        private void AddGraphNodeDependencies(ImporterGraph graph, AssetImportContext ctx)
        {
            var nodeTypes = graph.Nodes.Where(n => n != null).Select(n => n.GetType()).Distinct();
            foreach (var type in nodeTypes)
            {
                ctx.DependsOnCustomDependency(NodeRegistration.GetNodeTypeDependencyKey(type));
            }
        }

        /// <summary>
        /// This method is called at the end of the import if the <see cref="Graph"/> execution was a success.
        /// </summary>
        /// <remarks>
        /// The default implementation looks for <see cref="UnityEngine.Object"/> to add them in the <see cref="AssetImportContext"/> as resulting assets of the import.
        /// The following <see cref="ImportResult{T}"/> types are supported:
        /// - <see cref="UnityEngine.Object"/>
        /// - <see cref="UnityEngine.Object"/>[]
        /// - List&lt;<see cref="UnityEngine.Object"/>&gt;
        /// - Dictionary&lt;string, <see cref="UnityEngine.Object"/>&gt;
        /// </remarks>
        /// <param name="results">A readonly list of <see cref="ImportResult{T}"/> that get generated from the <see cref="Graph"/> execution.</param>
        /// <param name="ctx">This argument contains all the contextual information needed to process the import event and is also used by the custom importer to store the resulting Unity Asset.</param>
        protected virtual void ProcessImportResult(IReadOnlyList<IGraphValue> results, AssetImportContext ctx)
        {
            foreach (var value in results)
            {
                if (value.Value == null)
                {
                    continue;
                }

                if (typeof(Object).IsAssignableFrom(value.Type))
                {
                    AddObjectToAssetFromObject(value, ctx);
                }
                else if (value.Type.IsArray && typeof(Object).IsAssignableFrom(value.Type.GetElementType()))
                {
                    AddObjectToAssetFromArray(value, ctx);
                }
                else if (value.Type.IsGenericType
                         && value.Type.GetGenericTypeDefinition() == typeof(List<>)
                         && typeof(Object).IsAssignableFrom(value.Type.GetGenericArguments()[0]))
                {
                    AddObjectToAssetFromList(value, ctx);
                }
                else if (value.Type.IsGenericType
                         && value.Type.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                         && value.Type.GetGenericArguments()[0] == typeof(string)
                         && typeof(Object).IsAssignableFrom(value.Type.GetGenericArguments()[1]))
                {
                    AddObjectToAssetFromDictionary(value, ctx);
                }
            }
        }

        /// <summary>
        /// This method is called at the end of the import.
        /// </summary>
        /// <param name="graphLogger">The graph logger.</param>
        protected virtual void OnFinishingAssetImport(GraphLogger graphLogger) {}

        private void AddObjectToAssetFromObject(IGraphValue result, AssetImportContext ctx)
        {
            ctx.AddObjectToAsset(result.Id, (Object)result.Value);
        }

        private void AddObjectToAssetFromArray(IGraphValue result, AssetImportContext ctx)
        {
            var values = (object[])result.Value;
            for (var index = 0; index < values.Length; index++)
            {
                if (values[index] == null)
                {
                    continue;
                }
                ctx.AddObjectToAsset($"{result.Id}_{index}", (Object)values[index]);
            }
        }

        private void AddObjectToAssetFromList(IGraphValue result, AssetImportContext ctx)
        {
            var list = (IList)result.Value;
            for (var index = 0; index < list.Count; index++)
            {
                var obj = list[index];
                if (obj == null)
                {
                    continue;
                }
                ctx.AddObjectToAsset($"{result.Id}_{index}", (Object)obj);
            }
        }

        private void AddObjectToAssetFromDictionary(IGraphValue result, AssetImportContext ctx)
        {
            var dictionary = (IDictionary)result.Value;
            foreach (var o in dictionary)
            {
                var pair = (DictionaryEntry)o;
                if (pair.Value == null)
                {
                    continue;
                }
                ctx.AddObjectToAsset($"{result.Id}_{(string)pair.Key}", (Object)pair.Value);
            }
        }

        /// <summary>
        /// Add an <see cref="ImportSetting{T}"/> to the import setting overrides.
        /// </summary>
        /// <param name="setting">An <see cref="ImportSetting{T}"/> to add.</param>
        /// <returns>The <see cref="ModularImporterValidationResult"/> of this command.</returns>
        public ModularImporterValidationResult AddImportSettingOverride(IGraphValue setting)
        {
            var validator = new ImportSettingOverrideValidator();
            var validationResult = validator.ValidateAddition(setting, this);
            if (validationResult.IsValid)
            {
                m_ImportSettingOverrides.Add(setting);
            }
            else
            {
                validationResult.DisplayErrors();
            }
            return validationResult;
        }

        /// <summary>
        /// Remove an <see cref="ImportSetting{T}"/> from the import setting overrides.
        /// </summary>
        /// <param name="setting">An <see cref="ImportSetting{T}"/> to remove.</param>
        /// <returns>The <see cref="ModularImporterValidationResult"/> of this command.</returns>
        public ModularImporterValidationResult RemoveImportSettingOverride(IGraphValue setting)
        {
            var validator = new ImportSettingOverrideValidator();
            var validationResult = validator.ValidateRemoval(setting, this);
            if (validationResult.IsValid)
            {
                m_ImportSettingOverrides.Remove(setting);
            }
            else
            {
                validationResult.DisplayErrors();
            }
            return validationResult;
        }

        /// <summary>
        /// Validate this <see cref="ModularImporter"/>.
        /// </summary>
        /// <returns>The <see cref="ModularImporterValidationResult"/> of this validation.</returns>
        public ModularImporterValidationResult Validate()
        {
            var validator = new ModularImporterValidator();
            var validationResult = validator.ValidateImporter(this);
            if (!validationResult.IsValid)
            {
                validationResult.DisplayErrors();
            }

            return validationResult;
        }
    }
}
