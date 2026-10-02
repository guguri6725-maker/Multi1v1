using System;

namespace UnityEngine.Importer.Validations
{
    /// <summary>
    /// An error created during a graph validation or a new edge addition to a graph.
    /// </summary>
    public class EdgeValidationError : BaseGraphValidationError
    {
        /// <summary>
        /// The invalid <see cref="Edge"/>.
        /// </summary>
        public Edge Edge { get; }

        /// <summary>
        /// <inheritdoc cref="EdgeValidationError"/>
        /// </summary>
        /// <param name="edge">The invalid <see cref="Edge"/>.</param>
        /// <param name="message">The error message.</param>
        /// <param name="validationType">The Validation type that triggerred this error.</param>
        internal EdgeValidationError(Edge edge, string message, Type validationType) : base(message, validationType)
        {
            Edge = edge;
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"Edge {Edge} : {Message}";
        }
    }
}
