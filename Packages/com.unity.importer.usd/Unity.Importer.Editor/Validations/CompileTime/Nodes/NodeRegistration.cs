using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Importer;

namespace UnityEditor.Importer.NodeDeclaration.Validation
{
    internal class NodeRegistration : AssetPostprocessor
    {
        private const string dependencyPrefix = "com.unity.importer/ModularImporterNode";

        /// <summary>
        /// InitializeOnLoadMethod is required to avoid the AssetDatabase from putting all the usd in the import queue with empty dependencies for the nodes.
        /// </summary>
        [InitializeOnLoadMethod]
        static void RegisterAllNodesDependencies()
        {
            var nodeTypes = GetAllNodeTypes();
            NodeMetadataCache.Cache = ValidateNodes(nodeTypes, true).NodeMetadata;

            AssetDatabase.StartAssetEditing();
            AssetDatabase.UnregisterCustomDependencyPrefixFilter($"{dependencyPrefix}/");
            foreach (var kvp in NodeMetadataCache.Cache)
            {
                AssetDatabase.RegisterCustomDependency(GetNodeTypeDependencyKey(kvp.Key), Hash128.Compute(kvp.Value.version));
            }
            AssetDatabase.StopAssetEditing();
        }

        /// <summary>
        /// We need OnPostprocessAllAssets to make sure RegisterAllNodesDependencies is called at least once when the AssetDatabase is in a ready state.
        /// </summary>
        private static void OnPostprocessAllAssets(
            string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths,
            bool didDomainReload)
        {
            if (didDomainReload)
            {
                RegisterAllNodesDependencies();
            }
        }

        internal static string GetNodeTypeDependencyKey(Type type) => $"{dependencyPrefix}/{type.FullName}";

        private static List<Type> GetAllNodeTypes()
        {
            return TypeCache.GetTypesDerivedFrom<INodeSerialization>()
                .Union(TypeCache.GetTypesWithAttribute<NodeMetadataAttribute>())
                .Distinct()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .ToList();
        }

        internal static NodeDeclarationValidationResult ValidateNodes(List<Type> nodeTypes, bool displayErrors)
        {
            var context = new NodeDeclarationValidationContext(nodeTypes);
            var validations = GetNodeValidations();
            foreach (var validation in validations)
            {
                validation.Setup(context);
            }

            foreach (var type in nodeTypes)
            {
                foreach (var validation in validations)
                {
                    validation.Validate(type, context);
                }
            }

            var result = new NodeDeclarationValidationResult(context);
            if (displayErrors)
            {
                result.DisplayErrors();
            }

            return result;
        }

        private static List<BaseNodeDeclarationValidation> GetNodeValidations()
        {
            return new List<BaseNodeDeclarationValidation>
            {
                new ImplementINodeValidation(),
                new MissingAttributeValidation(),
                new NoNodeCollisionValidation()
            };
        }

        private static void DisplayErrors(List<NodeDeclarationError> errors)
        {
            foreach (var error in errors)
            {
                Debug.LogError(error);
            }
        }
    }
}
