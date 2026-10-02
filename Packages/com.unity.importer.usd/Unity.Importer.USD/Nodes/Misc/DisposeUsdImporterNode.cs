using pxr;
using System;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node ensure that an USD stage used during
    /// </summary>
    [NodeMetadata("DisposeUsdImporterNode", 1)]
    public class DisposeUsdImporterNode : Node<DisposeUsdImporterNode.InputPort, DisposeUsdImporterNode.OutputPort>
    {
        private const string baseFieldName = "gameObjects_";

        /// <summary>
        /// Input ports of the <see cref="DisposeUsdImporterNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// The root of our imported GameObject hierarchy.
            /// </summary>
            public GameObject root;

            /// <summary>
            /// A USD stage to dispose off.
            /// </summary>
            public UsdStage stage;
        }

        /// <summary>
        /// Output ports of the <see cref="DisposeUsdImporterNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// The root of our imported GameObject hierarchy.
            /// </summary>
            public GameObject root;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.root = Input.root;
            Input.stage.Unload();
            Input.stage.Dispose();
        }
    }
}
