namespace Unity.Importer.USD
{
    /// <summary>
    /// Structure containing the skinning data required to create skinned meshes in Unity
    /// </summary>
    public struct SkinningData
    {
        /// <summary>
        /// The number of bone influences
        /// </summary>
        public int BoneInfluenceCount;

        /// <summary>
        /// The blend weights
        /// </summary>
        public unsafe float* BlendWeights;

        /// <summary>
        /// The blend indices
        /// </summary>
        public unsafe int* BlendIndices;
    }
}
