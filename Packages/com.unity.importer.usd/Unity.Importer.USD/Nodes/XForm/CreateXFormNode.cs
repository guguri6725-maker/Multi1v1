using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will create GameObjects and initialize their transform position, scale and rotation, using the provided <see cref="XFormableDescription"/>.
    /// </summary>
    [NodeMetadata("CreateXFormNode", 1)]
    public class CreateXFormNode : Node<CreateXFormNode.InputPort, CreateXFormNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="CreateXFormNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// A PrimPath-XFormableDescription mapping, read from the usd file.
            /// </summary>
            public Dictionary<string, XFormableDescription> xFormableDescriptions;
        }

        /// <summary>
        /// Output ports for the <see cref="CreateXFormNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-GameObjects mapping, resulting from the conversion.
            /// This mapping will be used when reconstructing the hierarchy.
            /// </summary>
            public Dictionary<string, GameObject> gameObjects;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.gameObjects = new Dictionary<string, GameObject>(Input.xFormableDescriptions.Count);
            foreach (var description in Input.xFormableDescriptions)
            {
                var go = new GameObject(description.Value.name);
                var transform = go.transform;
                transform.localPosition = description.Value.transformData.translation;
                transform.localRotation = description.Value.transformData.rotation;
                transform.localScale = description.Value.transformData.scale;
                Output.gameObjects.Add(description.Key, go);
            }
        }
    }
}
