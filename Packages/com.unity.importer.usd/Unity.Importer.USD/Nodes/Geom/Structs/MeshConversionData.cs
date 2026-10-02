using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Mathematics;


namespace Unity.Importer.USD
{
    /// <summary>
    /// Transitory data needed for the conversion to MeshDescriptions
    /// </summary>
    internal struct MeshConversionData
    {
        public int TriangulatedIndexCount;
        public const int SkinningDataSize = sizeof(float) + sizeof(int);
        public bool IsLeftHanded;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<int> FaceVertexCounts;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<int> FaceVertexIndices;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<int2> FaceIndexToIndexRanges;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<int> SubMeshReorderedIndicesMapping;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float3> Points;
        [NativeDisableContainerSafetyRestriction]
        public PrimVarData<float3> Normals;
        [NativeDisableContainerSafetyRestriction]
        public PrimVarData<float4> Colors;
        [NativeDisableContainerSafetyRestriction]
        public PrimVarData<float2> Uv0;
        [NativeDisableContainerSafetyRestriction]
        public PrimVarData<float2> Uv1;
        [NativeDisableContainerSafetyRestriction]
        public PrimVarData<float2> Uv2;
        [NativeDisableContainerSafetyRestriction]
        public PrimVarData<float2> Uv3;
        [NativeDisableContainerSafetyRestriction]
        public PrimVarData<float2> Uv4;
        [NativeDisableContainerSafetyRestriction]
        public PrimVarData<float2> Uv5;
        [NativeDisableContainerSafetyRestriction]
        public PrimVarData<float2> Uv6;
        [NativeDisableContainerSafetyRestriction]
        public PrimVarData<float2> Uv7;
        [NativeDisableContainerSafetyRestriction, ReadOnly]
        public NativeArray<bool> VertexAttributeExists;

        [NativeDisableParallelForRestriction]
        public NativeArray<int> JointIndices;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> Weights;

        [NoAlias, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
        public NativeArray<Subset> SubMeshes;
        [NoAlias, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
        public NativeArray<BlendShape> BlendShapes;

        public void Dispose()
        {
            FaceVertexCounts.Dispose();
            FaceVertexIndices.Dispose();
            FaceIndexToIndexRanges.Dispose();
            SubMeshReorderedIndicesMapping.Dispose();
            Points.Dispose();
            Normals.Dispose();
            Colors.Dispose();
            Uv0.Dispose();
            Uv1.Dispose();
            Uv2.Dispose();
            Uv3.Dispose();
            Uv4.Dispose();
            Uv5.Dispose();
            Uv6.Dispose();
            Uv7.Dispose();
            VertexAttributeExists.Dispose();
            JointIndices.Dispose();
            Weights.Dispose();
            if (SubMeshes.IsCreated)
            {
                for (var i = 0; i < SubMeshes.Length; i++)
                {
                    SubMeshes[i].Dispose();
                }
                SubMeshes.Dispose();
            }
            if (BlendShapes.IsCreated)
            {
                for (var i = 0; i < BlendShapes.Length; i++)
                {
                    BlendShapes[i].Dispose();
                }
                BlendShapes.Dispose();
            }
        }
    }
}
