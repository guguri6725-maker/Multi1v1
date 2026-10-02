using System.Collections.Generic;
using pxr;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will filter mesh based on whether they need to be skinned or not.
    /// </summary>
    [NodeMetadata("FilterMeshTypeNode", 1)]
    public class FilterMeshTypeNode : Node<FilterMeshTypeNode.InputPort, FilterMeshTypeNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="FilterMeshTypeNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// List of USD meshes to filter. This can include mesh prims that are USD references.
            /// </summary>
            public List<UsdPrim> meshPrims;

            /// <summary>
            /// A mapping of mesh Prim paths to blend shapes prims.
            /// </summary>
            public Dictionary<string, UsdSkelBlendShape[]> usdSkelBlendShapes;

            /// <summary>
            /// A mapping of mesh Prim paths to skinned mesh data.
            /// </summary>
            public Dictionary<string, UsdSkinnedMeshData> skinnedMeshData = new();
        }

        /// <summary>
        /// Output ports of the <see cref="FilterMeshTypeNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// The USD meshes that need a SkinnedMeshRenderer.
            /// </summary>
            public List<UsdPrim> skinnedMeshPrim = new();

            /// <summary>
            /// The USD meshes that need a MeshRenderer and MeshFilter.
            /// </summary>
            public List<UsdPrim> defaultMeshPrim = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            foreach (var meshPrim in Input.meshPrims)
            {
                var meshPath = meshPrim.GetPath().ToString();
                if (Input.usdSkelBlendShapes.ContainsKey(meshPath) || Input.skinnedMeshData.ContainsKey(meshPath))
                {
                    Output.skinnedMeshPrim.Add(meshPrim);
                }
                else
                {
                    Output.defaultMeshPrim.Add(meshPrim);
                }
            }
        }
    }
}
