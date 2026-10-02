using System;
using System.Collections.Generic;
using System.IO;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Importer;
using UnityEngine.Rendering;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will use the provided MeshDescriptions and convert them to Unity meshes.
    /// </summary>
    [NodeMetadata("WriteMeshNode", 3)]
    public class WriteMeshNode : Node<WriteMeshNode.InputPort, WriteMeshNode.OutputPort>
    {
        private static readonly MeshUpdateFlags meshUpdateFlags = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontResetBoneBounds;
        /// <summary>
        /// Input ports of the <see cref="WriteMeshNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// Path to MeshDescriptions. Mesh Descriptions are considered unique in this collection.
            /// </summary>
            public Dictionary<string, MeshDescription> pathToMeshDescription;
        }

        /// <summary>
        /// Output ports of the <see cref="WriteMeshNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// Unique meshes only.
            /// </summary>
            public Dictionary<string, Mesh> pathToUnityMesh = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            var outMeshes = new Mesh[Input.pathToMeshDescription.Count];
            var meshDescriptions = new MeshDescription[Input.pathToMeshDescription.Count];
            int i = 0;
            foreach (var kvp in Input.pathToMeshDescription)
            {
                meshDescriptions[i] = kvp.Value;

                var mesh = new Mesh() { name = Path.GetFileNameWithoutExtension(kvp.Key) };
                outMeshes[i++] = mesh;
                Output.pathToUnityMesh.Add(kvp.Key, mesh);
            }

            WriteMeshDescriptionsToMeshes(meshDescriptions, outMeshes);

            foreach (var kvp in Input.pathToMeshDescription)
            {
                kvp.Value.Dispose();
            }
        }

        private static void WriteMeshDescriptionsToMeshes(MeshDescription[] meshDescriptions, Mesh[] unityMeshes)
        {
            var meshDataArray = Mesh.AllocateWritableMeshData(meshDescriptions.Length);

            for (int i = 0; i < meshDescriptions.Length; i++)
            {
                var meshDescription = meshDescriptions[i];
                meshDataArray[i].SetVertexBufferParams(meshDescription.VertexCount[0], meshDescription.VertexAttributeDescriptors);
            }

            var populateMeshDataArray = new PopulateMeshDataArrayJob()
            {
                meshDataArray = meshDataArray,
                meshDescriptions = new NativeArray<MeshDescription>(meshDescriptions, Allocator.TempJob)
            };

            var jobHandle = populateMeshDataArray.Schedule(populateMeshDataArray.meshDescriptions.Length, populateMeshDataArray.meshDescriptions.Length / 16);
            jobHandle.Complete();

            Mesh.ApplyAndDisposeWritableMeshData(populateMeshDataArray.meshDataArray, unityMeshes, meshUpdateFlags);

            for (var i = 0; i < populateMeshDataArray.meshDescriptions.Length; i++)
            {
                var bounds = populateMeshDataArray.meshDescriptions[i].Bounds;
                if (bounds.extents.sqrMagnitude == 0 && unityMeshes[i].vertexCount > 0)
                    unityMeshes[i].RecalculateBounds();
                else
                    unityMeshes[i].bounds = bounds;
            }

            for (var i = 0; i < populateMeshDataArray.meshDescriptions.Length; i++)
            {
                var mesh = unityMeshes[i];
                var blendShapes = populateMeshDataArray.meshDescriptions[i].BlendShapeDescriptions;

                for (var j = 0; j < blendShapes.Length; j++)
                {
                    for (var k = 0; k < blendShapes[j].FrameCount; k++)
                    {
                        AddBlendShapeFrame(mesh, populateMeshDataArray.meshDescriptions[i], j, k);
                    }
                }

                if (populateMeshDataArray.meshDescriptions[i].BoneInfluenceCount > 4)
                {
                    SetMoreThan4BoneWeights(unityMeshes[i], meshDescriptions[i]);
                }

                if (populateMeshDataArray.meshDescriptions[i].JointsBindingMatrices.Length > 0)
                {
                    mesh.bindposes = populateMeshDataArray.meshDescriptions[i].JointsBindingMatrices.Reinterpret<Matrix4x4>().ToArray();
                }
            }
            populateMeshDataArray.meshDescriptions.Dispose();
        }

        private static void SetMoreThan4BoneWeights(Mesh mesh, MeshDescription meshDescription)
        {
            var vertexCount = meshDescription.VertexCount[0];
            var bonesPerVertexArray = new NativeArray<byte>(vertexCount , Allocator.TempJob);
            var weightsArray = new NativeArray<BoneWeight1>(
                vertexCount * meshDescription.BoneInfluenceCount, Allocator.TempJob);

            var populateWeightArraysJob = new PopulateWeightsArraysJob()
            {
                weightsArray = weightsArray,
                bonesPerVertexArray = bonesPerVertexArray,
                boneInfluenceCount = meshDescription.BoneInfluenceCount,
                skinningData = meshDescription.SkinningData
            };
            populateWeightArraysJob.Schedule(vertexCount, 128).Complete();

            mesh.SetBoneWeights(bonesPerVertexArray, weightsArray);
            bonesPerVertexArray.Dispose();
            weightsArray.Dispose();
        }

        private static void AddBlendShapeFrame(Mesh mesh, MeshDescription meshDescription, int blendShapeIndex, int frameIndex)
        {
            var deltaVertices = new Vector3[mesh.vertexCount];
            var deltaNormals = new Vector3[mesh.vertexCount];
            var deltaTangents = new Vector3[mesh.vertexCount];
            var blendShape = meshDescription.BlendShapeDescriptions[blendShapeIndex];
            var baseIndex = frameIndex * meshDescription.Indices.Length;

            for (var i = 0; i < meshDescription.Indices.Length; i++)
            {
                var dataIndex = baseIndex + i;
                var vertexIndex = meshDescription.Indices[i];

                var float3 = blendShape.DeltaVertices[dataIndex];
                deltaVertices[vertexIndex].Set(float3.x, float3.y, float3.z);

                float3 = blendShape.DeltaNormals[dataIndex];
                deltaNormals[vertexIndex].Set(float3.x, float3.y, float3.z);

                //TODO compute delta tangents
                deltaTangents[vertexIndex].Set(0f, 0f, 0f);
            }

            mesh.AddBlendShapeFrame(
                new string(blendShape.Name.ToArray()),
                blendShape.Weights[frameIndex],
                deltaVertices,
                deltaNormals,
                deltaTangents
            );
        }
    }

    [BurstCompile]
    internal struct PopulateWeightsArraysJob : IJobParallelFor
    {
        [NoAlias, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
        public NativeArray<BoneWeight1> weightsArray;

        [NoAlias, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
        public NativeArray<byte> bonesPerVertexArray;

        [ReadOnly]
        public int boneInfluenceCount;

        [NativeDisableContainerSafetyRestriction]
        [ReadOnly]
        public NativeArray<byte> skinningData;


        public unsafe void Execute(int i)
        {
            var skinningDataOffset = i * boneInfluenceCount * MeshConversionData.SkinningDataSize;
            var skinningDataPtr = (byte*)skinningData.GetUnsafePtr() + skinningDataOffset;
            var blendWeightsSize = boneInfluenceCount * sizeof(float);
            var weightsOffset = i * boneInfluenceCount;

            var jointWeightCompares = new NativeArray<WeightCompare>(boneInfluenceCount, Allocator.Temp);
            for (var k = 0; k < boneInfluenceCount; k++)
            {
                jointWeightCompares[k] = new WeightCompare()
                {
                    Weight = *(float*)(skinningDataPtr + sizeof(float) * k),
                    Index = k
                };
            }

            jointWeightCompares.Sort();

            for (var k = 0; k < boneInfluenceCount; k++)
            {
                var weightIndex = jointWeightCompares[k].Index;
                weightsArray[weightsOffset + k] = new BoneWeight1()
                {
                    weight = *(float*)(skinningDataPtr + sizeof(float) * weightIndex),
                    boneIndex = *(int*)(skinningDataPtr + blendWeightsSize + sizeof(int) * weightIndex)
                };
            }

            bonesPerVertexArray[i] = (byte)(boneInfluenceCount);
        }
    }

    internal struct WeightCompare : IComparable<WeightCompare>
    {
        public float Weight;
        public int Index;

        public int CompareTo(WeightCompare other)
        {
            return Weight - other.Weight < 0 ? 1 : -1;
        }
    }

    [BurstCompile]
    internal struct PopulateMeshDataArrayJob : IJobParallelFor
    {
        private static readonly MeshUpdateFlags meshUpdateFlags = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontResetBoneBounds;
        public Mesh.MeshDataArray meshDataArray;

        [NoAlias, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
        [ReadOnly]
        public NativeArray<MeshDescription> meshDescriptions;

        public unsafe void Execute(int i)
        {
            var mesh = meshDataArray[i];
            var meshDescription = meshDescriptions[i];

            mesh.SetIndexBufferParams(meshDescription.Indices.Length, IndexFormat.UInt32);
            var indicesOut = mesh.GetIndexData<int>();
            UnsafeUtility.MemCpy(indicesOut.GetUnsafePtr(), (int*)meshDescription.Indices.GetUnsafeReadOnlyPtr(), meshDescription.Indices.Length * sizeof(int));

            // copy vertex data into separate streams according to https://docs.unity3d.com/2023.2/Documentation/ScriptReference/Rendering.VertexAttributeDescriptor.html
            byte* vertexDataStream0 = (byte*)mesh.GetVertexData<byte>().GetUnsafePtr();
            UnsafeUtility.MemCpyStride(vertexDataStream0, meshDescription.VertexPositionAndNormalSize,
                (byte*)meshDescription.VertexData.GetUnsafeReadOnlyPtr(), meshDescription.VertexSize,
                meshDescription.VertexPositionAndNormalSize, meshDescription.VertexCount[0]);

            if (meshDescription.VertexColorAndUVSize > 0)
            {
                byte* vertexDataStream1 = (byte*)mesh.GetVertexData<byte>(1).GetUnsafePtr();
                UnsafeUtility.MemCpyStride(vertexDataStream1, meshDescription.VertexColorAndUVSize,
                    (byte*)meshDescription.VertexData.GetUnsafeReadOnlyPtr() +
                    meshDescription.VertexPositionAndNormalSize, meshDescription.VertexSize,
                    meshDescription.VertexColorAndUVSize, meshDescription.VertexCount[0]);
            }

            if (meshDescription.SkinningData.Length > 0 && meshDescription.BoneInfluenceCount <= 4)
            {
                byte* skinningData = (byte*)mesh.GetVertexData<byte>(meshDescription.skinningDataStream).GetUnsafePtr();
                UnsafeUtility.MemCpy(skinningData, (byte*)meshDescription.SkinningData.GetUnsafeReadOnlyPtr(), meshDescription.VertexCount[0] * meshDescription.BoneInfluenceCount * (sizeof(float) + sizeof(int)));
            }

            mesh.subMeshCount = meshDescription.SubMeshDescriptors.Length;
            for (int j = 0; j < meshDescription.SubMeshDescriptors.Length; j++)
            {
                mesh.SetSubMesh(j, meshDescription.SubMeshDescriptors[j], meshUpdateFlags);
            }
        }
    }
}
