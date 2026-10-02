using pxr;
using UnityEngine.Importer;
using System.Collections.Generic;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Filter node which outputs a list of prims that are of the given USD <a href="https://graphics.pixar.com/usd/release/api/class_tf_type.html">TfType</a> in a given Usd stage
    /// </summary>
    /// <remarks>
    /// Inputs :
    ///     pxr.primList : Prims to filter the nodes from
    ///     pxr.tfType : TfType filter to apply
    ///
    /// Outputs :
    ///     List&lt;pxr.UsdPrim&gt; : List of prims that are of given TfType
    /// </remarks>
    [NodeMetadata("FilterPrimsByTfTypeNode", 0, description = "Filter a list of prims by the given TfType", displayName = "FilterPrimsByTfTypeNode")]
    public class FilterPrimsByTfTypeNode : Node<FilterPrimsByTfTypeNode.InputPort, FilterPrimsByTfTypeNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="FilterPrimsByTfTypeNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// Prims to filter the nodes from
            /// </summary>
            public List<UsdPrim> primList;

            /// <summary>
            /// TfType filter to apply
            /// </summary>
            public TfType tfType;
        }

        /// <summary>
        /// Output ports of the <see cref="FilterPrimsByTfTypeNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// List of prims that are of given TfType
            /// </summary>
            public List<UsdPrim> primList = new List<UsdPrim>();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            foreach (var prim in Input.primList)
            {
                if (prim.IsA(Input.tfType))
                {
                    Output.primList.Add(prim);
                }
            }
        }
    }

    /// <summary>
    /// Filter node which outputs a list of prims that are of the given USD <a href="https://graphics.pixar.com/usd/release/api/class_tf_type.html">TfType</a> in given list of prims
    /// </summary>
    /// <remarks>
    /// Inputs :
    ///     List&lt;pxr.UsdPrim&gt; : Stage to filter the nodes from
    ///     pxr.TfType : TfType filter to apply
    ///
    /// Outputs :
    ///     List&lt;pxr.UsdPrim&gt; : List of prims that are of the given TfType
    /// </remarks>
    [NodeMetadata("FilterStageByTfTypeNode", 0, description = "Filter a stage by prims with the given TfType", displayName = "FilterStageByTfTypeNode")]
    public class FilterStageByTfTypeNode : Node<FilterStageByTfTypeNode.InputPort, FilterStageByTfTypeNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="FilterStageByTfTypeNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// Stage to filter the nodes from
            /// </summary>
            public UsdStage stage;

            /// <summary>
            /// TfType filter to apply
            /// </summary>
            public TfType tfType;
        }

        /// <summary>
        /// Output ports of the <see cref="FilterStageByTfTypeNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// List of prims that are of the given TfType
            /// </summary>
            public List<UsdPrim> primList = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            var prims = Input.stage.Traverse(Usd.UsdTraverseInstanceProxies());
            foreach (var prim in prims)
            {
                if (prim.IsA(Input.tfType))
                {
                    Output.primList.Add(prim);
                }
            }
        }
    }
}
