using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace Unity.Importer.USD.GeomJobs
{
    [BurstCompile]
    internal struct ComputeFaceIndexToIndexRangesJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<MeshConversionData> _meshConversionData;

        public unsafe ComputeFaceIndexToIndexRangesJob(NativeArray<MeshConversionData> meshConversionData)
        {
            _meshConversionData = meshConversionData;
        }

        public void Execute(int primIndex)
        {
            var triangleIndexStart = 0;
            for (var i = 0; i < _meshConversionData[primIndex].FaceVertexCounts.Length; i++)
            {
                var triangleCount = _meshConversionData[primIndex].FaceVertexCounts[i] - 2;
                var meshConversionData = _meshConversionData[primIndex];
                meshConversionData.FaceIndexToIndexRanges[i] = new int2(triangleIndexStart, triangleCount * 3);
                triangleIndexStart += triangleCount * 3;
            }
        }
    }
}
