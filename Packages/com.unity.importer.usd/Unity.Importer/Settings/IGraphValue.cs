using System;

namespace UnityEngine.Importer
{
    /// <summary>
    /// Represent an indexed value in an <see cref="ImporterGraph"/>.
    /// </summary>
    /// <remarks>
    /// It is used by the <see cref="ImportResult{T}"/> struct to gather all results from the <see cref="ResultEdge"/> after a graph is executed.
    /// </remarks>
    public interface IGraphValue
    {
        /// <summary>
        /// The id used to reference this <see cref="Value"/>.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The current value associated with its <see cref="Id"/>.
        /// </summary>
        object Value { get; }
        /// <summary>
        /// The type of this <see cref="Value"/>.
        /// </summary>
        Type Type { get; }
    }
}
