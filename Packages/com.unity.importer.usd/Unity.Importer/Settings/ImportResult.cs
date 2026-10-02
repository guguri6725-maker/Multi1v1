using System;

namespace UnityEngine.Importer
{
    /// <summary>
    /// Represent one result of the execution of an <see cref="ImporterGraph"/>.
    /// </summary>
    /// <remarks>
    /// The execution of an <see cref="ImporterGraph"/> will always generate a list of ImportResult,
    /// each one corresponding to one <see cref="ResultEdge"/> declared in that graph.
    /// </remarks>
    /// <typeparam name="T">The Type of the ImportResult.</typeparam>
    public struct ImportResult<T> : IGraphValue
    {
        private T value;

        internal ImportResult(string id, T value)
        {
            Id = id;
            this.value = value;
        }

        /// <inheritdoc cref="IGraphValue.Id"/>
        public string Id { get; }

        /// <inheritdoc cref="IGraphValue.Value"/>
        public T Value => value;

        object IGraphValue.Value => value;

        /// <inheritdoc cref="IGraphValue.Type"/>
        public Type Type => typeof(T);
    }
}
