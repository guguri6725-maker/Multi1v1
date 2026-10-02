using pxr;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Get the USD <a href="https://graphics.pixar.com/usd/release/api/class_tf_type.html">TfType </a> with the given name
    /// </summary>
    /// <remarks>
    /// Inputs :
    ///     string : TfType name
    ///
    /// Outputs :
    ///     pxr.TfType : Matching TfType.
    /// </remarks>
    [NodeMetadata("GetTfTypeByNameNode", 0, description = "Retrieve the USD TfType", displayName = "GetTfTypeByNameNode")]
    public class GetTfTypeByNameNode : Node<GetTfTypeByNameNode.InputPort, GetTfTypeByNameNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="GetTfTypeByNameNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// TfType name
            /// </summary>
            public string tfTypeName;
        }

        /// <summary>
        /// Output ports of the <see cref="GetTfTypeByNameNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// Matching TfType
            /// </summary>
            public TfType tfType;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.tfType = TfType.FindByName(Input.tfTypeName);
        }
    }
}
