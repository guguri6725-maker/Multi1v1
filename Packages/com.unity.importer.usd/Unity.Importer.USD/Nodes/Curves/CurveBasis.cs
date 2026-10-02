namespace Unity.Importer.USD
{
    /// <summary>
    /// Defines the matrix &amp; v-step applied to sample a particular curve, only relevant for cubic curves
    /// </summary>
    public enum CurveBasis
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined,
        /// <summary>
        /// A curve of v-step 3
        /// </summary>
        Bezier,
        /// <summary>
        /// A curve of v-step 1
        /// </summary>
        CatmullRom,
        /// <summary>
        /// A curve of v-step 1
        /// </summary>
        BSpline
    }
}
