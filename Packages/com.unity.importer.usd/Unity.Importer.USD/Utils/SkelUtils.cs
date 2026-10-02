using pxr;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Utilities to access Usd skeletal data
    /// </summary>
    public class SkelUtils
    {
        /// <summary>
        /// Retrieves the bindings that are applied within the given skel root prim
        /// </summary>
        public static UsdSkelBindingVector GetSkelBindingsFromCache(UsdPrim skeletonRootPrim, UsdSkelCache skeletonCache, GraphLogger GraphLogger = null)
        {
            var skeletonRoot = new UsdSkelRoot(skeletonRootPrim);

            if (!skeletonCache.Populate(skeletonRoot, Usd.UsdPrimDefaultPredicate))
            {
                if (GraphLogger != null)
                {
                    GraphLogger.LogImportError($"Failed to populate skeleton cache with SkelRoot '{skeletonRoot}'.",
                        null, NodeErrors.CouldNotPopulateSkeletonCache);
                }
                else
                {
                    Debug.LogError($"Failed to populate skeleton cache with SkelRoot '{skeletonRoot}'.");
                }
                return null;
            }

            var bindings = new UsdSkelBindingVector();
            if (!skeletonCache.ComputeSkelBindings(skeletonRoot, bindings, Usd.UsdPrimDefaultPredicate))
            {
                if (GraphLogger != null)
                {
                    GraphLogger.LogImportError($"Could not compute binding for SkelRoot '{skeletonRoot}'.", null,
                        NodeErrors.CouldNotComputeBindingForSkelRoot);
                }
                else
                {
                    Debug.LogError($"Could not compute binding for SkelRoot '{skeletonRoot}'.");
                }
                return null;
            }

            return bindings;
        }
    }
}
