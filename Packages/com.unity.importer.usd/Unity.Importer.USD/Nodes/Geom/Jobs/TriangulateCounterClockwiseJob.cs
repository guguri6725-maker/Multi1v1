using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Importer.USD.GeomJobs
{
    [BurstCompile]
    internal struct TriangulateCounterClockwiseJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<MeshConversionData> _meshConversionData;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<MeshDescription> _meshDescription;

        public unsafe TriangulateCounterClockwiseJob(NativeArray<MeshConversionData> meshConversionData, NativeArray<MeshDescription> meshDescription)
        {
            _meshConversionData = meshConversionData;
            _meshDescription = meshDescription;
        }

        public void Execute(int primIndex)
        {
            var first = 0;
            var newIndicesIdx = 0;
            var meshConversionData = _meshConversionData[primIndex];
            var meshDescription = _meshDescription[primIndex];
            for (var i = 0; i < meshConversionData.FaceVertexCounts.Length; i++)
            {
                var triangleCount = meshConversionData.FaceVertexCounts[i] - 2;
                var next = first + 1;
                var t = 0;
                for (; t < triangleCount; t++)
                {
                    meshDescription.Indices[newIndicesIdx++] = meshConversionData.FaceVertexIndices[next++];
                    meshDescription.Indices[newIndicesIdx++] = meshConversionData.FaceVertexIndices[first];
                    meshDescription.Indices[newIndicesIdx++] = meshConversionData.FaceVertexIndices[next];
                }
                first += meshConversionData.FaceVertexCounts[i];
            }
        }
    }
}
