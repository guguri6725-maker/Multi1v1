using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Helper class to easily compare mesh vertices
    /// </summary>
    internal struct VertexCompare : IEquatable<VertexCompare>
    {
        private const float FloatPrecision = 9.99999943962493E-11f;
        private readonly unsafe float* vertexData;
        private readonly unsafe float* blendWeights;
        private readonly unsafe int* blendIndices;
        private readonly int vertexDataSize;
        private readonly int boneInfluenceCount;
        private readonly int hashcode;
        private BlendShapeInfluenceCompare blendShapeInfluence;

        /// <summary>
        /// Constructor
        /// </summary>
        public unsafe VertexCompare(float* vertexData, int vertexDataSize, float* blendWeights, int* blendIndices, int boneInfluenceCount, BlendShapeInfluenceCompare blendShapeInfluence)
        {
            this.vertexData = vertexData;
            this.vertexDataSize = vertexDataSize;
            this.blendWeights = blendWeights;
            this.blendIndices = blendIndices;
            this.boneInfluenceCount = boneInfluenceCount;
            this.blendShapeInfluence = blendShapeInfluence;
            hashcode = ComputeHashCode(vertexData, vertexDataSize / sizeof(float), blendWeights, blendIndices, boneInfluenceCount, blendShapeInfluence);
        }

        /// <summary>
        /// See documentation for &lt;IEquatable.Equals&gt; for more details
        /// </summary>
        public unsafe bool Equals(VertexCompare other)
        {
            if (GetHashCode() != other.GetHashCode())
            {
                return false;
            }

            var equality = 0 == UnsafeUtility.MemCmp(vertexData, other.vertexData, vertexDataSize) && blendShapeInfluence.Equals(other.blendShapeInfluence);
            if (!equality || boneInfluenceCount == 0)
            {
                return equality;
            }

            for (var bone = 0; bone < boneInfluenceCount; bone++)
            {
                if (blendIndices[bone] != other.blendIndices[bone] ||
                    Math.Abs(blendWeights[bone] - other.blendWeights[bone]) > FloatPrecision)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// See documentation for &lt;Object.GetHashCode&gt; for more details
        /// </summary>
        public override int GetHashCode()
        {
            return hashcode;
        }

        private static unsafe int ComputeHashCode(float* vertexData, int vertexDataCount,
            float* blendWeights, int* blendIndices, int boneInfluenceCount, BlendShapeInfluenceCompare blendShapeInfluence)
        {
            var hashcode = 0;
            for (var i = 0; i < vertexDataCount; i++)
            {
                hashcode ^= vertexData[i].GetHashCode();
            }

            for (var i = 0; i < boneInfluenceCount; i++)
            {
                hashcode ^= blendIndices[i].GetHashCode();
                hashcode ^= blendWeights[i].GetHashCode();
            }

            return hashcode ^ blendShapeInfluence.GetHashCode();
        }
    }
}
