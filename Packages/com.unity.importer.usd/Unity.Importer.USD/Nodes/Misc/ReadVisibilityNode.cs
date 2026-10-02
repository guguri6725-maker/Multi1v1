using System.Collections.Generic;
using pxr;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will parse the given prims and output a list of invisible primpath
    /// </summary>
    [NodeMetadata("ReadVisibilityNode", 0)]
    public class ReadVisibilityNode : Node<ReadVisibilityNode.InputPort, ReadVisibilityNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadVisibilityNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// Usd prims to parse for visibility.
            /// </summary>
            public List<UsdPrim> prims;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadVisibilityNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A collection of invisible prims' paths
            /// </summary>
            public HashSet<string> invisiblePaths;
        }

        /// <inheritdoc cref="Node{TInput,TOutput}.Run"/>
        public override void Run()
        {
            Output.invisiblePaths = new HashSet<string>(Input.prims.Count);
            var usdGeomImageableTfType = TfType.FindByName("UsdGeomImageable");

            foreach (var prim in Input.prims)
            {
                if (prim.IsA(usdGeomImageableTfType))
                {
                    var invisible = new UsdGeomImageable(prim).ComputeVisibility(UsdTimeCode.EarliestTime()) == UsdGeomTokens.invisible;

                    if (invisible)
                    {
                        Output.invisiblePaths.Add(prim.GetPath());
                    }
                }
                else
                {
                    Output.invisiblePaths.Add(prim.GetPath());
                }
            }
        }
    }
}
