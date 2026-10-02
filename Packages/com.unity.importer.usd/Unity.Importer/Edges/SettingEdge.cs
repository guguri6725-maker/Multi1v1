using System;

namespace UnityEngine.Importer
{
    /// <summary>
    /// A setting edge connects a setting id to an input of any <see cref="Node{TInput,TOutput}"/> in the graph.
    /// </summary>
    /// <remarks>
    /// A valid setting edge must abide to the following rules :
    /// - destination is an input field of a node in the graph
    /// - id is an import setting id in the graph
    /// - import setting and destination field are of the same type
    /// - setting edge is unique
    /// - only one incoming edge/SettingEdge per input
    /// </remarks>
    [Serializable]
    public struct SettingEdge : IEquatable<SettingEdge>, IDestinationEdge
    {
        /// <summary>
        /// The setting id of the <see cref="ImportSetting{T}"/> from which to retrieve the setting's value.
        /// </summary>
        public string Id => id;

        [SerializeField]
        private string id;

        /// <summary>
        /// A <see cref="NodePort"/> representing the destination of the <see cref="SettingEdge"/>.
        /// </summary>
        public NodePort Destination => destination;

        [SerializeField]
        private NodePort destination;

        /// <summary>
        /// <inheritdoc cref="SettingEdge"/>
        /// </summary>
        /// <param name="settingId">The <see cref="ImporterGraph"/> setting's Id that will be injected to an input of a <see cref="INode{TInput,TOutput}"/>.</param>
        /// <param name="destination">The <see cref="SettingEdge"/> destination <see cref="INode{TInput,TOutput}"/>.</param>
        /// <param name="destinationFieldName">The <see cref="SettingEdge"/> destination input field on the destination <see cref="INode{TInput,TOutput}"/>.</param>
        public SettingEdge(string settingId, INode<InputPorts, OutputPorts> destination, string destinationFieldName)
        {
            this.destination = new NodePort(destination, NodePort.NodePortTarget.Input, destinationFieldName);
            id = settingId;
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public bool Equals(SettingEdge other)
        {
            return id == other.id && destination.Equals(other.destination);
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return obj is SettingEdge other && Equals(other);
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            return HashCode.Combine(id, destination);
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"{id} => {destination}";
        }
    }
}
