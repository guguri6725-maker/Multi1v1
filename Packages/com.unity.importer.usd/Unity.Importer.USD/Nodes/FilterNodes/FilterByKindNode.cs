using pxr;
using UnityEngine.Importer;
using System.Collections.Generic;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Filter node which outputs a list of prims that are of the given USD <a href="https://graphics.pixar.com/usd/dev/api/kind_page_front.html#kind_coreKinds">Kind</a> in a given Usd stage
    /// </summary>
    /// <remarks>
    /// Inputs :
    ///     pxr.UsdStage : Stage to filter the nodes from
    ///     pxr.TfToken : Kind of prim to filter nodes from
    ///
    /// Outputs :
    ///     List&lt;pxr.UsdPrim&gt; : List of prims that are of given Kind
    /// </remarks>
    [NodeMetadata("FilterStageByKindNode", 0, description = "Filter a stage by prims by the given Kind", displayName = "FilterStageByKindNode")]
    public class FilterStageByKindNode : Node<FilterStageByKindNode.InputPort, FilterStageByKindNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="FilterStageByKindNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// The USD stage
            /// </summary>
            public UsdStage stage;

            /// <summary>
            /// Kind of prim to filter nodes from
            /// </summary>
            public TfToken kind;
        }

        /// <summary>
        /// Output ports of the <see cref="FilterStageByKindNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// List of prims that are of given Kind
            /// </summary>
            public List<UsdPrim> primList = new List<UsdPrim>();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            var prims = Input.stage.Traverse(Usd.UsdTraverseInstanceProxies());
            foreach (var prim in prims)
            {
                var modelAPI = new UsdModelAPI(prim);
                TfToken primKind = new TfToken();
                modelAPI.GetKind(primKind);
                if (primKind == Input.kind)
                {
                    Output.primList.Add(prim);
                }
            }
        }
    }

    /// <summary>
    /// Filter node which outputs a list of prims that are of the given USD <a href="https://graphics.pixar.com/usd/dev/api/kind_page_front.html#kind_coreKinds">Kind</a> in given a list of prims
    /// </summary>
    /// <remarks>
    /// Inputs :
    ///     List&lt;pxr.UsdPrim&gt; : List of prims to filter from
    ///     pxr.TfToken : Kind of prim to filter nodes from
    ///
    /// Outputs :
    ///     List&lt;pxr.UsdPrim&gt; : List of prims that are of the given Kind
    /// </remarks>
    [NodeMetadata("FilterPrimsByKindNode", 0, description = "Filter a list of prims by the given Kind", displayName = "FilterPrimsByKindNode")]
    public class FilterPrimsByKindNode : Node<FilterPrimsByKindNode.InputPort, FilterPrimsByKindNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="FilterPrimsByKindNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// List of prims to filter from
            /// </summary>
            public List<UsdPrim> primList;

            /// <summary>
            /// Kind of prim to filter nodes from
            /// </summary>
            public TfToken kind;
        }

        /// <summary>
        /// Output ports of the <see cref="FilterPrimsByKindNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// List of prims that are of the given Kind
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
                var modelAPI = new UsdModelAPI(prim);
                TfToken primKind = new TfToken();
                modelAPI.GetKind(primKind);
                if (primKind == Input.kind)
                {
                    Output.primList.Add(prim);
                }
            }
        }
    }
}
