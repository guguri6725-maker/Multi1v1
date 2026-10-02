namespace UnityEngine.Importer
{
    /// <summary>
    /// A context to store and access logs provided to each nodes of a graph.
    /// </summary>
    public abstract class GraphLogger
    {
        internal GraphLogger() {}

        /// <summary>
        /// Logs import error. The error id would be accumulated if the error id is registered for analytics.
        /// The accumulated error ids and their counts during an import can be retrieved with <see cref="GetErrorsForAnalytics"/>.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="obj">Optional Object that is targeted by the error.</param>
        /// <param name="errorId">The internal error id used for analytics.</param>
        public abstract void LogImportError(string errorMessage, Object obj = null, int errorId = -1);

        /// <summary>
        /// Logs import warning. The warning id would be accumulated if the warning id is registered for analytics.
        /// The accumulated warning ids and their counts during an import can be retrieved with <see cref="GetWarningsForAnalytics"/>.
        /// </summary>
        /// <param name="warningMessage">The warning message.</param>
        /// <param name="obj">Optional Object that is targeted by the warning.</param>
        /// <param name="warningId">The internal warning id used for analytics.</param>
        public abstract void LogImportWarning(string warningMessage, Object obj = null, int warningId = -1);

        /// <summary>
        /// Register analytics data during import.
        /// </summary>
        /// <param name="key">The key of the analytics data.</param>
        /// <param name="value">The value of the analytics data.</param>
        public abstract void RegisterAnalyticsData(string key, object value);
    }
}
