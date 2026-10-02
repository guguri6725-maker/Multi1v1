using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node removes duplicate vertices from input meshes and output meshes containing unique vertices
    /// </summary>
    [NodeMetadata("DuplicateVerticesRemovalNode", 2)]
    public class DuplicateVerticesRemovalNode : Node<DuplicateVerticesRemovalNode.InputPort, DuplicateVerticesRemovalNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="DuplicateVerticesRemovalNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// Prim path to MeshDescription. MeshDescriptions are unique in this collection.
            /// </summary>
            public Dictionary<string, MeshDescription> pathToMeshDescription;
        }

        /// <summary>
        /// Output ports of the <see cref="DuplicateVerticesRemovalNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// Prim path to MeshDescription. MeshDescriptions are unique in this collection.
            /// </summary>
            public Dictionary<string, MeshDescription> pathToMeshDescription;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            var meshCount = Input.pathToMeshDescription.Count;
            var meshDescriptions = new NativeArray<MeshDescription>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var blendShapeReferencesByMesh = new NativeArray<NativeArray<BlendShapeDescriptionReference>>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var vertexComparesByMesh = new NativeArray<NativeArray<VertexCompare>>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var oldToNewIndexMapByMesh = new NativeArray<UnsafeParallelHashMap<int, int>>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var meshIndex = 0;
            foreach (var entry in Input.pathToMeshDescription)
            {
                var meshVertexCount = entry.Value.Indices.Length;
                meshDescriptions[meshIndex] = entry.Value;
                blendShapeReferencesByMesh[meshIndex] = ConstructBlendShapeReferences(entry.Value);
                vertexComparesByMesh[meshIndex] = new NativeArray<VertexCompare>(meshVertexCount, Allocator.TempJob);
                oldToNewIndexMapByMesh[meshIndex] = new UnsafeParallelHashMap<int, int>(meshVertexCount, Allocator.TempJob);
                meshIndex++;
            }

            var createVertexCompareJob = new CreateVertexCompareJob(meshDescriptions, blendShapeReferencesByMesh, vertexComparesByMesh);
            var weldDuplicatesJob = new WeldDuplicatesJob(meshDescriptions, vertexComparesByMesh, oldToNewIndexMapByMesh);
            var moveVertexDataJob = new MoveVertexDataJob(meshDescriptions, oldToNewIndexMapByMesh);

            var jobHandle = createVertexCompareJob.Schedule(meshDescriptions.Length, 32);
            jobHandle = weldDuplicatesJob.Schedule(meshDescriptions.Length, 1, jobHandle);
            jobHandle = moveVertexDataJob.Schedule(meshDescriptions.Length, 1, jobHandle);

            jobHandle.Complete();

            for (var i = 0; i < Input.pathToMeshDescription.Count; i++)
            {
                vertexComparesByMesh[i].Dispose();
                blendShapeReferencesByMesh[i].Dispose();
                oldToNewIndexMapByMesh[i].Dispose();
            }
            meshDescriptions.Dispose();
            vertexComparesByMesh.Dispose();
            blendShapeReferencesByMesh.Dispose();
            oldToNewIndexMapByMesh.Dispose();

            Output.pathToMeshDescription = Input.pathToMeshDescription;
        }

        private static unsafe NativeArray<BlendShapeDescriptionReference> ConstructBlendShapeReferences(MeshDescription meshDescription)
        {
            var blendShapeReferences = new NativeArray<BlendShapeDescriptionReference>(meshDescription.BlendShapeDescriptions.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            if (meshDescription.BlendShapeDescriptions.Length > 0)
            {
                for (var blendShapeIndex = 0; blendShapeIndex < meshDescription.BlendShapeDescriptions.Length; blendShapeIndex++)
                {
                    blendShapeReferences[blendShapeIndex] = new BlendShapeDescriptionReference()
                    {
                        deltaVertices = (float3*)meshDescription.BlendShapeDescriptions[blendShapeIndex].DeltaVertices.GetUnsafePtr(),
                        deltaNormals = (float3*)meshDescription.BlendShapeDescriptions[blendShapeIndex].DeltaNormals.GetUnsafePtr(),
                        frameCount = meshDescription.BlendShapeDescriptions[blendShapeIndex].FrameCount,
                        vertexCount = meshDescription.VertexCount[0]
                    };
                }
            }

            return blendShapeReferences;
        }
    }

    /// <summary>
    /// Create VertexCompare structs for vertex comparison
    /// </summary>
    [BurstCompile]
    internal readonly struct CreateVertexCompareJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] private readonly NativeArray<MeshDescription> meshDescriptions;

        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] private readonly NativeArray<NativeArray<BlendShapeDescriptionReference>> blendShapeReferencesByMesh;

        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] private readonly NativeArray<NativeArray<VertexCompare>> vertexComparesByMesh;

        public CreateVertexCompareJob(NativeArray<MeshDescription> meshDescriptions,
                                      NativeArray<NativeArray<BlendShapeDescriptionReference>> blendShapeReferencesByMesh, NativeArray<NativeArray<VertexCompare>> vertexComparesByMesh)
        {
            this.meshDescriptions = meshDescriptions;
            this.blendShapeReferencesByMesh = blendShapeReferencesByMesh;
            this.vertexComparesByMesh = vertexComparesByMesh;
        }

        public unsafe void Execute(int meshIndex)
        {
            var meshDescription = meshDescriptions[meshIndex];
            var blendShapeReferences = blendShapeReferencesByMesh[meshIndex].Length > 0
                ? (BlendShapeDescriptionReference*)blendShapeReferencesByMesh[meshIndex].GetUnsafePtr()
                : null;
            var vertexCompares = vertexComparesByMesh[meshIndex];
            var vertexDataPtr = (byte*)meshDescription.VertexData.GetUnsafePtr();
            var skinningDataPtr = (byte*)meshDescription.SkinningData.GetUnsafePtr();

            for (var vertexIndex = 0; vertexIndex < meshDescription.Indices.Length; vertexIndex++)
            {
                var dataOffset = vertexIndex * meshDescription.VertexSize;
                float* blendWeights = null;
                int* blendIndices = null;

                if (meshDescription.SkinningData.Length > 0)
                {
                    var skinningDataOffset = vertexIndex * meshDescription.BoneInfluenceCount *
                        MeshConversionData.SkinningDataSize;
                    var blendWeightsSize = meshDescription.BoneInfluenceCount * sizeof(float);
                    blendWeights = (float*)(skinningDataPtr + skinningDataOffset);
                    blendIndices = (int*)(skinningDataPtr + skinningDataOffset + blendWeightsSize);
                }

                var blendShapeInfluence = new BlendShapeInfluenceCompare(vertexIndex,
                    meshDescription.BlendShapeDescriptions.Length, blendShapeReferences);

                vertexCompares[vertexIndex] = new VertexCompare(vertexData: (float*)(vertexDataPtr + dataOffset),
                    vertexDataSize: meshDescription.VertexSize, blendWeights: blendWeights, blendIndices: blendIndices,
                    boneInfluenceCount: meshDescription.BoneInfluenceCount, blendShapeInfluence: blendShapeInfluence);
            }
        }
    }

    /// <summary>
    /// Computing unique vertices and their new indices
    /// </summary>
    [BurstCompile]
    internal struct WeldDuplicatesJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] private readonly NativeArray<NativeArray<VertexCompare>> vertexComparesByMesh;

        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] private readonly NativeArray<MeshDescription> meshDescriptions;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<UnsafeParallelHashMap<int, int>> oldToNewIndexMapByMesh;

        public WeldDuplicatesJob(NativeArray<MeshDescription> meshDescriptions, NativeArray<NativeArray<VertexCompare>> vertexComparesByMesh,
                                 NativeArray<UnsafeParallelHashMap<int, int>> oldToNewIndexMapByMesh)
        {
            this.meshDescriptions = meshDescriptions;
            this.vertexComparesByMesh = vertexComparesByMesh;
            this.oldToNewIndexMapByMesh = oldToNewIndexMapByMesh;
        }

        public void Execute(int meshIndex)
        {
            var uniqueVertexCurrentIndex = 0;
            var meshDescription = meshDescriptions[meshIndex];
            var uniqueVertexMap = new UnsafeParallelHashMap<VertexCompare, int>(meshDescription.Indices.Length, Allocator.TempJob);
            var vertexCompares = vertexComparesByMesh[meshIndex];
            var oldToNewIndexMap = oldToNewIndexMapByMesh[meshIndex];

            for (var i = 0; i < meshDescription.Indices.Length; i++)
            {
                if (uniqueVertexMap.TryGetValue(vertexCompares[i], out var duplicateIndex))
                {
                    meshDescription.Indices[i] = duplicateIndex;
                }
                else
                {
                    oldToNewIndexMap.Add(i, uniqueVertexCurrentIndex);
                    uniqueVertexMap.Add(vertexCompares[i], uniqueVertexCurrentIndex);
                    meshDescription.Indices[i] = uniqueVertexCurrentIndex++;
                }
            }

            meshDescription.VertexCount[0] = uniqueVertexCurrentIndex;
            uniqueVertexMap.Dispose();
        }
    }

    /// <summary>
    /// Move vertex data of unique vertices into their new indices
    /// </summary>
    [BurstCompile]
    internal struct MoveVertexDataJob : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] private readonly NativeArray<MeshDescription> meshDescriptions;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<UnsafeParallelHashMap<int, int>> oldToNewIndexMapByMesh;

        public MoveVertexDataJob(NativeArray<MeshDescription> meshDescriptions, NativeArray<UnsafeParallelHashMap<int, int>> oldToNewIndexMapByMesh)
        {
            this.meshDescriptions = meshDescriptions;
            this.oldToNewIndexMapByMesh = oldToNewIndexMapByMesh;
        }

        public void Execute(int meshIndex)
        {
            var meshDescription = meshDescriptions[meshIndex];
            var oldToNewIndexMap = oldToNewIndexMapByMesh[meshIndex];
            foreach (var entry in oldToNewIndexMap)
            {
                MoveVertexData(meshDescription, entry.Key, entry.Value);
            }
        }

        private static unsafe void MoveVertexData(MeshDescription meshDescription, int fromIndex, int toIndex)
        {
            var vertexDataPtr = (byte*)meshDescription.VertexData.GetUnsafePtr();
            var skinningDataPtr = (byte*)meshDescription.SkinningData.GetUnsafePtr();
            var vertexSkinningDataSize = meshDescription.BoneInfluenceCount * MeshConversionData.SkinningDataSize;
            UnsafeUtility.MemMove(vertexDataPtr + toIndex * meshDescription.VertexSize,
                vertexDataPtr + fromIndex * meshDescription.VertexSize,
                meshDescription.VertexSize);

            if (meshDescription.SkinningData.Length > 0)
            {
                UnsafeUtility.MemMove(skinningDataPtr + toIndex * vertexSkinningDataSize,
                    skinningDataPtr + fromIndex * vertexSkinningDataSize, vertexSkinningDataSize);
            }
        }
    }
}
