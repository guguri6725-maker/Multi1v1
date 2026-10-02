using System.Collections.Generic;
using System.IO;
using Unity.Importer.USD;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Importer;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// Modular Importer that explicitly handle usd, usda, usdc and usdz format in Unity.
    /// </summary>
    [ScriptedImporter(2, new[] {"usd", "usda", "usdc", "usdz"}, new string[] {}, 100, AllowCaching = false)]
    public class UsdModularImporter : ModularImporter
    {
        /// <summary>
        /// Use this setting on usd root composition to let the import know it should process this file as a USDStage
        /// and generate the assets from the composition it contains.
        /// </summary>
        /// <remarks>
        /// - Usdz files are automatically imported and this setting doesn't have any effect on assets with this extension.
        /// - Non imported assets (isUsdRoot == false) will generate a <see cref="DefaultUSD"/> asset.
        /// </remarks>
        public bool isUsdRoot = false;

        /// <summary>
        /// The path to the default USD ImporterGraph made by Unity and used by any newly imported asset.
        /// </summary>
        protected override string DefaultGraphPath => "Packages/com.unity.importer.usd/Unity.Importer.USD.Editor/ImportGraph/usdImporter.asset";

        /// <summary>
        /// A set of USD error ids to track for analytics.
        /// </summary>
        protected override HashSet<int> ErrorIdsToTrack => new(NodeErrors.errorIdToDescriptions.Keys);

        /// <summary>
        /// A set of USD warning ids to track for analytics.
        /// </summary>
        protected override HashSet<int> WarningIdsToTrack => new(NodeWarnings.warningIdToDescriptions.Keys);

        /// <inheritdoc cref="ModularImporter.OnImportAsset"/>
        public override void OnImportAsset(AssetImportContext ctx)
        {
            bool isSelfContained = Path.GetExtension(ctx.assetPath) == ".usdz";
            if (isSelfContained || isUsdRoot)
            {
                base.OnImportAsset(ctx);
            }
            else
            {
                if (importSettingsMissing && Graph.asset == null)
                {
                    Graph = AssetDatabase.LoadAssetAtPath<ImporterGraph>(DefaultGraphPath);
                }
                ctx.AddObjectToAsset("default", ScriptableObject.CreateInstance<DefaultUSD>());
            }
        }

        protected override void OnFinishingAssetImport(GraphLogger graphLogger)
        {
            base.OnFinishingAssetImport(graphLogger);

            var editorAssetLoadingLog = graphLogger as EditorGraphLogger;

            if (!EditorAnalytics.enabled || editorAssetLoadingLog == null)
                return;
            UsdModularImporterAnalytics.SendEvent(editorAssetLoadingLog);
        }
    }
}
