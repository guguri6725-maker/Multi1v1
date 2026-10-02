using UnityEngine;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// Empty class used by non-imported USD files.
    /// </summary>
    /// <remarks>
    /// usd, usda and usdc files do not generate Assets by default in Unity.
    /// It is the user choice to tell the <see cref="UsdModularImporter"/> to process a given usd composition into actual assets.
    /// The import for any file can be enable either from the inspector or setting <see cref="UsdModularImporter.isUsdRoot"/> to true on any given usd asset.
    ///
    /// It is important to note that Usdz files are always imported because they are self contained and state explicitly what is the root of the stage.
    /// </remarks>
    public class DefaultUSD : ScriptableObject
    {
    }
}
