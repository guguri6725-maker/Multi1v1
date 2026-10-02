namespace UnityEngine.Importer
{
    /// <summary>
    /// A context to store and access logs provided to each nodes of a graph at runtime.
    /// </summary>
    public class RuntimeGraphLogger : GraphLogger
    {
        /// <inheritdoc cref="GraphLogger.LogImportError"/>
        public override void LogImportError(string errorMessage, Object obj = null, int errorId = -1)
        {
            Debug.LogError(errorMessage);
        }

        /// <inheritdoc cref="GraphLogger.LogImportWarning"/>
        public override void LogImportWarning(string warningMessage, Object obj = null, int warningId = -1)
        {
            Debug.LogWarning(warningMessage);
        }

        /// <inheritdoc cref="GraphLogger.RegisterAnalyticsData"/>
        public override void RegisterAnalyticsData(string key, object value)
        {
        }
    }
}
