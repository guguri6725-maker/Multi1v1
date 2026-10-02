using System.Collections.Generic;
using pxr;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will create empty GameObjects in the hierarchy, at the path of the provided prims.
    /// </summary>
    [NodeMetadata("CreateEmptyObjectsNode", 1)]
    public class CreateEmptyObjectsNode : Node<CreateEmptyObjectsNode.InputPort, CreateEmptyObjectsNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="CreateEmptyObjectsNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// A list of prims to convert to empty GameObjects.
            /// </summary>
            public List<UsdPrim> prims;
        }

        /// <summary>
        /// Output ports of the <see cref="CreateEmptyObjectsNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-GameObjects mapping, resulting from the conversion.
            /// </summary>
            public Dictionary<string, GameObject> gameObjects;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.gameObjects = new Dictionary<string, GameObject>(Input.prims.Count);
            foreach (var prim in Input.prims)
            {
                Output.gameObjects.Add(prim.GetPath(), new GameObject(prim.GetName()));
            }
        }
    }
}
