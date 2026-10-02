using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Importer.USD.GeomJobs
{
    [BurstCompile]
    internal struct WriteBlendShapesJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<MeshConversionData> _meshConversionData;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<MeshDescription> _meshDescription;

        private StageConversionData _stageConversionData;

        public WriteBlendShapesJob(StageConversionData stageConversionData,  NativeArray<MeshConversionData> meshConversionData, NativeArray<MeshDescription> meshDescription)
        {
            _meshConversionData = meshConversionData;
            _meshDescription = meshDescription;
            _stageConversionData = stageConversionData;
        }

        public void Execute(int primIndex)
        {
            var meshDescription = _meshDescription[primIndex];
            var meshConversionData = _meshConversionData[primIndex];
            for (var i = 0; i < meshConversionData.BlendShapes.Length; i++) //for each blendshapes
            {
                var blendShape = meshConversionData.BlendShapes[i];
                var blendShapeDesc = meshDescription.BlendShapeDescriptions[i];

                blendShapeDesc.FrameCount = blendShape.Weights.Length;
                for (var j = 0; j < blendShape.Weights.Length; j++) //for each frame in the blendshape (called in-betweens in USD)
                {
                    var readBaseIndex = j * blendShape.FrameSize;
                    var writeBaseIndex = j * meshDescription.Indices.Length;
                    for (var k = 0; k < meshDescription.Indices.Length; k++) // write the frame data
                    {
                        var readPointIndex = meshDescription.Indices[k];
                        var writeIndex = writeBaseIndex + k;

                        if (blendShape.PointIndexToLocation.TryGetValue(readPointIndex, out int pointIndexIndex))
                        {
                            var offset = blendShape.Offsets[pointIndexIndex + readBaseIndex];
                            blendShapeDesc.DeltaVertices[writeIndex] = new float3(offset.x * _stageConversionData.ScaleFactor, offset.y * _stageConversionData.ScaleFactor, -offset.z * _stageConversionData.ScaleFactor);
                            var normalOffset = blendShape.NormalOffsets[pointIndexIndex + readBaseIndex];
                            blendShapeDesc.DeltaNormals[writeIndex] = new float3(normalOffset.x * _stageConversionData.ScaleFactor, normalOffset.y * _stageConversionData.ScaleFactor, -normalOffset.z * _stageConversionData.ScaleFactor);
                        }
                        else
                        {
                            blendShapeDesc.DeltaVertices[writeIndex] = float3.zero;
                            blendShapeDesc.DeltaNormals[writeIndex] = float3.zero;
                        }
                    }

                    blendShapeDesc.Weights[j] = blendShape.Weights[j];
                }

                meshDescription.BlendShapeDescriptions[i] = blendShapeDesc;
            }
        }
    }
}
