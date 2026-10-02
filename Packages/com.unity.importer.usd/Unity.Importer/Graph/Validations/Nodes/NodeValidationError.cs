using System;

namespace UnityEngine.Importer.Validations
{
    /// <summary>
    /// An error created during a graph validation or a new node addition to a graph.
    /// </summary>
    public class NodeValidationError : BaseGraphValidationError
    {
        /// <summary>
        /// The invalid <see cref="INode{TInput,TOutput}"/>.
        /// </summary>
        public INode<InputPorts, OutputPorts> Node { get; }

        /// <summary>
        /// <inheritdoc cref="NodeValidationError"/>
        /// </summary>
        /// <param name="node">The invalid <see cref="INode{TInput,TOutput}"/>.</param>
        /// <param name="message">The error message.</param>
        /// <param name="validationType">The Validation type that triggerred this error.</param>
        internal NodeValidationError(INode<InputPorts, OutputPorts> node, string message, Type validationType) : base(message, validationType)
        {
            Node = node;
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"Node {(Node != null ? Node : "Null-Node")} : {Message}";
        }
    }
}
