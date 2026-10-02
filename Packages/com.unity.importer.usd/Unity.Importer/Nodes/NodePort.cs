using System;
using System.Reflection;

namespace UnityEngine.Importer
{
    /// <summary>
    /// Represents a <see cref="INode{TInput,TOutput}"/> port (combination of a node instance and a field name).
    /// </summary>
    [Serializable]
    public struct NodePort : IEquatable<NodePort>
    {
        internal enum NodePortTarget : byte
        {
            Input = 0,
            Output = 1
        }

        [SerializeField]
        private byte targetedPortSet;

        [SerializeField]
        private string fieldName;

        [SerializeReference]
        private INodeSerialization node;

        /// <summary>
        /// A field name to be found in the input/output of a <see cref="INode{TInput,TOutput}"/>.
        /// </summary>
        /// <remarks>
        /// The field search is done :
        /// - if used as an edge's origin => in the Output class of the <see cref="node"/>
        /// - if used as an edge's destination => in the Input class of the <see cref="node"/>
        /// </remarks>
        public string FieldName => fieldName;

        /// <summary>
        /// The <see cref="INode{TInput,TOutput}"/> instance containing the field designed by <see cref="fieldName"/>.
        /// </summary>
        public INode<InputPorts, OutputPorts> Node => (INode<InputPorts, OutputPorts>)node;

        /// <summary>
        /// The targeted port set for this node port (node inputs or outputs)
        /// </summary>
        internal NodePortTarget TargetedPortSet => (NodePortTarget)targetedPortSet;

        internal NodePort(INode<InputPorts, OutputPorts> node, NodePortTarget targetedPortSet, string fieldName)
        {
            this.fieldName = fieldName;
            this.targetedPortSet = (byte)targetedPortSet;
            this.node = node;
        }

        /// <inheritdoc cref="object.Equals(object)" />
        public bool Equals(NodePort other)
        {
            return fieldName == other.fieldName && TargetedPortSet == other.TargetedPortSet && Equals(node, other.node);
        }

        /// <inheritdoc cref="object.Equals(object)" />
        public override bool Equals(object obj)
        {
            return obj is NodePort other && Equals(other);
        }

        /// <inheritdoc cref="object.GetHashCode" />
        public override int GetHashCode()
        {
            return HashCode.Combine(fieldName, TargetedPortSet, node);
        }

        /// <inheritdoc cref="object.ToString" />
        public override string ToString()
        {
            var targetName = TargetedPortSet == NodePortTarget.Input ?
                Node != null ? Node.GetInputType.Name : nameof(InputPorts) :
                Node != null ? Node.GetOutputType.Name : nameof(OutputPorts);
            return Node != null ? $"{Node}.{targetName}.{FieldName}" : $"Null-node.{targetName}.{FieldName}";
        }

        internal FieldInfo ToFieldInfo()
        {
            if (Node == null)
                return null;

            foreach (var interfaceType in Node.GetType().GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(INode<,>))
                {
                    var outputType = interfaceType.GetGenericArguments()[(int)TargetedPortSet];
                    return outputType.GetField(FieldName);
                }
            }
            return null;
        }

        internal object GetPortValue()
        {
            return ToFieldInfo().GetValue(TargetedPortSet == NodePortTarget.Input ? Node.Input : Node.Output);
        }

        internal void SetPortValue(object value)
        {
            ToFieldInfo().SetValue(TargetedPortSet == NodePortTarget.Input ? Node.Input : Node.Output, value);
        }
    }
}
