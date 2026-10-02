using System.Collections.Generic;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// Structure encapsulating the skinning query data that is required by animation nodes.
    /// UsdSkelSkinningQuery objects have a limited lifetime bound to the UsdSkelCache object they come from
    /// We cache their data in order to use it later in the graph without having to move the whole UsdSkelCache
    /// object around
    /// </summary>
    public class SkinningQueryData
    {
        /// <summary>
        /// The mesh prim path
        /// </summary>
        public string meshPath;

        /// <summary>
        /// The blend shape token -> blend shape target relationship
        /// </summary>
        public Dictionary<string, string> blendShapeTokenToTarget;
    }
}
