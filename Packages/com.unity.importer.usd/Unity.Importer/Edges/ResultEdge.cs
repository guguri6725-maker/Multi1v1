using System;
using System.Globalization;
using System.Reflection;

namespace UnityEngine.Importer
{
    /// <summary>
    /// A result edge connects an output of any <see cref="Node{TInput,TOutput}"/> in the graph to a result id.
    /// </summary>
    /// <remarks>
    /// TODO: Link to graph results once it's added.
    /// </remarks>
    [Serializable]
    public struct ResultEdge : IEquatable<ResultEdge>, IOriginEdge
    {
        /// <summary>
        /// The setting id under which the result can be retrieved after the graph is processed.
        /// </summary>
        public string Id => id;

        [SerializeField]
        private string id;

        /// <summary>
        /// A <see cref="NodePort"/> representing the origin of the ResultEdge.
        /// </summary>
        public NodePort Origin => origin;

        [SerializeField]
        private NodePort origin;

        /// <summary>
        /// <inheritdoc cref="ResultEdge"/>
        /// </summary>
        /// <param name="resultId">The result Id from the <see cref="ImporterGraph"/> under which this <see cref="INode{TInput,TOutput}"/> output will be saved.</param>
        /// <param name="origin">The <see cref="ResultEdge"/> origin <see cref="INode{TInput,TOutput}"/>.</param>
        /// <param name="originFieldName">The <see cref="ResultEdge"/> origin output field on the origin <see cref="INode{TInput,TOutput}"/>.</param>
        public ResultEdge(string resultId, INode<InputPorts, OutputPorts> origin, string originFieldName)
        {
            this.origin = new NodePort(origin, NodePort.NodePortTarget.Output, originFieldName);
            this.id = resultId;
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public bool Equals(ResultEdge other)
        {
            return id == other.id && origin.Equals(other.origin);
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return obj is ResultEdge other && Equals(other);
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            return HashCode.Combine(id, origin);
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"{origin} => {id}";
        }

        internal IGraphValue GetImportResultValue()
        {
            var fieldInfo = origin.ToFieldInfo();
            if (fieldInfo == null)
                return null;

            var type = fieldInfo.FieldType;

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var setting = (IGraphValue)Activator.CreateInstance(
                typeof(ImportResult<>).MakeGenericType(type),
                flags,
                null,
                new[] { id, origin.GetPortValue() },
                CultureInfo.InvariantCulture);
            return setting;
        }
    }
}
