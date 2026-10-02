using System.Collections.Generic;
using pxr;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will parse the given prims and output a mapping of prim path to their Master prim path.
    /// If the prim is not a proxy and a master prim does not exist, the prim path will be mapped with its own path.
    /// </summary>
    [NodeMetadata("ReadReferencesNode", 1)]
    public class ReadReferencesNode : Node<ReadReferencesNode.InputPort, ReadReferencesNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadReferencesNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// Usd prims to parse for any related Master prim path.
            /// </summary>
            public List<UsdPrim> prims;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadReferencesNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A mapping of prim path to their Master prim path, if any.
            /// </summary>
            public Dictionary<string, string> objsToRefs;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.objsToRefs = new Dictionary<string, string>();
            foreach (var prim in Input.prims)
            {
                if (prim.IsInstanceProxy())
                {
                    Output.objsToRefs.Add(prim.GetPath(), prim.GetPrimInPrototype().GetPath());
                }
                else
                {
                    Output.objsToRefs.Add(prim.GetPath(), prim.GetPath());
                }
            }
        }
    }
}
