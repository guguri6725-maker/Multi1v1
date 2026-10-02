using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
namespace Unity.Importer.USD
{
    /// <summary>
    /// Data describing a mesh in Unity.
    /// </summary>
    public struct MeshDescription : IDisposable
    {
        /// <summary>
        /// Bounds of the mesh.
        /// </summary>
        public Bounds Bounds;

        /// <summary>
        /// Memory size of one of this mesh vertex.
        /// </summary>
        public int VertexSize => VertexPositionAndNormalSize + VertexColorAndUVSize;

        /// <summary>
        /// Memory size of one of this mesh vertex's position and normal
        /// </summary>
        public int VertexPositionAndNormalSize;

        /// <summary>
        /// Memory size of one of this mesh vertex's color and texture coordinates
        /// </summary>
        public int VertexColorAndUVSize;

        /// <summary>
        /// Stream to use for skinning data
        /// </summary>
        public int skinningDataStream => VertexColorAndUVSize > 0 ? 2 : 1;

        /// <summary>
        /// Mesh indices buffer.
        /// </summary>
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<int> Indices;

        /// <summary>
        /// Mesh vertex count.
        /// </summary>
        /// <remarks>
        /// Due to job limitation, this value is stored in the first entry of this NativeArray
        /// </remarks>
        public NativeArray<int> VertexCount;

        /// <summary>
        /// Mesh data buffer, used to write the Unity mesh.
        /// </summary>
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<byte> VertexData;

        /// <summary>
        /// <see cref="SubMeshDescriptor"/> for this mesh.
        /// </summary>
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<SubMeshDescriptor> SubMeshDescriptors;

        /// <summary>
        /// Describe the data provided for each vertex (position, uv, skinned mesh weights, etc...).
        /// </summary>
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<VertexAttributeDescriptor> VertexAttributeDescriptors;

        /// <summary>
        /// Blend shape data for this mesh.
        /// </summary>
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<BlendShapeDescription> BlendShapeDescriptions;

        /// <summary>
        /// Amount of bone influencing a vertex for a skinned mesh.
        /// </summary>
        public int BoneInfluenceCount;

        /// <summary>
        /// Skinned mesh data buffer. Used to write the skinned mesh info in a Unity mesh.
        /// </summary>
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<byte> SkinningData;

        /// <summary>
        /// Joint binding matrix for skinned mesh.
        /// </summary>
        [NativeDisableParallelForRestriction]
        public NativeArray<float4x4> JointsBindingMatrices;

        /// <summary>
        /// See documentation for &lt;IDisposable.Dispose&gt; for more details
        /// </summary>
        public void Dispose()
        {
            Indices.Dispose();
            VertexCount.Dispose();
            VertexData.Dispose();
            SubMeshDescriptors.Dispose();
            VertexAttributeDescriptors.Dispose();

            if (BlendShapeDescriptions.IsCreated)
            {
                for (var i = 0; i < BlendShapeDescriptions.Length; i++)
                {
                    BlendShapeDescriptions[i].Dispose();
                }
                BlendShapeDescriptions.Dispose();
            }

            SkinningData.Dispose();
            JointsBindingMatrices.Dispose();
        }
    }
}
