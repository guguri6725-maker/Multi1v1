using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Unity.Importer.USD.GeomJobs
{
    [BurstCompile]
    internal struct WriteSkinnedMeshVerticesJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<MeshConversionData> _meshConversionData;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<MeshDescription> _meshDescription;

        private StageConversionData _stageConversionData;

        public WriteSkinnedMeshVerticesJob(StageConversionData stageConversionData, NativeArray<MeshConversionData> meshConversionData, NativeArray<MeshDescription> meshDescription)
        {
            _meshConversionData = meshConversionData;
            _meshDescription = meshDescription;
            _stageConversionData = stageConversionData;
        }

        public unsafe void Execute(int primIndex)
        {
            var meshDescription = _meshDescription[primIndex];
            var meshConversionData = _meshConversionData[primIndex];
            var writtenVertices = 0;
            var skinningBufferPtr = (byte*)meshDescription.SkinningData.GetUnsafePtr();
            var weightsPtr = (float*)meshConversionData.Weights.GetUnsafePtr();
            var indicesPtr = (int*)meshConversionData.JointIndices.GetUnsafePtr();
            bool hasSubmeshes = meshConversionData.SubMeshes.Length > 0;

            for (var i = 0; i < meshDescription.Indices.Length; i++)
            {
                var dataIndex = hasSubmeshes ? meshConversionData.SubMeshReorderedIndicesMapping[i] : i;
                var vertexDataBufferPtr = (byte*)meshDescription.VertexData.GetUnsafePtr();
                var currentVertexData = new VertexData
                {
                    Position = meshConversionData.Points[meshDescription.Indices[i]],

                    Normal = meshConversionData.VertexAttributeExists[0] ? meshConversionData.Normals.OutValues[dataIndex] : float3.zero,
                    Color = meshConversionData.VertexAttributeExists[1] ? meshConversionData.Colors.OutValues[dataIndex] : float4.zero,
                    Uv0 = meshConversionData.VertexAttributeExists[2] ? meshConversionData.Uv0.OutValues[dataIndex] : float2.zero,
                    Uv1 = meshConversionData.VertexAttributeExists[3] ? meshConversionData.Uv1.OutValues[dataIndex] : float2.zero,
                    Uv2 = meshConversionData.VertexAttributeExists[4] ? meshConversionData.Uv2.OutValues[dataIndex] : float2.zero,
                    Uv3 = meshConversionData.VertexAttributeExists[5] ? meshConversionData.Uv3.OutValues[dataIndex] : float2.zero,
                    Uv4 = meshConversionData.VertexAttributeExists[6] ? meshConversionData.Uv4.OutValues[dataIndex] : float2.zero,
                    Uv5 = meshConversionData.VertexAttributeExists[7] ? meshConversionData.Uv5.OutValues[dataIndex] : float2.zero,
                    Uv6 = meshConversionData.VertexAttributeExists[8] ? meshConversionData.Uv6.OutValues[dataIndex] : float2.zero,
                    Uv7 = meshConversionData.VertexAttributeExists[9] ? meshConversionData.Uv7.OutValues[dataIndex] : float2.zero
                };
                var currentSkinningData = new SkinningData
                {
                    BlendWeights = weightsPtr + meshDescription.Indices[i] * meshDescription.BoneInfluenceCount,
                    BlendIndices = indicesPtr + meshDescription.Indices[i] * meshDescription.BoneInfluenceCount,
                    BoneInfluenceCount = meshDescription.BoneInfluenceCount
                };

                meshDescription.Indices[i] = writtenVertices;
                WriteMeshVerticesJob.WriteVertexData(vertexDataBufferPtr, writtenVertices, currentVertexData, meshConversionData, meshDescription, _stageConversionData);
                WriteSkinningData(skinningBufferPtr, writtenVertices, currentSkinningData, meshConversionData);
                writtenVertices++;
            }

            meshDescription.VertexCount[0] = writtenVertices;
        }

        private static unsafe void WriteSkinningData(byte* skinningBufferPtr, int vertexIndex, SkinningData vertex, MeshConversionData meshConversionData)
        {
            if (vertex.BoneInfluenceCount == 0)
                return;

            var skinningPtr = skinningBufferPtr + vertexIndex * vertex.BoneInfluenceCount * MeshConversionData.SkinningDataSize;

            UnsafeUtility.MemCpy(skinningPtr, vertex.BlendWeights, vertex.BoneInfluenceCount * sizeof(float));

            skinningPtr += vertex.BoneInfluenceCount * sizeof(float);
            UnsafeUtility.MemCpy(skinningPtr, vertex.BlendIndices, vertex.BoneInfluenceCount * sizeof(int));
        }
    }
}
