using System;
using UnityEngine.Analytics;
using System.Collections.Generic;
using Unity.Importer.USD;
using UnityEngine;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// A struct to hold the data for the USD Importer analytics event payload
    /// </summary>
    [Serializable]
# if UNITY_2023_2_OR_NEWER
    /// Analytics API changed in 2023.2 with old API deprecated
    internal struct UsdModularImporterAnalyticsData : IAnalytic.IData
# else
    internal struct UsdModularImporterAnalyticsData
# endif
    {
        public string[] dccOrigins;
        public int[] dccOriginsCounts;
        public ulong totalUsdFilesSize;
        public ulong totalAssetFilesSize;
        public int usdFilesCount;
        public int assetFilesCount;
        public string[] usdSchemas;
        public int[] usdSchemasCounts;
        public string[] usdFileExtensions;
        public int[] usdFileExtensionsCount;
        public double importDurationMs;
        public string[] errors;
        public int[] errorsCount;
        public string[] warnings;
        public int[] warningsCount;
    }

    /// <summary>
    /// A class to send the usdImporterUsage analytics event
    /// </summary>
    internal static class UsdModularImporterAnalytics
    {
        private const string VendorKey = "unity.usd.importer";
        private const string EventName = "usdImporterUsage";
        private const int EventVersion = 3;
#if UNITY_2023_2_OR_NEWER
        [AnalyticInfo(eventName: EventName, vendorKey: VendorKey, maxEventsPerHour: 1000, maxNumberOfElements: 1000, version: EventVersion)]
        internal class UsdModularImporterAnalytic : IAnalytic
        {
            public UsdModularImporterAnalytic(UsdModularImporterAnalyticsData data)
            {
                m_Data = data;
            }

            public bool TryGatherData(out IAnalytic.IData data, out System.Exception error)
            {
                error = null;
                data = m_Data;

                return data != null;
            }

            UsdModularImporterAnalyticsData m_Data;
        }
# else
        private const int MaxEventsPerHour = 100;
        private const int MaxNumberOfElements = 100;
#endif

        /// <summary>
        /// Sent an event to EditorAnalytics based on the USD Importer graph data in the editorAssetLoadingLog.
        /// </summary>
        /// <param name="editorAssetLoadingLog">The logged data from the USD Importer graph run</param>
        /// <returns>Whether the analytics event was handled successfully</returns>
        public static AnalyticsResult SendEvent(EditorGraphLogger editorAssetLoadingLog)
        {
#if !UNITY_2023_2_OR_NEWER
            AnalyticsResult result = EditorAnalytics.RegisterEventWithLimit(EventName, MaxEventsPerHour, MaxNumberOfElements, VendorKey, EventVersion);

            if (result != AnalyticsResult.Ok)
            {
                Debug.LogError($"Failed to register EditorAnalytics event '{EventName}_{EventVersion}'. Reason: {result}.");
                return result;
            }
# endif
            var dccOrigins = editorAssetLoadingLog.GetAnalyticsData<string[]>(AnalyticsDataKeys.DccOrigins);
            var dccOriginsCounts = editorAssetLoadingLog.GetAnalyticsData<int[]>(AnalyticsDataKeys.DccOriginsCounts);
            var usdSchemas = editorAssetLoadingLog.GetAnalyticsData<string[]>(AnalyticsDataKeys.UsdSchemas);
            var usdSchemasCounts = editorAssetLoadingLog.GetAnalyticsData<int[]>(AnalyticsDataKeys.UsdSchemasCounts);
            var totalUsdFilesSize = editorAssetLoadingLog.GetAnalyticsData<ulong>(AnalyticsDataKeys.TotalUsdFilesSize);
            var totalAssetFilesSize = editorAssetLoadingLog.GetAnalyticsData<ulong>(AnalyticsDataKeys.TotalAssetFilesSize);
            var usdFilesCount = editorAssetLoadingLog.GetAnalyticsData<int>(AnalyticsDataKeys.UsdFilesCount);
            var assetFilesCount = editorAssetLoadingLog.GetAnalyticsData<int>(AnalyticsDataKeys.AssetFilesCount);
            var usdFilesExtensions = editorAssetLoadingLog.GetAnalyticsData<string[]>(AnalyticsDataKeys.UsdFileExtensions);
            var usdFilesExtensionsCount = editorAssetLoadingLog.GetAnalyticsData<int[]>(AnalyticsDataKeys.UsdFileExtensionsCount);
            var importDurationMs = editorAssetLoadingLog.GetAnalyticsData<double>("importDurationMs");
            GetDescriptionsAndCount(editorAssetLoadingLog.GetErrorsForAnalytics(), NodeErrors.errorIdToDescriptions, out var errors, out var errorsCount);
            GetDescriptionsAndCount(editorAssetLoadingLog.GetWarningsForAnalytics(), NodeWarnings.warningIdToDescriptions, out var warnings, out var warningsCount);

            var data = new UsdModularImporterAnalyticsData
            {
                dccOrigins = dccOrigins,
                dccOriginsCounts = dccOriginsCounts,
                usdSchemas = usdSchemas,
                usdSchemasCounts = usdSchemasCounts,
                totalUsdFilesSize = totalUsdFilesSize,
                totalAssetFilesSize = totalAssetFilesSize,
                usdFilesCount = usdFilesCount,
                assetFilesCount = assetFilesCount,
                usdFileExtensions = usdFilesExtensions,
                usdFileExtensionsCount = usdFilesExtensionsCount,
                importDurationMs = importDurationMs,
                errors = errors,
                errorsCount = errorsCount,
                warnings = warnings,
                warningsCount = warningsCount
            };
# if UNITY_2023_2_OR_NEWER
            UsdModularImporterAnalytic analytic = new UsdModularImporterAnalytic(data);
            AnalyticsResult result = EditorAnalytics.SendAnalytic(analytic);
# else
            result = EditorAnalytics.SendEventWithLimit(EventName, data);
# endif
            if (result != AnalyticsResult.Ok)
            {
                Debug.LogError($"Failed to send EditorAnalytics event '{EventName}_{EventVersion}'. Reason: {result}.");
            }
            return result;
        }

        /// <summary>
        /// Transform input data into arrays of keys and values for analytics.
        /// </summary>
        /// <param name="idsToCount">map from id to count of a particular description</param>
        /// <param name="idsToDescription">map from id to description</param>
        /// <param name="keys">an array of descriptions</param>
        /// <param name="values">an array of description counts</param>
        private static void GetDescriptionsAndCount(IReadOnlyDictionary<int, int> idsToCount,
            IReadOnlyDictionary<int, string> idsToDescription, out string[] keys, out int[] values)
        {
            keys = new string[idsToCount.Count];
            values = new int[idsToCount.Count];

            var count = 0;
            foreach (var entry in idsToCount)
            {
                keys[count] = idsToDescription[entry.Key];
                values[count] = entry.Value;
                count++;
            }
        }
    }
}
