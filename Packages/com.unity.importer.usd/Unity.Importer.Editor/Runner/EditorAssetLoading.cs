using UnityEditor.AssetImporters;
using UnityEngine.Importer;

namespace UnityEditor.Importer
{
    /// <summary>
    /// An asset loading wrapper provided to each nodes of a graph during an import at editor time.
    /// </summary>
    public sealed class EditorAssetLoading : AssetLoading
    {
        private readonly AssetImportContext m_assetImportContext;

        /// <summary>
        /// An asset loading wrapper provided to each nodes of a graph during an import at editor time.
        /// </summary>
        /// <param name="assetImportContext">The AssetImportContext of the current import (provided by a <see cref="ScriptedImporter"/>).</param>
        public EditorAssetLoading(AssetImportContext assetImportContext)
        {
            m_assetImportContext = assetImportContext;
        }

        /// <inheritdoc cref="AssetLoading.AssetPath"/>
        public override string AssetPath => m_assetImportContext.assetPath;

        /// <inheritdoc cref="AssetLoading.DependsOnSourceAsset"/>
        public override void DependsOnSourceAsset(string filePath)
        {
            m_assetImportContext.DependsOnSourceAsset(filePath);
        }

        /// <inheritdoc cref="AssetLoading.GetAssetAtPath{T}"/>
        public override T GetAssetAtPath<T>(string filePath)
        {
            m_assetImportContext.DependsOnArtifact(filePath);
            return AssetDatabase.LoadAssetAtPath<T>(filePath);
        }
    }
}
