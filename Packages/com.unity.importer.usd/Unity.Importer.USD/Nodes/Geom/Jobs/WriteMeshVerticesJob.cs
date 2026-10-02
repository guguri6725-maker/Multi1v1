using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Unity.Importer.USD.GeomJobs
{
    [BurstCompile]
    internal struct WriteMeshVerticesJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<MeshConversionData> _meshConversionData;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<MeshDescription> _meshDescription;

        private StageConversionData _stageConversionData;

        public WriteMeshVerticesJob(StageConversionData stageConversionData, NativeArray<MeshConversionData> meshConversionData, NativeArray<MeshDescription> meshDescription)
        {
            _meshConversionData = meshConversionData;
            _meshDescription = meshDescription;
            _stageConversionData = stageConversionData;
        }

        public static unsafe void WriteVertexData(byte* vertexBufferPtr, int vertexIndex, VertexData vertex, MeshConversionData meshConversionData, MeshDescription meshDescription, StageConversionData stageConversionData)
        {
            vertex.Position *= stageConversionData.ScaleFactor;

            if (stageConversionData.IsZup)
            {
                var swap = vertex.Position.z;
                vertex.Position.z = vertex.Position.y;
                vertex.Position.y = swap;

                swap = vertex.Normal.z;
                vertex.Normal.z = vertex.Normal.y;
                vertex.Normal.y = swap;
            }
            else if (!meshConversionData.IsLeftHanded)
            {
                vertex.Position.z = -vertex.Position.z;
                vertex.Normal.z = -vertex.Normal.z;
            }

            var vertexPtr = vertexBufferPtr + vertexIndex * meshDescription.VertexSize;

            UnsafeUtility.MemCpy(vertexPtr, &vertex.Position, sizeof(float3));
            var attributeOffset = sizeof(float3);

            if (meshConversionData.VertexAttributeExists[0])
                attributeOffset = WriteVertexAttribute<float3>(vertexPtr, &vertex.Normal, attributeOffset);
            if (meshConversionData.VertexAttributeExists[1])
                attributeOffset = WriteVertexAttribute<float4>(vertexPtr, &vertex.Color, attributeOffset);
            if (meshConversionData.VertexAttributeExists[2])
                attributeOffset = WriteVertexAttribute<float2>(vertexPtr, &vertex.Uv0, attributeOffset);
            if (meshConversionData.VertexAttributeExists[3])
                attributeOffset = WriteVertexAttribute<float2>(vertexPtr, &vertex.Uv1, attributeOffset);
            if (meshConversionData.VertexAttributeExists[4])
                attributeOffset = WriteVertexAttribute<float2>(vertexPtr, &vertex.Uv2, attributeOffset);
            if (meshConversionData.VertexAttributeExists[5])
                attributeOffset = WriteVertexAttribute<float2>(vertexPtr, &vertex.Uv3, attributeOffset);
            if (meshConversionData.VertexAttributeExists[6])
                attributeOffset = WriteVertexAttribute<float2>(vertexPtr, &vertex.Uv4, attributeOffset);
            if (meshConversionData.VertexAttributeExists[7])
                attributeOffset = WriteVertexAttribute<float2>(vertexPtr, &vertex.Uv5, attributeOffset);
            if (meshConversionData.VertexAttributeExists[8])
                attributeOffset = WriteVertexAttribute<float2>(vertexPtr, &vertex.Uv6, attributeOffset);
            if (meshConversionData.VertexAttributeExists[9]) WriteVertexAttribute<float2>(vertexPtr, &vertex.Uv7, attributeOffset);
        }

        private static unsafe int WriteVertexAttribute<T>(byte* vertexPtr, void* source, int attributeOffset) where T : unmanaged
        {
            UnsafeUtility.MemCpy(vertexPtr + attributeOffset, source, sizeof(T));
            return attributeOffset + sizeof(T);
        }

        public unsafe void Execute(int primIndex)
        {
            var meshDescription = _meshDescription[primIndex];
            var meshConversionData = _meshConversionData[primIndex];
            var writtenVertices = 0;
            bool hasSubmeshes = meshConversionData.SubMeshes.Length > 0;

            for (var i = 0; i < meshDescription.Indices.Length; i++)
            {
                var dataIndex = hasSubmeshes ? meshConversionData.SubMeshReorderedIndicesMapping[i] : i;
                var vertexDataBufferPtr = (byte*)meshDescription.VertexData.GetUnsafePtr();
                var currentVertex = new VertexData()
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
                    Uv7 = meshConversionData.VertexAttributeExists[9] ? meshConversionData.Uv7.OutValues[dataIndex] : float2.zero,
                };

                meshDescription.Indices[i] = writtenVertices;
                WriteVertexData(vertexDataBufferPtr, writtenVertices, currentVertex, meshConversionData, meshDescription, _stageConversionData);
                writtenVertices++;
            }


            meshDescription.VertexCount[0] = writtenVertices;
        }
    }
}
