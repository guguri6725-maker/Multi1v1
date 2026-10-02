namespace Unity.Importer.USD
{
    /// <summary>
    /// Represents the data related to an XForm USD Prim.
    /// The translation, rotation and scale data have been converted to a representation closer to Unity Transforms.
    /// </summary>
    public struct XFormableDescription
    {
        /// <summary>
        /// The GameObject path on which the transform data should be applied
        /// </summary>
        public string name;

        /// <summary>
        /// The transform data
        /// </summary>
        public TransformData transformData;
    }
}
