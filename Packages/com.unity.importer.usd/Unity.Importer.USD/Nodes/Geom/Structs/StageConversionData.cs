namespace Unity.Importer.USD
{
    /// <summary>
    /// Data describing necessary stage data needed from mesh conversion.
    /// </summary>
    public struct StageConversionData
    {
        /// <summary>
        /// The scale factor.
        /// </summary>
        public float ScaleFactor;

        /// <summary>
        /// Is Z the up Axis?
        /// </summary>
        public bool IsZup;
    }
}
