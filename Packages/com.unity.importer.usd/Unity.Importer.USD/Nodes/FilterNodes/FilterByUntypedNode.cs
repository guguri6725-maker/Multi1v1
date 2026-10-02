using pxr;
using UnityEngine.Importer;
using System.Collections.Generic;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Filter node which outputs a list of prims that dont have a concrete UsdType in a given Usd stage.
    /// E.g. def primNode { }
    /// </summary>
    /// <remarks>
    /// Inputs :
    ///     pxr.UsdStage : Stage to filter the nodes from
    ///
    /// Outputs :
    ///     List&lt;pxr.UsdPrim&gt; : List of prims that dont have a type
    /// </remarks>
    [NodeMetadata("FilterStageByUntypedNode", 0, description = "Filter a stage for prims with no TfTypes", displayName = "FilterStageByUntypedNode")]
    public class FilterStageByUntypedNode : Node<FilterStageByUntypedNode.InputPort, FilterStageByUntypedNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="FilterStageByUntypedNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// Stage to filter the nodes from
            /// </summary>
            public UsdStage stage;
        }

        /// <summary>
        /// Output ports of the <see cref="FilterStageByUntypedNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// List of prims that dont have a type
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
                if (prim.GetTypeName().IsEmpty())
                {
                    Output.primList.Add(prim);
                }
            }
        }
    }

    /// <summary>
    /// Filter node which outputs a list of prims that dont have a concrete UsdType in given a list of prims
    /// </summary>
    /// <remarks>
    /// Inputs :
    ///     List&lt;pxr.UsdPrim&gt; : List of prims to filter from
    ///
    /// Outputs :
    ///     List&lt;pxr.UsdPrim&gt; : List of prims that dont have a type
    /// </remarks>
    [NodeMetadata("FilterPrimsByUntypedNode", 0, description = "Filter a list of prims with no TfTypes", displayName = "FilterPrimsByUntypedNode")]
    public class FilterPrimsByUntypedNode : Node<FilterPrimsByUntypedNode.InputPort, FilterPrimsByUntypedNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="FilterPrimsByUntypedNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// List of prims to filter from
            /// </summary>
            public List<UsdPrim> primList;
        }

        /// <summary>
        /// Output ports of the <see cref="FilterPrimsByUntypedNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// List of prims that dont have a type
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
                if (prim.GetTypeName().IsEmpty())
                {
                    Output.primList.Add(prim);
                }
            }
        }
    }
}
