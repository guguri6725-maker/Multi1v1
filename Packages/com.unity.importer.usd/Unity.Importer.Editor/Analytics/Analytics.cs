using System;
using UnityEngine;
using UnityEngine.Analytics;

namespace UnityEditor.Importer.Analytics
{
    /// <summary>
    /// 1- Setup the schema on https://schemata.prd.dp.unity3d.com/onboarding and validate the schema with the analytics team first.
    /// -- The event can't be named modular_importer anymore, because we used that for our test.
    /// 2- Change the ModularImportEvent struct to reflect the correct schema.
    /// 3- go to https://console.cloud.google.com/bigquery?project=unity-ai-data-access-ai-prd and try to fetch the sent data
    /// -- the query should look like
    /// SELECT * FROM `unity-ai-data-prd.editor_analytics_raw.editor_analytics_modular_importer_v1` WHERE date(submit_datetime) between DATE_SUB(current_date(), INTERVAL 30 DAY) and current_date();
    /// --
    /// 4- go to our dashboard https://datastudio.google.com/u/0/reporting/a8ac84b5-5d2b-4311-b9bc-00ec0c4df30c/
    /// 5- add the new query (Edit - Resource/Manage added data sources)
    /// 6- add some data in the analytics dashboard from this query result.
    /// </summary>
    static class AnalyticsTools
    {
        // TODO: fill in the event name once it's published on schemata.
        private const string k_ImportEvent = "";
        // This is the actual limit of the same event one editor instance can fire each hour.
        private const int k_MaxEventPerHour = 10000;
        private const int k_MaxItemPerEvent = 10;
        private const string k_EventPackageName = "unity.com.unity.importer";

        /// TODO: This struct has to match the schema on Schemata (editor.analytics.modular_importer_event.v1)
        /// All data added in the struct for now were just for testing purposes and need to be changed.
        struct ModularImportEvent
        {
            public string Importer;
            public string Extension;
            public string assembly_info;
            public string package;
            public string package_ver;
            public long ts;
            public int t_since_start;
        }

        /// <summary>
        /// Use this method to send an analytics event when an asset was imported through the ModularImporter
        /// </summary>
        /// <param name="guid">The guid of the ImporterGraph asset that was used</param>
        /// <param name="extension">The extension of the imported Source Asset</param>
        internal static void SendModularImportEvent(string guid, string extension)
        {
            throw new NotImplementedException("Do not use until the analytics are reviewed and validated.");

#pragma warning disable CS0162
            if (!EditorAnalytics.enabled)
                return;

            if (UnityEngine.Analytics.Analytics.IsCustomEventEnabled(k_ImportEvent) != AnalyticsResult.Ok)
            {
                var registration =
                    EditorAnalytics.RegisterEventWithLimit(k_ImportEvent, k_MaxEventPerHour, k_MaxItemPerEvent, k_EventPackageName);
                if (registration != AnalyticsResult.Ok)
                {
                    Debug.LogError(
                        $"Unity.Editor.Importer.Analytics didn't initialized properly, error is {registration.ToString()}");
                    return;
                }
            }
            var sentEvent = EditorAnalytics.SendEventWithLimit(k_ImportEvent,
                new ModularImportEvent()
                {
                    Extension = extension,
                    Importer = guid,
                    assembly_info = "some_assembly_name",
                    package = "com.unity.importer", //TODO: I think we definitely don't need that data
                    package_ver = "0.1.0", // TODO: the version should be pulled from pacman if we want to keep that in the future.
                    t_since_start = (int)12000, // TODO: change that by the time it took to actually import the file.
                    ts = DateTime.Now.Millisecond
                });
            if (sentEvent != AnalyticsResult.Ok)
            {
                Debug.LogError($"Unity.Editor.Importer.Analytics couldn't send event modular_importer, error is {sentEvent.ToString()}");
            }
#pragma warning restore CS0162
        }
    }
}
