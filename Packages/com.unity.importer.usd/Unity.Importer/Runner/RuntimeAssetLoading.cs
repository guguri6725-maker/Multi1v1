namespace UnityEngine.Importer
{
    /// <summary>
    /// An asset loading wrapper provided to each nodes of a graph during an import at runtime.
    /// </summary>
    public sealed class RuntimeAssetLoading : AssetLoading
    {
        /// <inheritdoc cref="AssetLoading.AssetPath"/>
        public override string AssetPath { get; }

        /// <summary>
        /// An asset loading wrapper provided to each nodes of a graph during an import at runtime.
        /// </summary>
        /// <param name="assetPath">The path of the imported file.</param>
        public RuntimeAssetLoading(string assetPath)
        {
            AssetPath = assetPath;
        }
    }
}
