using System;
using Unity.Mathematics;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Helper structure that allow for easy comparison of blend shape data
    /// </summary>
    internal struct BlendShapeDescriptionReference
    {
        /// <summary>
        /// Position offsets for each vertex.
        /// </summary>
        public unsafe float3* deltaVertices;

        /// <summary>
        /// Normal offsets for each vertex.
        /// </summary>
        public unsafe float3* deltaNormals;

        /// <summary>
        /// Number of frames in this blend shape.
        /// </summary>
        public int frameCount;

        /// <summary>
        /// Number of vertices.
        /// </summary>
        public int vertexCount;
    }

    /// <summary>
    /// Helper structure that implements comparison between blend shape influences
    /// </summary>
    internal struct BlendShapeInfluenceCompare : IEquatable<BlendShapeInfluenceCompare>
    {
        public int VertexIndex;
        public int BlendShapeCount;
        private int hashcode;
        public unsafe BlendShapeDescriptionReference* blendShapeRefs;

        /// <summary>
        /// Constructor
        /// </summary>
        public unsafe BlendShapeInfluenceCompare(int vertexIndex, int blendShapeCount,
                                                 BlendShapeDescriptionReference* blendShapeRefs)
        {
            VertexIndex = vertexIndex;
            BlendShapeCount = blendShapeCount;
            this.blendShapeRefs = blendShapeRefs;
            hashcode = ComputeHashCode(vertexIndex, blendShapeCount, blendShapeRefs);
        }

        /// <summary>
        /// See documentation for &lt;IEquatable.Equals&gt; for more details
        /// </summary>
        public unsafe bool Equals(BlendShapeInfluenceCompare other)
        {
            if (GetHashCode() != other.GetHashCode())
            {
                return false;
            }

            for (var blendShapeIndex = 0; blendShapeIndex < BlendShapeCount; blendShapeIndex++)
            {
                var deltaVertices = blendShapeRefs[blendShapeIndex].deltaVertices;
                var deltaNormals = blendShapeRefs[blendShapeIndex].deltaNormals;
                var frameCount = blendShapeRefs[blendShapeIndex].frameCount;
                var vertexCount = blendShapeRefs[blendShapeIndex].vertexCount;

                for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    var frameVertexIndex = vertexCount * frameIndex + VertexIndex;
                    var otherFrameVertexIndex = vertexCount * frameIndex + other.VertexIndex;

                    if (!(math.all(deltaVertices[frameVertexIndex] == deltaVertices[otherFrameVertexIndex]) &
                          math.all(deltaNormals[frameVertexIndex] == deltaNormals[otherFrameVertexIndex])))
                    {
                        return false;
                    }
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

        private static unsafe int ComputeHashCode(int vertexIndex, int blendShapeCount,
            BlendShapeDescriptionReference* blendShapeRefs)
        {
            var hashcode = blendShapeCount;
            for (var blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
            {
                var deltaVertices = blendShapeRefs[blendShapeIndex].deltaVertices;
                var deltaNormals = blendShapeRefs[blendShapeIndex].deltaNormals;
                var frameCount = blendShapeRefs[blendShapeIndex].frameCount;
                var vertexCount = blendShapeRefs[blendShapeIndex].vertexCount;

                for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    var frameVertexIndex = vertexCount * frameIndex + vertexIndex;
                    hashcode ^= (int)math.hash(deltaVertices[frameVertexIndex]);
                    hashcode ^= (int)math.hash(deltaNormals[frameVertexIndex]);
                }
            }

            return hashcode;
        }
    }
}
