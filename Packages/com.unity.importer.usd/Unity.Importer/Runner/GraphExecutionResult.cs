using System;
using System.Collections.Generic;

namespace UnityEngine.Importer
{
    /// <summary>
    /// Represents the result of the execution of a graph.
    /// </summary>
    public class GraphExecutionResult
    {
        private readonly List<IGraphValue> results;

        /// <summary>
        /// Was the execution of the graph successful (graph validation and execution).
        /// </summary>
        public bool IsSuccess => ExecutionException == null;

        /// <summary>
        /// Results gathered from the graph execution.
        /// </summary>
        public IReadOnlyList<IGraphValue> Results => results;

        /// <summary>
        /// An exception that occured during the execution of the graph.
        /// </summary>
        public Exception ExecutionException { get; }

        internal GraphExecutionResult(List<IGraphValue> results)
        {
            this.results = results;
        }

        internal GraphExecutionResult(Exception executionException)
        {
            results = new List<IGraphValue>();
            ExecutionException = executionException;
        }
    }
}
