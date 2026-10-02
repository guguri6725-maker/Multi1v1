using System;
using pxr;
using Unity.Collections;
using Unity.Mathematics;

namespace Unity.Importer.USD
{
    //TODO default weight for a blendshape are extracted from the frame 0 of its animation. Follow up on this when we support animation
    /// <summary>
    /// BlendShape data for a mesh.
    /// </summary>
    /// <remarks>
    /// We will write each blendshape frame consecutively in our array.
    /// considering a blendshape : frame_0 with offsets off_00, off_01, off_02 AND frame_1 with offsets off_10, off_11, off_12
    /// the resulting offsets array will be : off_00, off_01, off_02, off_10, off_11, off_12
    ///
    /// Note : offsets arrays need to have a size of mesh.vertexCount.
    /// </remarks>
    public struct BlendShapeDescription : IDisposable
    {
        /// <summary>
        /// Name of the blend shape.
        /// </summary>
        public NativeArray<char> Name;

        /// <summary>
        /// Position offsets for each vertex.
        /// </summary>
        public NativeArray<float3> DeltaVertices;

        /// <summary>
        /// Normal offsets for each vertex.
        /// </summary>
        public NativeArray<float3> DeltaNormals;

        /// <summary>
        /// Weight of each frame of the blend shape.
        /// </summary>
        public NativeArray<float> Weights;

        /// <summary>
        /// Number of frames in this blend shape.
        /// </summary>
        public int FrameCount;

        /// <summary>
        /// BlendShape data for a mesh.
        /// </summary>
        /// <param name="blendShapeIn">The &lt;pxr.UsdSkelBlendShape&gt; used to initialize this description.</param>
        /// <param name="vertexCount">The vertex count of the linked mesh</param>
        public BlendShapeDescription(UsdSkelBlendShape blendShapeIn, int vertexCount)
        {
            var primName = blendShapeIn.GetPrim().GetName().ToString();
            Name = new NativeArray<char>(primName.ToCharArray(), Allocator.Temp);

            FrameCount = blendShapeIn.GetInbetweens().Count + 1; //in-between count + default last frame
            Weights = new NativeArray<float>(FrameCount, Allocator.Temp);
            Weights[FrameCount - 1] = 1f;

            var size = vertexCount * FrameCount;

            DeltaVertices = new NativeArray<float3>(size, Allocator.Temp);
            DeltaNormals = new NativeArray<float3>(size, Allocator.Temp);
        }

        /// <summary>
        /// See documentation for &lt;IDisposable.Dispose&gt; for more details
        /// </summary>
        public void Dispose()
        {
            Name.Dispose();
            DeltaVertices.Dispose();
            DeltaNormals.Dispose();
            Weights.Dispose();
        }
    }
}
