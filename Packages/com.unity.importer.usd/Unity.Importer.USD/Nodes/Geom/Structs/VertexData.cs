using Unity.Mathematics;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Data structure containing all the required per-vertex properties in order to create meshes in Unity
    /// </summary>
    internal struct VertexData
    {
        /// <summary>
        /// Position
        /// </summary>
        public float3 Position;

        /// <summary>
        /// Normal
        /// </summary>
        public float3 Normal;

        /// <summary>
        /// Color
        /// </summary>
        public float4 Color;

        /// <summary>
        /// UV in texture coordinates #0
        /// </summary>
        public float2 Uv0;

        /// <summary>
        /// UV in texture coordinates #1
        /// </summary>
        public float2 Uv1;

        /// <summary>
        /// UV in texture coordinates #2
        /// </summary>
        public float2 Uv2;

        /// <summary>
        /// UV in texture coordinates #3
        /// </summary>
        public float2 Uv3;

        /// <summary>
        /// UV in texture coordinates #4
        /// </summary>
        public float2 Uv4;

        /// <summary>
        /// UV in texture coordinates #5
        /// </summary>
        public float2 Uv5;

        /// <summary>
        /// UV in texture coordinates #6
        /// </summary>
        public float2 Uv6;

        /// <summary>
        /// UV in texture coordinates #7
        /// </summary>
        public float2 Uv7;
    }
}
