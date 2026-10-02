using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Importer;

namespace UnityEditor.Importer
{
    /// <summary>
    /// A context to store and access logs provided to each nodes of a graph during an import at editor time.
    /// </summary>
    public class EditorGraphLogger : GraphLogger
    {
        private readonly AssetImportContext assetImportContext;
        private readonly Dictionary<int, int> errorCounts;
        private readonly Dictionary<int, int> warningCounts;
        private readonly Dictionary<string, object> analyticsData;
        private HashSet<int> errorIdsToTrack;
        private HashSet<int> warningIdsToTrack;

        private static readonly Regex ExceptionOrigin = new(@"^\s+at\s(Unity|UnityEngine|UnityEditor)\.");
        private const int ExceptionId = 0;
        private const int AnalyticsDataCastingError = 1;

        /// <summary>
        /// A context to store and access logs provided to each nodes of a graph at editor time.
        /// </summary>
        /// <param name="assetImportContext">The AssetImportContext of the current import (provided by a <see cref="ScriptedImporter"/>).</param>
        public EditorGraphLogger(AssetImportContext assetImportContext)
        {
            this.assetImportContext = assetImportContext;
            errorCounts = new Dictionary<int, int>();
            warningCounts = new Dictionary<int, int>();
            analyticsData = new Dictionary<string, object>();
        }

        /// <summary>
        /// Add analytics data during import. Data of the same key can be retrieved with <see cref="GetAnalyticsData"/>.
        /// </summary>
        /// <param name="key">The key of the analytics data.</param>
        /// <param name="value">The value of the analytics data.</param>
        public override void RegisterAnalyticsData(string key, object value)
        {
            analyticsData.TryAdd(key, value);
        }

        /// <summary>
        /// Get analytics data added with <see cref="RegisterAnalyticsData"/>.
        /// </summary>
        /// <param name="key">The key of the analytics data.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <returns>The value corresponding to key.</returns>
        public T GetAnalyticsData<T>(string key)
        {
            analyticsData.TryGetValue(key, out var value);
            T castedValue = default;

            if (value == null)
            {
                return castedValue;
            }

            try
            {
                castedValue = (T)value;
            }
            catch (InvalidCastException)
            {
                Debug.LogWarning($"Cannot cast analytics data {key} to {typeof(T)}, value type is {value.GetType().Name}");
                IncreaseCount(errorCounts, AnalyticsDataCastingError);
                castedValue = default;
            }

            return castedValue;
        }

        /// <inheritdoc cref="GraphLogger.LogImportError"/>
        public override void LogImportError(string errorMessage, UnityEngine.Object obj = null, int errorId = -1)
        {
            assetImportContext.LogImportError(errorMessage, obj);

            if (errorIdsToTrack.Contains(errorId))
            {
                IncreaseCount(errorCounts, errorId);
            }
        }

        /// <inheritdoc cref="GraphLogger.LogImportWarning"/>
        public override void LogImportWarning(string warningMessage, UnityEngine.Object obj = null, int warningId = -1)
        {
            assetImportContext.LogImportWarning(warningMessage, obj);

            if (warningIdsToTrack.Contains(warningId))
            {
                IncreaseCount(warningCounts, warningId);
            }
        }

        private static void IncreaseCount(IDictionary<int, int> dictionary, int key)
        {
            if (dictionary.TryGetValue(key, out var count))
            {
                dictionary[key] = count + 1;
            }
            else
            {
                dictionary.Add(key, 1);
            }
        }

        /// <summary>
        /// Get internal error ids and their corresponding counts registered with <see cref="LogImportError"/>.
        /// An error id of value 0 is included if an exception is thrown during the import.
        /// An error id of value 1 is included if casting error is thrown in  <see cref="GetAnalyticsData"/>.
        /// </summary>
        /// <returns>The accumulated errors and their counts during the import.</returns>
        public IReadOnlyDictionary<int, int> GetErrorsForAnalytics()
        {
            return errorCounts;
        }

        /// <summary>
        /// Get internal warning ids and their corresponding counts registered with <see cref="LogImportWarning"/>.
        /// </summary>
        /// <returns>The accumulated warnings and their counts during the import.</returns>
        public IReadOnlyDictionary<int, int> GetWarningsForAnalytics()
        {
            return warningCounts;
        }

        internal void SetException(Exception exception)
        {
            if (ExceptionOrigin.IsMatch(exception.StackTrace))
            {
                errorCounts.Add(ExceptionId, 1);
            }
        }

        internal void SetErrorLogsToTrack(HashSet<int> errorIdsToTrack)
        {
            this.errorIdsToTrack = errorIdsToTrack;
        }

        internal void SetWarningLogsToTrack(HashSet<int> warningIdsToTrack)
        {
            this.warningIdsToTrack = warningIdsToTrack;
        }
    }
}
