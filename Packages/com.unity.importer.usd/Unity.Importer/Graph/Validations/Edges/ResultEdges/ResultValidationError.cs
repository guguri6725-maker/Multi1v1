using System;
using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    /// <summary>
    /// An error created during a graph validation.
    /// </summary>
    public class ResultValidationError : BaseGraphValidationError
    {
        /// <summary>
        /// The invalid <see cref="Edge">Edges</see>.
        /// </summary>
        public List<ResultEdge> Edges { get; }

        /// <summary>
        /// The result Id being referenced multiple time.
        /// </summary>
        public string Id { get; }

        internal ResultValidationError(string id, List<ResultEdge> edges, string message, Type validationType) : base(message, validationType)
        {
            Id = id;
            Edges = edges;
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"{Id} : {Message}";
        }
    }
}
