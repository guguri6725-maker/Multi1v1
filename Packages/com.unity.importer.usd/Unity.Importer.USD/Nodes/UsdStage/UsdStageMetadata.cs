using pxr;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Metadata extracted from the USD stage of the imported file.
    /// </summary>
    public struct UsdStageMetadata
    {
        /// <summary>
        /// Is Z the up Axis in this USD file (corresponding to the 'upAxis' USD property).
        /// </summary>
        public bool isStageZup;

        /// <summary>
        /// The amount of meters per unit in this USD file (corresponding to the 'metersPerUnit' USD property).
        /// </summary>
        public float metersPerUnit;

        /// <summary>
        /// The start time code in this USD file (corresponding to the 'startTimeCode' USD property).
        /// </summary>
        public UsdTimeCode startTimeCode;

        /// <summary>
        /// The time codes per seconds in this USD file (corresponding to the 'timeCodesPerSecond' USD property).
        /// </summary>
        public UsdTimeCode timeCodesPerSecond;
    }
}
