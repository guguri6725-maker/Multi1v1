using System;

namespace UnityEngine.Importer
{
    /// <summary>
    /// Represents a data flow from an origin <see cref="NodePort"/> to a destination <see cref="NodePort"/>.
    /// </summary>
    /// <remarks>
    /// A valid edge must abide to the following rules :
    /// - origin is an output field of a node in the graph
    /// - destination is an input field of another node in the graph
    /// - origin and destination field are of the same type
    /// - edge is unique
    /// - only one incoming edge per input
    /// </remarks>
    [Serializable]
    public struct Edge : IEquatable<Edge>, IOriginEdge, IDestinationEdge
    {
        /// <summary>
        /// A <see cref="NodePort"/> representing the origin of the Edge.
        /// </summary>
        public NodePort Origin => origin;

        [SerializeField]
        private NodePort origin;

        /// <summary>
        /// A <see cref="NodePort"/> representing the destination of the Edge.
        /// </summary>
        public NodePort Destination => destination;

        [SerializeField]
        private NodePort destination;

        /// <summary>
        /// <inheritdoc cref="Edge"/>
        /// </summary>
        /// <param name="origin">The <see cref="Edge"/> origin <see cref="INode{TInput,TOutput}"/>.</param>
        /// <param name="originFieldName">The <see cref="Edge"/> origin output field on the origin <see cref="INode{TInput,TOutput}"/>.</param>
        /// <param name="destination">The <see cref="Edge"/> destination <see cref="INode{TInput,TOutput}"/>.</param>
        /// <param name="destinationFieldName">The <see cref="Edge"/> destination input field on the destination <see cref="INode{TInput,TOutput}"/>.</param>
        public Edge(INode<InputPorts, OutputPorts> origin, string originFieldName, INode<InputPorts, OutputPorts> destination, string destinationFieldName)
        {
            this.origin = new NodePort(origin, NodePort.NodePortTarget.Output, originFieldName);
            this.destination = new NodePort(destination, NodePort.NodePortTarget.Input, destinationFieldName);
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public bool Equals(Edge other)
        {
            return origin.Equals(other.origin) && destination.Equals(other.destination);
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return obj is Edge other && Equals(other);
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            return HashCode.Combine(origin, destination);
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"{origin} => {destination}";
        }
    }
}
