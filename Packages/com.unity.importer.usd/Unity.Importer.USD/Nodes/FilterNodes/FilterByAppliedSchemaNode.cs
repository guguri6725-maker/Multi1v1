using System.Collections.Generic;
using pxr;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Filter node which outputs a list of prims that have the given schema applied in a given Usd stage.
    /// </summary>
    /// <remarks>
    /// Inputs :
    ///     pxr.UsdStage : Stage to filter the nodes from
    ///     pxr.schemaName : The name of the schema to look for, as returned by UsdPrim.GetAppliedSchemas()
    ///
    /// Outputs :
    ///     List&lt;pxr.UsdPrim&gt; : List of prims that have the schema applied
    /// </remarks>
    [NodeMetadata("FilterStageByAppliedSchemaNode", 0, description = "Filter a list of prims that have the given schema applied", displayName = "FilterStageByAppliedSchemaNode")]
    public class FilterStageByAppliedSchemaNode : Node<FilterStageByAppliedSchemaNode.InputPort, FilterStageByAppliedSchemaNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="FilterStageByAppliedSchemaNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// The USD stage
            /// </summary>
            public UsdStage stage;

            /// <summary>
            /// The name of the schema to look for
            /// </summary>
            public string schemaName;
        }

        /// <summary>
        /// Output ports of the <see cref="FilterStageByAppliedSchemaNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// List of prims that have the schema applied
            /// </summary>
            public List<UsdPrim> primList = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            var schemaTypeName = Input.schemaName;
            var primIterator = Input.stage.Traverse();
            var schemaToken = new TfToken(schemaTypeName);
            foreach (var prim in primIterator)
            {
                if (prim.HasAPI(schemaToken))
                    Output.primList.Add(prim);
            }
        }
    }
}
