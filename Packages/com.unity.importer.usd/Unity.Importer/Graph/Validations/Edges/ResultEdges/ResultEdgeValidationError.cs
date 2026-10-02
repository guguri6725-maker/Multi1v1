using System;

namespace UnityEngine.Importer.Validations
{
    /// <summary>
    /// An error created during a graph validation or a new edge addition to a graph.
    /// </summary>
    public class ResultEdgeValidationError : BaseGraphValidationError
    {
        /// <summary>
        /// The invalid <see cref="Edge"/>.
        /// </summary>
        public ResultEdge Edge { get; }

        /// <summary>
        /// <inheritdoc cref="EdgeValidationError"/>
        /// </summary>
        /// <param name="edge">The invalid <see cref="Edge"/>.</param>
        /// <param name="message">The error message.</param>
        /// <param name="validationType">The Validation type that triggerred this error.</param>
        internal ResultEdgeValidationError(ResultEdge edge, string message, Type validationType) : base(message, validationType)
        {
            Edge = edge;
        }

        /// <summary>
        /// Get a string representing the error.
        /// </summary>
        /// <returns>Message representing the error.</returns>
        public override string ToString()
        {
            return $"{Edge} : {Message}";
        }
    }
}
