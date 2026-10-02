using System;
using System.Collections.Generic;
using pxr;
using Unity.Collections;
using Unity.Importer.USD.GeomJobs;
using Unity.Jobs;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will use the provided usdMeshes and convert them to Unity skinned meshes.
    /// </summary>
    [NodeMetadata("ReadSkinnedMeshNode", 7)]
    public class ReadSkinnedMeshNode : Node<ReadSkinnedMeshNode.InputPort, ReadSkinnedMeshNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadSkinnedMeshNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// This structure defines the mappings between Primvar names in UsdGeomMesh and vertex channels in the resulting MeshDescriptions.
            /// </summary>
            public PrimvarToVertexAttributeMapping PrimVarMapping;

            /// <summary>
            /// List of USD meshes to convert. This can include mesh prims that are USD references.
            /// </summary>
            public List<UsdPrim> UsdMeshes;

            /// <summary>
            /// List of USD blendshapes to convert.
            /// </summary>
            public Dictionary<string, UsdSkelBlendShape[]> UsdSkelBlendShapes;

            /// <summary>
            /// Skinned mesh data used for computing vertex weight and joint indices.
            /// </summary>
            public Dictionary<string, UsdSkinnedMeshData> SkinnedMeshData;

            /// <summary>
            /// USD stage metadata.
            /// </summary>
            public UsdStageMetadata usdMetadata;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadSkinnedMeshNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// Prim path to MeshDescription. MeshDescriptions are unique in this collection. Instances are not included.
            /// </summary>
            public Dictionary<string, MeshDescription> PathToMeshDescription;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            var prototypes = new Dictionary<string, UsdGeomMesh>();
            foreach (var meshPrim in Input.UsdMeshes)
            {
                if (meshPrim.IsInstanceProxy())
                {
                    var prototypePath = meshPrim.GetPrimInPrototype().GetPath();
                    if (!prototypes.ContainsKey(prototypePath))
                    {
                        prototypes.Add(prototypePath, new UsdGeomMesh(meshPrim.GetPrimInPrototype()));
                    }
                }
                else
                {
                    prototypes.Add(meshPrim.GetPath(), new UsdGeomMesh(meshPrim));
                }
            }

            ConvertMeshes(prototypes);
        }

        private void ConvertMeshes(Dictionary<string, UsdGeomMesh> prototypes)
        {
            var meshDescriptions = new NativeArray<MeshDescription>(prototypes.Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var meshConversionData = new NativeArray<MeshConversionData>(prototypes.Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            try
            {
                var timeCode = UsdTimeCode.EarliestTime();
                var primVarMappingTokens = new PrimvarToVertexAttributeMappingToken(Input.PrimVarMapping);
                var i = 0;
                var prototypePaths = new string[prototypes.Count];
                Output.PathToMeshDescription = new Dictionary<string, MeshDescription>();
                var stageConversionData = new StageConversionData() { ScaleFactor =  Input.usdMetadata.metersPerUnit, IsZup = Input.usdMetadata.isStageZup};


                foreach (var prototype in prototypes)
                {
                    prototypePaths[i] = prototype.Key;
                    UsdSkelBlendShape[] blendShapes = null;
                    Input.UsdSkelBlendShapes?.TryGetValue(prototype.Key, out blendShapes);
                    UsdSkinnedMeshData skinnedMeshData = default;
                    Input.SkinnedMeshData?.TryGetValue(prototype.Key, out skinnedMeshData);

                    (meshDescriptions[i], meshConversionData[i]) = MeshConversionUtils.InitializeMeshDescriptionAndConversionData(prototype.Value, timeCode,
                        stageConversionData, primVarMappingTokens, blendShapes, skinnedMeshData, Input.GraphLogger);

                    i++;
                }

                if (prototypes.Count > 0)
                {
                    var jobHandle = new ComputeFaceIndexToIndexRangesJob(meshConversionData).Schedule(prototypes.Count, 1);
                    jobHandle = new ComputeFaceIndexToIndexRangesJob(meshConversionData).Schedule(prototypes.Count, 1, jobHandle);
                    jobHandle = new TriangulateCounterClockwiseJob(meshConversionData, meshDescriptions).Schedule(prototypes.Count, 1, jobHandle);
                    jobHandle = new FlattenPrimvarsJob(meshConversionData, meshDescriptions).Schedule(prototypes.Count, 1, jobHandle);
                    jobHandle = new ReorderIndicesJob(meshConversionData, meshDescriptions).Schedule(prototypes.Count, 1, jobHandle);
                    jobHandle = new WriteBlendShapesJob(stageConversionData, meshConversionData, meshDescriptions).Schedule(prototypes.Count, 1, jobHandle);
                    jobHandle = new WriteSkinnedMeshVerticesJob(stageConversionData, meshConversionData, meshDescriptions).Schedule(prototypes.Count, 1, jobHandle);
                    jobHandle.Complete();
                }

                i = 0;
                foreach (var path in prototypePaths)
                {
                    Output.PathToMeshDescription.Add(path, meshDescriptions[i]);
                    i++;
                }
            }
            catch (Exception e)
            {
                Input.GraphLogger.LogImportError(e.Message, null, NodeErrors.ReadSkinnedMeshError);
                throw;
            }
            finally
            {
                foreach (var meshConversionDataValue in meshConversionData)
                {
                    meshConversionDataValue.Dispose();
                }

                meshConversionData.Dispose();
                meshDescriptions.Dispose();
            }
        }
    }
}
