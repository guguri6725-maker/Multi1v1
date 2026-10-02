using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using pxr;
using Unity.USD.Core;
using UnityEngine.Importer;

#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will create a USD stage for the imported USD file.
    /// </summary>
    /// <remarks>
    /// When used in the Unity Editor, this node is also looking for all references the USD stage is using
    /// and will error out if any of them are outside of the Unity project.
    /// If all referenced files are in the project, it'll add a dependency to them
    /// so that the composition stays up to date whenever another file is changed.
    /// </remarks>
    [NodeMetadata("UsdStageOpenNode", 3)]
    public class UsdStageOpenNode : Node<UsdStageOpenNode.InputPort, UsdStageOpenNode.OutputPort>
    {
        private const string UsdExtension = ".usd";
        private const string UsdzExtension = ".usdz";
        private const string k_UsdzRegex = @"(.+)\[.+\]$";

        /// <summary>
        /// Input ports of the <see cref="UsdStageOpenNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
        }

        /// <summary>
        /// Output ports of the <see cref="UsdStageOpenNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// The USD stage of the imported USD file
            /// </summary>
            public UsdStage stage;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            InitUsd.Initialize();

            var usdPath = Path.GetFullPath(Input.AssetLoading.AssetPath);
            Output.stage = UsdStage.Open(usdPath, UsdStage.InitialLoadSet.LoadAll);

#if UNITY_EDITOR
            if (Input.AssetLoading is not RuntimeAssetLoading)
                RegisterUsdReferences(usdPath, Output.stage);
#endif
        }

#if UNITY_EDITOR
        private void RegisterUsdReferences(string usdPath, UsdStage stage)
        {
            SdfLayerRefPtrVector layers = new SdfLayerRefPtrVector();
            StdStringVector assets = new StdStringVector();
            StdStringVector unresolvedPaths = new StdStringVector();

            if (UsdUtils.UsdUtilsComputeAllDependencies(new SdfAssetPath(stage.GetRootLayer().identifier), layers, assets, unresolvedPaths))
            {
                bool anyError = false;
                var rootFolder = Path.GetDirectoryName(usdPath).Replace('\\', '/');
                foreach (var layer in layers)
                {
                    anyError = !TryAddDependencyToReference(rootFolder, layer.GetRealPath()) | anyError;
                    layer.Dispose();
                }

                foreach (var asset in assets)
                {
                    anyError = !TryAddDependencyToReference(rootFolder, asset) | anyError;
                }

                foreach (var unresolved in unresolvedPaths)
                {
                    anyError = !TryAddDependencyToReference(rootFolder, unresolved) | anyError;
                }

                if (anyError)
                {
                    throw new Exception("USD files referencing assets outside of the Unity project are not supported.");
                }
            }

            // analytics
            ulong totalUsdFilesSize = 0;
            ulong totalAssetFilesSize = 0;
            int usdFilesCount = 0;
            int assetFilesCount = 0;
            CollectDccOrigins(layers, out var dccOrigins, out var dccOriginsCounts);
            CollectSchemas(out var usdSchemas, out var usdSchemasCounts);


            var fileExtensionsCount = new Dictionary<string, int>();
            if (Path.GetExtension(usdPath) == UsdzExtension)
            {
                GetUsdAndAssetFilesSizeForUsdZip(usdPath, ref totalUsdFilesSize, ref totalAssetFilesSize, ref usdFilesCount, ref assetFilesCount, ref fileExtensionsCount);
            }
            else
            {
                GetUsdAndAssetFilesSizeForUsdFiles(layers, assets, ref totalUsdFilesSize, ref totalAssetFilesSize, ref usdFilesCount, ref assetFilesCount, ref fileExtensionsCount);
            }

            GetKeyValueArrays(fileExtensionsCount, out var usdFileExtensions, out var usdFileExtensionsCount);
            Input.GraphLogger.RegisterAnalyticsData(AnalyticsDataKeys.DccOrigins, dccOrigins);
            Input.GraphLogger.RegisterAnalyticsData(AnalyticsDataKeys.DccOriginsCounts, dccOriginsCounts);
            Input.GraphLogger.RegisterAnalyticsData(AnalyticsDataKeys.UsdSchemas, usdSchemas);
            Input.GraphLogger.RegisterAnalyticsData(AnalyticsDataKeys.UsdSchemasCounts, usdSchemasCounts);
            Input.GraphLogger.RegisterAnalyticsData(AnalyticsDataKeys.TotalUsdFilesSize, totalUsdFilesSize);
            Input.GraphLogger.RegisterAnalyticsData(AnalyticsDataKeys.TotalAssetFilesSize, totalAssetFilesSize);
            Input.GraphLogger.RegisterAnalyticsData(AnalyticsDataKeys.UsdFilesCount, usdFilesCount);
            Input.GraphLogger.RegisterAnalyticsData(AnalyticsDataKeys.AssetFilesCount, assetFilesCount);
            Input.GraphLogger.RegisterAnalyticsData(AnalyticsDataKeys.UsdFileExtensions, usdFileExtensions);
            Input.GraphLogger.RegisterAnalyticsData(AnalyticsDataKeys.UsdFileExtensionsCount, usdFileExtensionsCount);
            layers.Dispose();
        }

        private bool TryAddDependencyToReference(string assetRoot, string referencePath)
        {
            // FileUtils and AssetDatabase methods doesn't understand \ character as a folder separator.
            var fixedPath = referencePath.Replace('\\', '/');
            try
            {
                // if the path is not rooted, then it may not be resolved and is likely relative to the root asset.
                if (!Path.IsPathRooted(fixedPath))
                {
                    fixedPath = assetRoot + "/" + fixedPath;
                }
            }
            catch (ArgumentException)
            {
                // The path contains illegal characters. A file with that path cannot exist
                // Accept the path but do not add the dependency
                return true;
            }

            var path = FileUtil.GetProjectRelativePath(fixedPath);
            // Maybe the file is in a package and GetLogicalPath will be able to resolve it.
            if (string.IsNullOrEmpty(path))
            {
                path = FileUtil.GetLogicalPath(fixedPath);
            }

            // Getting relative path was unsuccessful, the file is very likely outside of Unity.
            if (string.IsNullOrEmpty(path))
            {
                Input.GraphLogger.LogImportError($"Referenced layer is not in Unity project : {referencePath}",
                    null, NodeErrors.ReferencedLayerNotInUnityProject);
                return false;
            }

            // GetProjectRelativePath returns a valid output for files outside of the Assets folder
            // but still inside the project root. Let's ask the AssetDatabase if the path is valid.
            // Side Note: This is non deterministic behaviour
            // because it relays on the list of Roots known by the AssetDatabase at that point.
            // We should probably also depends on it, but it is very unlikely that
            // adding a cached package will change any of these resolution path.
            if (!AssetDatabase.TryGetAssetFolderInfo(path, out _, out _))
            {
                Input.GraphLogger.LogImportError($"Referenced layer is not in Unity project : {referencePath}",
                    null, NodeErrors.ReferencedLayerNotInUnityProject);
                return false;
            }

            Input.AssetLoading.DependsOnSourceAsset(path);
            return true;
        }

        private void GetUsdAndAssetFilesSizeForUsdZip(string usdZipFilePath, ref ulong totalUsdFilesSize,
            ref ulong totalAssetFilesSize, ref int usdFilesCount, ref int assetFilesCount,
            ref Dictionary<string, int> usdFilesExtensionsCount
        )
        {
            using var usdZipFile = UsdZipFile.Open(usdZipFilePath);
            IncreaseCount(usdFilesExtensionsCount, UsdzExtension);
            var fileNames = usdZipFile.GetFileNames();
            foreach (var fileName in fileNames)
            {
                var extension = Path.GetExtension(fileName);
                if (extension.StartsWith(UsdExtension))
                {
                    // we may have nested USDZ files
                    if (extension == UsdzExtension)
                    {
                        GetUsdAndAssetFilesSizeForUsdZip(fileName, ref totalUsdFilesSize, ref totalAssetFilesSize, ref usdFilesCount, ref assetFilesCount, ref usdFilesExtensionsCount);
                    }
                    else
                    {
                        IncreaseCount(usdFilesExtensionsCount, extension);
                        totalUsdFilesSize += usdZipFile.GetFileInfo(fileName).uncompressedSize;
                        usdFilesCount += 1;
                    }
                }
                else
                {
                    totalAssetFilesSize += usdZipFile.GetFileInfo(fileName).uncompressedSize;
                    assetFilesCount += 1;
                }
            }
        }

        private void GetUsdAndAssetFilesSizeForUsdFiles(SdfLayerRefPtrVector layers, StdStringVector assets,
            ref ulong totalUsdFilesSize, ref ulong totalAssetFilesSize, ref int usdFilesCount, ref int assetFilesCount,
            ref Dictionary<string, int> usdFilesExtensionsCount)
        {
            var usdzRegex = new Regex(k_UsdzRegex);
            var usdzCache = new HashSet<string>();

            foreach (var layer in layers)
            {
                var fileInfo = new FileInfo(layer.GetRealPath());
                var fileSize = fileInfo.Length;
                totalUsdFilesSize += (ulong)fileSize;
                var fileExtension = fileInfo.Extension;
                IncreaseCount(usdFilesExtensionsCount, fileExtension);
                layer.Dispose();
            }

            foreach (var asset in assets)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(asset);
                    totalAssetFilesSize += (ulong)fileInfo.Length;
                }
                catch (Exception e)
                {
                    // USDZ files are unlikely to come down this path, but catch them just in case
                    var match = usdzRegex.Match(asset);
                    if (match.Success)
                    {
                        var filePath = match.Groups[1].Value;
                        if (!usdzCache.Contains(filePath))
                        {
                            GetUsdAndAssetFilesSizeForUsdZip(filePath, ref totalUsdFilesSize, ref totalAssetFilesSize, ref usdFilesCount, ref assetFilesCount, ref usdFilesExtensionsCount);
                            usdzCache.Add(filePath);
                        }
                    }
                }
            }

            usdFilesCount += layers.Count;
            assetFilesCount += assets.Count;
        }

        private void CollectDccOrigins(SdfLayerRefPtrVector layers, out string[] dccOrigins, out int[] dccOriginsCounts)
        {
            var originsCount = new Dictionary<string, int>();
            foreach (var layer in layers)
            {
                var doc = layer.GetDocumentation();
                if (doc.StartsWith("Blender"))
                {
                    IncreaseCount(originsCount, "Blender");
                }

                var customLayerData = layer.GetCustomLayerData();
                var creator = customLayerData.GetValueAtPath("creator");

                if (creator != null)
                {
                    IncreaseCount(originsCount, "3DsMax");
                }
                layer.Dispose();
            }

            GetKeyValueArrays(originsCount, out dccOrigins, out dccOriginsCounts);
        }

        private void CollectSchemas(out string[] usdSchemas, out int[] usdSchemasCount)
        {
            var prims = Output.stage.Traverse(Usd.UsdTraverseInstanceProxies());
            var schemasCount = new Dictionary<string, int>();
            foreach (var prim in prims)
            {
                var schemas = prim.GetAppliedSchemas();

                foreach (var schema in schemas)
                {
                    if (UsdAPISchemas.usdAPISchemasNames.Contains(schema))
                    {
                        IncreaseCount(schemasCount, schema);
                    }
                }
            }

            GetKeyValueArrays(schemasCount, out usdSchemas, out usdSchemasCount);
        }

        private static void IncreaseCount(Dictionary<string, int> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out int count))
            {
                dictionary[key] = count + 1;
            }
            else
            {
                dictionary.Add(key, 1);
            }
        }

        private static void GetKeyValueArrays(Dictionary<string, int> dictionary, out string[] keys, out int[] values)
        {
            keys = new string[dictionary.Count];
            values = new int[dictionary.Count];
            var i = 0;
            foreach (var entry in dictionary)
            {
                keys[i] = entry.Key;
                values[i] = entry.Value;
                i++;
            }
        }

#endif
    }
}
