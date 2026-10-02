namespace UnityEngine.Importer
{
    /// <summary>
    /// A class for the asset loading wrapper provided to a <see cref="GraphRunner"/>.
    /// </summary>
    public abstract class AssetLoading
    {
        /// <summary>
        /// The path of the imported file.
        /// </summary>
        public virtual string AssetPath => string.Empty;

        /// <summary>
        /// Declares a dependency to an other file in the project.
        /// </summary>
        /// <remarks>
        /// Changing, removing, or adding a file at that path in the project will re-import the asset declaring that dependency.
        /// <seealso cref="UnityEditor.AssetImporters.AssetImportContext"/>.
        /// </remarks>
        /// <param name="filePath">The path of the file to depends on. It can be a path in the Assets folder or any Package.</param>
        public virtual void DependsOnSourceAsset(string filePath) {}

        /// <summary>
        /// Loads another asset at the given path and adds a dependency to the loaded artifact.
        /// </summary>
        /// <remarks>
        /// If the imported artifact at the given path is changed, this will re-import the asset loading it.
        /// <seealso cref="UnityEditor.AssetImporters.AssetImportContext"/>.
        /// </remarks>
        /// <param name="filePath">The path of the file generating the artifacts to depend on. It can be a path in the Assets folder or any Package.</param>
        /// <typeparam name="T">The type of asset to load.</typeparam>
        /// <returns>The Asset instance at the given <paramref name="filePath"/>.</returns>
        public virtual T GetAssetAtPath<T>(string filePath) where T : Object
        {
            return null;
        }

        internal AssetLoading()
        {
        }
    }
}
