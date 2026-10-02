using Unity.Burst;
using Unity.Jobs;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Importer.USD.GeomJobs
{
    [BurstCompile()]
    internal struct FlattenPrimvarsJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<MeshConversionData> _meshConversionData;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<MeshDescription> _meshDescription;


        public unsafe FlattenPrimvarsJob(NativeArray<MeshConversionData> meshConversionData, NativeArray<MeshDescription> meshDescription)
        {
            _meshConversionData = meshConversionData;
            _meshDescription = meshDescription;
        }

        public void Execute(int primIndex)
        {
            var meshConversionData = _meshConversionData[primIndex];
            if (meshConversionData.Normals.Values.Length > 0) FlattenPrimVar(primIndex, meshConversionData.Normals);
            if (meshConversionData.Colors.Values.Length > 0) FlattenPrimVar(primIndex, meshConversionData.Colors);
            if (meshConversionData.Uv0.Values.Length > 0) FlattenPrimVar(primIndex, meshConversionData.Uv0);
            if (meshConversionData.Uv1.Values.Length > 0) FlattenPrimVar(primIndex, meshConversionData.Uv1);
            if (meshConversionData.Uv2.Values.Length > 0) FlattenPrimVar(primIndex, meshConversionData.Uv2);
            if (meshConversionData.Uv3.Values.Length > 0) FlattenPrimVar(primIndex, meshConversionData.Uv3);
            if (meshConversionData.Uv4.Values.Length > 0) FlattenPrimVar(primIndex, meshConversionData.Uv4);
            if (meshConversionData.Uv5.Values.Length > 0) FlattenPrimVar(primIndex, meshConversionData.Uv5);
            if (meshConversionData.Uv6.Values.Length > 0) FlattenPrimVar(primIndex, meshConversionData.Uv6);
            if (meshConversionData.Uv7.Values.Length > 0) FlattenPrimVar(primIndex, meshConversionData.Uv7);
        }

        private void FlattenPrimVar<T>(int primIndex, PrimVarData<T> primvar) where T : struct, IEquatable<T>, IFormattable
        {
            if (primvar.Values.Length == 0)
                return;

            if (primvar.Interpolation == PrimVarInterpolation.Constant)
            {
                var constantValue = primvar.Values[0];
                for (var i = 0; i < primvar.OutValues.Length; i++)
                {
                    primvar.OutValues[i] = constantValue;
                }
            }
            else if (primvar.Interpolation == PrimVarInterpolation.Uniform) // 1 value per face
            {
                for (var faceIdx = 0; faceIdx < primvar.Values.Length; faceIdx++)
                {
                    var newFaceIndices = _meshConversionData[primIndex].FaceIndexToIndexRanges[faceIdx];
                    var value = primvar.Values[faceIdx];
                    var faceIndicesLastIndex = newFaceIndices.x + newFaceIndices.y;
                    for (var i = newFaceIndices.x; i < faceIndicesLastIndex; i++)
                    {
                        primvar.OutValues[i] = value;
                    }
                }
            }
            else if (primvar.Interpolation == PrimVarInterpolation.Vertex || primvar.Interpolation == PrimVarInterpolation.Varying) // 1 value per point
            {
                var indices = _meshDescription[primIndex].Indices;
                for (var i = 0; i < indices.Length; i++)
                {
                    primvar.OutValues[i] = primvar.Values[indices[i]];
                }
            }
            else if (primvar.Interpolation == PrimVarInterpolation.FaceVarying) // 1 value per vertex per triangle
            {
                var first = 0;
                var newIndicesIdx = 0;
                for (var i = 0; i < _meshConversionData[primIndex].FaceVertexCounts.Length; i++)
                {
                    var triangleCount = _meshConversionData[primIndex].FaceVertexCounts[i] - 2;
                    var next = first + 1;
                    for (var t = 0; t < triangleCount; t++)
                    {
                        primvar.OutValues[newIndicesIdx++] = primvar.Values[next++];
                        primvar.OutValues[newIndicesIdx++] = primvar.Values[first];
                        primvar.OutValues[newIndicesIdx++] = primvar.Values[next];
                    }
                    first += _meshConversionData[primIndex].FaceVertexCounts[i];
                }
            }
        }
    }
}
