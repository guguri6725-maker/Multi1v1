using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Importer.USD.GeomJobs
{
    [BurstCompile]
    internal struct ReorderIndicesJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<MeshConversionData> _meshConversionData;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<MeshDescription> _meshDescription;

        public unsafe ReorderIndicesJob(NativeArray<MeshConversionData> meshConversionData, NativeArray<MeshDescription> meshDescription)
        {
            _meshConversionData = meshConversionData;
            _meshDescription = meshDescription;
        }

        public void Execute(int primIndex)
        {
            var meshConversionData = _meshConversionData[primIndex];
            var meshDescription = _meshDescription[primIndex];
            if (_meshConversionData[primIndex].SubMeshes.Length > 0)
            {
                var reorderedVertexIndices = new NativeArray<int>(_meshDescription[primIndex].Indices.Length, Allocator.Temp,
                    NativeArrayOptions.UninitializedMemory);
                var idx = 0;
                for (var i = 0; i < meshConversionData.SubMeshes.Length; i++)
                {
                    var subMesh = meshConversionData.SubMeshes[i];
                    var subMeshStartIndex = idx;
                    for (var j = 0; j < subMesh.FaceIndices.Length; j++)
                    {
                        var range = meshConversionData.FaceIndexToIndexRanges[subMesh.FaceIndices[j]];
                        for (var k = 0; k < range.y; k++)
                        {
                            var index = range.x + k;
                            meshConversionData.SubMeshReorderedIndicesMapping[idx] = index;
                            reorderedVertexIndices[idx++] = _meshDescription[primIndex].Indices[index];
                        }
                    }

                    meshDescription.SubMeshDescriptors[i] = new SubMeshDescriptor(subMeshStartIndex,
                        idx - subMeshStartIndex, MeshTopology.Triangles);
                }

                _meshDescription[primIndex].Indices.CopyFrom(reorderedVertexIndices);
                reorderedVertexIndices.Dispose();
            }
            else
            {
                meshDescription.SubMeshDescriptors[0] = new SubMeshDescriptor(0, meshDescription.Indices.Length);
            }
        }
    }
}
