using System;
using pxr;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Importer;
using UnityEngine.Rendering;


namespace Unity.Importer.USD
{
    internal static class MeshConversionUtils
    {
        private static TfToken k_normal3fArrayToken = new("normal3f[]");
        private static TfToken k_Float3ArrayToken = new("float3[]");
        private static TfToken k_TexCoord2FArrayToken = new("texCoord2f[]");
        private static TfToken k_Float2ArrayToken = new("float2[]");
        private static TfToken k_Color3FArrayToken = new("color3f[]");
        private static TfToken k_Color4FArrayToken = new("color4f[]");

        public static PrimVarInterpolation TfTokenToPrimvarInterpolation(TfToken interpolation)
        {
            if (interpolation == UsdGeomTokens.constant) return PrimVarInterpolation.Constant;
            if (interpolation == UsdGeomTokens.uniform) return PrimVarInterpolation.Uniform;
            if (interpolation == UsdGeomTokens.vertex) return PrimVarInterpolation.Vertex;
            if (interpolation == UsdGeomTokens.varying) return PrimVarInterpolation.Varying;
            if (interpolation == UsdGeomTokens.faceVarying) return PrimVarInterpolation.FaceVarying;
            return PrimVarInterpolation.Undefined;
        }

        internal static unsafe NativeArray<Subset> ReadGeomSubsets(UsdGeomMesh prim, UsdTimeCode timeCode, int faceCount)
        {
            var binding = new UsdShadeMaterialBindingAPI(prim);

            var subsets = binding.GetMaterialBindSubsets();

            if (subsets.Count == 0)
            {
                return default;
            }

            var unassignedIndices = UsdGeomSubset.GetUnassignedIndices(subsets, (uint)faceCount);
            var subsetCount = subsets.Count + (unassignedIndices.size() > 0 ? 1 : 0);

            var result = new NativeArray<Subset>(subsetCount, Allocator.TempJob);
            for (var i = 0; i < subsets.Count; i++)
            {
                var indicesAttr = subsets[i].GetIndicesAttr();
                VtIntArray indices = indicesAttr.Get(timeCode);
                var subset = result[i];
                subset.FaceIndices = new NativeArray<int>((int)indices.size(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                indices.CopyToArray((IntPtr)subset.FaceIndices.GetUnsafePtr());
                result[i] = subset;
            }

            //Add the unassigned indices as another submesh
            if (unassignedIndices.size() > 0)
            {
                var subset = result[subsetCount - 1];
                subset.FaceIndices = new NativeArray<int>((int)unassignedIndices.size(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                unassignedIndices.CopyToArray((IntPtr)subset.FaceIndices.GetUnsafePtr());
                result[subsetCount - 1] = subset;
            }

            return result;
        }

        private static unsafe PrimVarData<float2> ReadFromVtVec2fArrayPrimvar(UsdGeomMesh usdMesh, UsdTimeCode timeCode, TfToken[] vertexAttributeMapping, int outValueSize)
        {
            var primvarsAPI = new UsdGeomPrimvarsAPI(usdMesh);
            foreach (var mappingString in vertexAttributeMapping)
            {
                var primvar = primvarsAPI.GetPrimvar(mappingString);
                if (primvar.IsDefined())
                {
                    var tokenType = primvar.GetTypeName().GetAsToken();
                    if (tokenType == k_TexCoord2FArrayToken || tokenType == k_Float2ArrayToken)
                    {
                        var val = new VtValue();
                        primvar.ComputeFlattened(val, timeCode);
                        VtVec2fArray attr = val;
                        var values = new NativeArray<float2>((int)attr.size(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        attr.CopyToArray((IntPtr)values.GetUnsafePtr());
                        return new PrimVarData<float2>()
                        {
                            Values = values,
                            Interpolation = TfTokenToPrimvarInterpolation(primvar.GetInterpolation()),
                            OutValues = new NativeArray<float2>(outValueSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
                        };
                    }
                }
            }
            // hack : Need to initialize for jobs
            return new PrimVarData<float2>()
            {
                Values = new NativeArray<float2>(0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                OutValues = new NativeArray<float2>(0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
            };
        }

        private static unsafe PrimVarData<float3> ReadFromVtVec3fArrayPrimvar(UsdGeomMesh usdMesh, UsdTimeCode timeCode, TfToken[] vertexAttributeMapping, int outValueSize)
        {
            var primvarsAPI = new UsdGeomPrimvarsAPI(usdMesh);
            foreach (var mappingString in vertexAttributeMapping)
            {
                var primvar = primvarsAPI.GetPrimvar(mappingString);
                if (primvar.IsDefined())
                {
                    var tokenType = primvar.GetTypeName().GetAsToken();
                    if (tokenType == k_normal3fArrayToken || tokenType == k_Float3ArrayToken)
                    {
                        var val = new VtValue();
                        primvar.ComputeFlattened(val, timeCode);
                        VtVec3fArray attr = val;
                        var values = new NativeArray<float3>((int)attr.size(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        attr.CopyToArray((IntPtr)values.GetUnsafePtr());
                        return new PrimVarData<float3>()
                        {
                            Values = values,
                            Interpolation = TfTokenToPrimvarInterpolation(primvar.GetInterpolation()),
                            OutValues = new NativeArray<float3>(outValueSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
                        };
                    }
                }
            }
            // hack : Need to initialize for jobs
            return new PrimVarData<float3>()
            {
                Values = new NativeArray<float3>(0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                OutValues = new NativeArray<float3>(0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
            };
        }

        private static unsafe PrimVarData<float4> ReadFromColor3fArrayPrimvar(UsdGeomMesh usdMesh, UsdTimeCode timeCode, TfToken[] vertexAttributeMapping, int outValueSize)
        {
            var primvarsAPI = new UsdGeomPrimvarsAPI(usdMesh);
            foreach (var mappingString in vertexAttributeMapping)
            {
                var primvar = primvarsAPI.GetPrimvar(mappingString);
                if (primvar.IsDefined())
                {
                    var tokenType = primvar.GetTypeName().GetAsToken();
                    if (tokenType == k_Color3FArrayToken)
                    {
                        var val = new VtValue();
                        primvar.ComputeFlattened(val, timeCode);
                        VtVec3fArray attr = val;
                        var size = (int)attr.size();
                        var values = new NativeArray<float3>(size, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        attr.CopyToArray((IntPtr)values.GetUnsafePtr());

                        var primVarData = new PrimVarData<float4>()
                        {
                            Values = new NativeArray<float4>(size, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                            Interpolation = TfTokenToPrimvarInterpolation(primvar.GetInterpolation()),
                            OutValues = new NativeArray<float4>(outValueSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
                        };

                        for (var i = 0; i < size; i++)
                        {
                            primVarData.Values[i] = new float4(values[i], 1.0f);
                        }

                        values.Dispose();

                        return primVarData;
                    }
                }
            }
            // hack : Need to initialize for jobs
            return new PrimVarData<float4>()
            {
                Values = new NativeArray<float4>(0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                OutValues = new NativeArray<float4>(0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
            };
        }

        private static unsafe PrimVarData<float4> ReadFromColor4fArrayPrimvar(UsdGeomMesh usdMesh, UsdTimeCode timeCode, TfToken[] vertexAttributeMapping, int outValueSize)
        {
            var primvarsAPI = new UsdGeomPrimvarsAPI(usdMesh);
            foreach (var mappingString in vertexAttributeMapping)
            {
                var primvar = primvarsAPI.GetPrimvar(mappingString);
                if (primvar.IsDefined())
                {
                    var tokenType = primvar.GetTypeName().GetAsToken();
                    if (tokenType == k_Color4FArrayToken)
                    {
                        var val = new VtValue();
                        primvar.ComputeFlattened(val, timeCode);
                        VtVec4fArray attr = val;
                        var values = new NativeArray<float4>((int)attr.size(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        attr.CopyToArray((IntPtr)values.GetUnsafePtr());
                        return new PrimVarData<float4>()
                        {
                            Values = values,
                            Interpolation = TfTokenToPrimvarInterpolation(primvar.GetInterpolation()),
                            OutValues = new NativeArray<float4>(outValueSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
                        };
                    }
                }
            }
            // hack : Need to initialize for jobs
            return new PrimVarData<float4>()
            {
                Values = new NativeArray<float4>(0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                OutValues = new NativeArray<float4>(0, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
            };
        }

        private static VertexAttributeDescriptor[] FromVertexAttributesFlags(VertexAttributesFlags e, int weightsElementSize, int skinningDataStream, ref NativeArray<bool> attributeExists)
        {
            var descs = new VertexAttributeDescriptor[math.countbits((int)e) + 1];
            attributeExists[0] = e.HasFlag(VertexAttributesFlags.Normal);
            attributeExists[1] = e.HasFlag(VertexAttributesFlags.Color);
            attributeExists[2] = e.HasFlag(VertexAttributesFlags.TexCoord0);
            attributeExists[3] = e.HasFlag(VertexAttributesFlags.TexCoord1);
            attributeExists[4] = e.HasFlag(VertexAttributesFlags.TexCoord2);
            attributeExists[5] = e.HasFlag(VertexAttributesFlags.TexCoord3);
            attributeExists[6] = e.HasFlag(VertexAttributesFlags.TexCoord4);
            attributeExists[7] = e.HasFlag(VertexAttributesFlags.TexCoord5);
            attributeExists[8] = e.HasFlag(VertexAttributesFlags.TexCoord6);
            attributeExists[9] = e.HasFlag(VertexAttributesFlags.TexCoord7);

            descs[0] = new VertexAttributeDescriptor(VertexAttribute.Position);
            var idx = 1;
            if (attributeExists[0]) { descs[idx++] = new VertexAttributeDescriptor(VertexAttribute.Normal); }
            if (attributeExists[1]) { descs[idx++] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, 1); }
            if (attributeExists[2]) { descs[idx++] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 1); }
            if (attributeExists[3]) { descs[idx++] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, 1); }
            if (attributeExists[4]) { descs[idx++] = new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, 2, 1); }
            if (attributeExists[5]) { descs[idx++] = new VertexAttributeDescriptor(VertexAttribute.TexCoord3, VertexAttributeFormat.Float32, 2, 1); }
            if (attributeExists[6]) { descs[idx++] = new VertexAttributeDescriptor(VertexAttribute.TexCoord4, VertexAttributeFormat.Float32, 2, 1); }
            if (attributeExists[7]) { descs[idx++] = new VertexAttributeDescriptor(VertexAttribute.TexCoord5, VertexAttributeFormat.Float32, 2, 1); }
            if (attributeExists[8]) { descs[idx++] = new VertexAttributeDescriptor(VertexAttribute.TexCoord6, VertexAttributeFormat.Float32, 2, 1); }
            if (attributeExists[9]) { descs[idx++] = new VertexAttributeDescriptor(VertexAttribute.TexCoord7, VertexAttributeFormat.Float32, 2, 1); }

            // Allow bone influences only if there's 4 or less influences per vertex
            if (weightsElementSize <= 4)
            {
                attributeExists[10] = e.HasFlag(VertexAttributesFlags.BlendWeight);
                attributeExists[11] = e.HasFlag(VertexAttributesFlags.BlendIndices);
                if (attributeExists[10]) { descs[idx++] = new VertexAttributeDescriptor(VertexAttribute.BlendWeight, VertexAttributeFormat.Float32, weightsElementSize, skinningDataStream); }
                if (attributeExists[11]) { descs[idx] = new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.SInt32, weightsElementSize, skinningDataStream); }
            }
            return descs;
        }

        private static Bounds ExtentsToBounds(VtVec3fArray extents, float scaleFactor, bool isZup, bool isLeftHanded)
        {
            var yIndex = 1;
            var zIndex = 2;
            var multiplier = 1;
            if (isZup)
            {
                yIndex = 2;
                zIndex = 1;
            }
            else if (!isLeftHanded)
            {
                multiplier = -1;
            }
            var min = new float3(extents[0][0], extents[0][yIndex], extents[1][zIndex] * multiplier);
            var max = new float3(extents[1][0], extents[1][yIndex], extents[0][zIndex] * multiplier);

            if (math.any(min > max))
                return new Bounds();

            return new Bounds(((min + max) * scaleFactor) / 2, (max - min) * scaleFactor);
        }

        private static int GetTriangleCount(NativeArray<int> faceVertexCounts)
        {
            var count = 0;
            for (var i = 0; i < faceVertexCounts.Length; i++)
            {
                count += faceVertexCounts[i] - 2;
            }
            return count;
        }

        public static (MeshDescription, MeshConversionData) InitializeMeshDescriptionAndConversionData(UsdGeomMesh usdMesh, UsdTimeCode timeCode,
            StageConversionData stageConversionData, PrimvarToVertexAttributeMappingToken primVarMappingTokens,
            UsdSkelBlendShape[] blendShapesIn, UsdSkinnedMeshData usdSkinnedMeshData, GraphLogger GraphLogger)
        {
            MeshConversionData meshConversionData = new MeshConversionData();
            MeshDescription meshDescription = new MeshDescription();

            InitializeGeometry();
            InitializeWorldProperties();
            InitializeBounds();
            InitializeSubMeshes();
            InitializeBlendShapes();
            InitializeSkinnedMesh();
            InitializeVertexAttributes();
            InitializeMeshDescriptionVertexData();

            return (meshDescription, meshConversionData);


            unsafe void InitializeGeometry()
            {
                VtIntArray indices = usdMesh.GetFaceVertexIndicesAttr().Get(timeCode);
                meshConversionData.FaceVertexIndices = new NativeArray<int>((int)indices.size(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                indices.CopyToArray((IntPtr)meshConversionData.FaceVertexIndices.GetUnsafePtr());

                VtIntArray faceCounts = usdMesh.GetFaceVertexCountsAttr().Get(timeCode);
                meshConversionData.FaceVertexCounts = new NativeArray<int>((int)faceCounts.size(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                faceCounts.CopyToArray((IntPtr)meshConversionData.FaceVertexCounts.GetUnsafePtr());

                meshConversionData.TriangulatedIndexCount = GetTriangleCount(meshConversionData.FaceVertexCounts) * 3;
                meshDescription.Indices = new NativeArray<int>(meshConversionData.TriangulatedIndexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                VtVec3fArray pointsIn = usdMesh.GetPointsAttr().Get(timeCode);
                meshConversionData.Points = new NativeArray<float3>((int)pointsIn.size(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                pointsIn.CopyToArray((IntPtr)meshConversionData.Points.GetUnsafePtr());

                VtVec3fArray normalsIn = usdMesh.GetNormalsAttr().Get(timeCode);

                if (normalsIn.size() > 0)
                {
                    meshConversionData.Normals = new PrimVarData<float3>()
                    {
                        Values = new NativeArray<float3>((int)normalsIn.size(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                        Interpolation = TfTokenToPrimvarInterpolation(usdMesh.GetNormalsInterpolation()),
                        OutValues = new NativeArray<float3>(meshConversionData.TriangulatedIndexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory)
                    };
                    normalsIn.CopyToArray((IntPtr)meshConversionData.Normals.Values.GetUnsafePtr());
                }
                else
                {
                    // fallback to primvar:normals
                    meshConversionData.Normals = ReadFromVtVec3fArrayPrimvar(usdMesh, timeCode, new[] { new TfToken("normals")}, meshConversionData.TriangulatedIndexCount);
                }

                meshConversionData.FaceIndexToIndexRanges = new NativeArray<int2>(meshConversionData.FaceVertexCounts.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                meshConversionData.SubMeshReorderedIndicesMapping = new NativeArray<int>(meshConversionData.TriangulatedIndexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            }

            unsafe void InitializeVertexAttributes()
            {
                meshConversionData.Colors = ReadFromColor4fArrayPrimvar(usdMesh, timeCode, primVarMappingTokens.Color, meshConversionData.TriangulatedIndexCount);
                if (meshConversionData.Colors.Values.Length == 0)
                {
                    meshConversionData.Colors.Dispose();
                    meshConversionData.Colors = ReadFromColor3fArrayPrimvar(usdMesh, timeCode, primVarMappingTokens.Color, meshConversionData.TriangulatedIndexCount);
                }

                meshConversionData.Uv0 = ReadFromVtVec2fArrayPrimvar(usdMesh, timeCode, primVarMappingTokens.TextCoord0, meshConversionData.TriangulatedIndexCount);
                meshConversionData.Uv1 = ReadFromVtVec2fArrayPrimvar(usdMesh, timeCode, primVarMappingTokens.TextCoord1, meshConversionData.TriangulatedIndexCount);
                meshConversionData.Uv2 = ReadFromVtVec2fArrayPrimvar(usdMesh, timeCode, primVarMappingTokens.TextCoord2, meshConversionData.TriangulatedIndexCount);
                meshConversionData.Uv3 = ReadFromVtVec2fArrayPrimvar(usdMesh, timeCode, primVarMappingTokens.TextCoord3, meshConversionData.TriangulatedIndexCount);
                meshConversionData.Uv4 = ReadFromVtVec2fArrayPrimvar(usdMesh, timeCode, primVarMappingTokens.TextCoord4, meshConversionData.TriangulatedIndexCount);
                meshConversionData.Uv5 = ReadFromVtVec2fArrayPrimvar(usdMesh, timeCode, primVarMappingTokens.TextCoord5, meshConversionData.TriangulatedIndexCount);
                meshConversionData.Uv6 = ReadFromVtVec2fArrayPrimvar(usdMesh, timeCode, primVarMappingTokens.TextCoord6, meshConversionData.TriangulatedIndexCount);
                meshConversionData.Uv7 = ReadFromVtVec2fArrayPrimvar(usdMesh, timeCode, primVarMappingTokens.TextCoord7, meshConversionData.TriangulatedIndexCount);

                var vertexAttributes = new VertexAttributesFlags();

                meshDescription.VertexPositionAndNormalSize = sizeof(float3);
                meshDescription.VertexColorAndUVSize = 0;
                if (meshConversionData.Normals.Values.Length > 0) { vertexAttributes |= VertexAttributesFlags.Normal; meshDescription.VertexPositionAndNormalSize += sizeof(float3);}
                if (meshConversionData.Colors.Values.Length > 0) { vertexAttributes |= VertexAttributesFlags.Color; meshDescription.VertexColorAndUVSize += sizeof(float4); }
                if (meshConversionData.Uv0.Values.Length > 0) { vertexAttributes |= VertexAttributesFlags.TexCoord0; meshDescription.VertexColorAndUVSize += sizeof(float2); }
                if (meshConversionData.Uv1.Values.Length > 0) { vertexAttributes |= VertexAttributesFlags.TexCoord1; meshDescription.VertexColorAndUVSize += sizeof(float2); }
                if (meshConversionData.Uv2.Values.Length > 0) { vertexAttributes |= VertexAttributesFlags.TexCoord2; meshDescription.VertexColorAndUVSize += sizeof(float2); }
                if (meshConversionData.Uv3.Values.Length > 0) { vertexAttributes |= VertexAttributesFlags.TexCoord3; meshDescription.VertexColorAndUVSize += sizeof(float2); }
                if (meshConversionData.Uv4.Values.Length > 0) { vertexAttributes |= VertexAttributesFlags.TexCoord4; meshDescription.VertexColorAndUVSize += sizeof(float2); }
                if (meshConversionData.Uv5.Values.Length > 0) { vertexAttributes |= VertexAttributesFlags.TexCoord5; meshDescription.VertexColorAndUVSize += sizeof(float2); }
                if (meshConversionData.Uv6.Values.Length > 0) { vertexAttributes |= VertexAttributesFlags.TexCoord6; meshDescription.VertexColorAndUVSize += sizeof(float2); }
                if (meshConversionData.Uv7.Values.Length > 0) { vertexAttributes |= VertexAttributesFlags.TexCoord7; meshDescription.VertexColorAndUVSize += sizeof(float2); }

                // Allow bone influences only if there's 4 or less influences per vertex
                if (meshDescription.SkinningData.Length > 0 && meshDescription.BoneInfluenceCount <= 4)
                {
                    vertexAttributes |= VertexAttributesFlags.BlendWeight;
                    vertexAttributes |= VertexAttributesFlags.BlendIndices;
                }

                meshConversionData.VertexAttributeExists = new NativeArray<bool>(12, Allocator.TempJob);
                meshDescription.VertexAttributeDescriptors = new NativeArray<VertexAttributeDescriptor>(
                    FromVertexAttributesFlags(vertexAttributes, meshDescription.BoneInfluenceCount,
                        meshDescription.skinningDataStream, ref meshConversionData.VertexAttributeExists),
                    Allocator.TempJob);
            }

            void InitializeWorldProperties()
            {
                var orientationAttr = usdMesh.GetOrientationAttr();
                meshConversionData.IsLeftHanded = orientationAttr != null && Vt.VtValueToTfToken(orientationAttr.Get()).Equals(UsdGeomTokens.leftHanded);
            }

            void InitializeBounds()
            {
                VtVec3fArray extents = usdMesh.GetExtentAttr().Get(timeCode);
                if (extents.size() >= 2)
                {
                    meshDescription.Bounds = ExtentsToBounds(extents, stageConversionData.ScaleFactor, stageConversionData.IsZup, meshConversionData.IsLeftHanded);
                }
                else
                    meshDescription.Bounds = new Bounds();
            }

            void InitializeSubMeshes()
            {
                meshConversionData.SubMeshes = ReadGeomSubsets(usdMesh, timeCode, (int)meshConversionData.FaceVertexCounts.Length);
                if (meshConversionData.SubMeshes.Length > 0)
                {
                    meshDescription.SubMeshDescriptors = new NativeArray<SubMeshDescriptor>(meshConversionData.SubMeshes.Length, Allocator.TempJob);
                }
                else
                {
                    meshDescription.SubMeshDescriptors = new NativeArray<SubMeshDescriptor>(1, Allocator.TempJob);
                }
            }

            void InitializeBlendShapes()
            {
                if (blendShapesIn != null)
                {
                    meshConversionData.BlendShapes = new NativeArray<BlendShape>(blendShapesIn.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    meshDescription.BlendShapeDescriptions = new NativeArray<BlendShapeDescription>(meshConversionData.BlendShapes.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

                    for (var i = 0; i < meshConversionData.BlendShapes.Length; i++)
                    {
                        var blendShapeIn = blendShapesIn[i];
                        meshConversionData.BlendShapes[i] = new BlendShape(blendShapeIn, timeCode, GraphLogger);
                        meshDescription.BlendShapeDescriptions[i] = new BlendShapeDescription(blendShapeIn, meshConversionData.TriangulatedIndexCount);
                    }
                }
                else
                {
                    meshConversionData.BlendShapes = default;
                    meshDescription.BlendShapeDescriptions = default;
                }
            }

            unsafe void InitializeSkinnedMesh()
            {
                if (!string.IsNullOrEmpty(usdSkinnedMeshData.skeletonRootPath))
                {
                    meshDescription.BoneInfluenceCount = usdSkinnedMeshData.weightsElementSize;

                    meshDescription.JointsBindingMatrices = new NativeArray<float4x4>(usdSkinnedMeshData.jointsBindingMatrices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                    for (var i = 0; i < usdSkinnedMeshData.jointsBindingMatrices.Length; i++)
                    {
                        var bindingMatrix = usdSkinnedMeshData.jointsBindingMatrices[i];
                        var column = bindingMatrix[3];
                        column.xyz *= stageConversionData.ScaleFactor;
                        usdSkinnedMeshData.jointsBindingMatrices[i][3] = column;
                    }

                    fixed(void* jointsBindingMatricesPointer = usdSkinnedMeshData.jointsBindingMatrices)
                    {
                        UnsafeUtility.MemCpy(meshDescription.JointsBindingMatrices.GetUnsafePtr(), jointsBindingMatricesPointer, usdSkinnedMeshData.jointsBindingMatrices.Length * sizeof(float4x4));
                    }
                    if (usdSkinnedMeshData.isWeightInterpolationConstant)
                    {
                        int indicesSize = usdSkinnedMeshData.weightsElementSize * meshConversionData.Points.Length;
                        meshConversionData.JointIndices = new NativeArray<int>(indicesSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        fixed(void* jointIndicesPointer = usdSkinnedMeshData.jointIndices)
                        {
                            UnsafeUtility.MemCpyReplicate(meshConversionData.JointIndices.GetUnsafePtr(), jointIndicesPointer, usdSkinnedMeshData.jointIndices.Length * sizeof(int), meshConversionData.Points.Length);
                        }
                    }
                    else
                    {
                        meshConversionData.JointIndices = new NativeArray<int>(usdSkinnedMeshData.jointIndices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        fixed(void* jointIndicesPointer = usdSkinnedMeshData.jointIndices)
                        {
                            UnsafeUtility.MemCpy(meshConversionData.JointIndices.GetUnsafePtr(), jointIndicesPointer, usdSkinnedMeshData.jointIndices.Length * sizeof(int));
                        }
                    }

                    if (usdSkinnedMeshData.meshToSkeletonJointIndices != null)
                    {
                        for (var i = 0; i < meshConversionData.JointIndices.Length; i++)
                        {
                            if (usdSkinnedMeshData.meshToSkeletonJointIndices.TryGetValue(meshConversionData.JointIndices[i], out var index))
                            {
                                meshConversionData.JointIndices[i] = index;
                            }
                            else
                            {
                                GraphLogger.LogImportWarning(
                                    $"Mesh {usdMesh.GetPath()} : joint index '{meshConversionData.JointIndices[i]}' does not have a mapping index provided by '{nameof(UsdSkinnedMeshData.meshToSkeletonJointIndices)}'. Will default to 0.",
                                    null, NodeWarnings.MeshJointNotMappingToSkeleton);
                                meshConversionData.JointIndices[i] = 0;
                            }
                        }
                    }

                    // Convert constant interpolation into vertex interpolation
                    if (usdSkinnedMeshData.isWeightInterpolationConstant)
                    {
                        int weightsSize = usdSkinnedMeshData.weightsElementSize * meshConversionData.Points.Length;
                        meshConversionData.Weights = new NativeArray<float>(weightsSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        fixed(void* weightsPointer = usdSkinnedMeshData.weights)
                        {
                            UnsafeUtility.MemCpyReplicate(meshConversionData.Weights.GetUnsafePtr(), weightsPointer, usdSkinnedMeshData.weights.Length * sizeof(float), meshConversionData.Points.Length);
                        }
                    }
                    else
                    {
                        meshConversionData.Weights = new NativeArray<float>(usdSkinnedMeshData.weights.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        fixed(void* weightsPointer = usdSkinnedMeshData.weights)
                        {
                            UnsafeUtility.MemCpy(meshConversionData.Weights.GetUnsafePtr(), weightsPointer, usdSkinnedMeshData.weights.Length * sizeof(float));
                        }
                    }

                    meshDescription.SkinningData = new NativeArray<byte>(meshConversionData.TriangulatedIndexCount * usdSkinnedMeshData.weightsElementSize * MeshConversionData.SkinningDataSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                }
                else
                {
                    meshDescription.JointsBindingMatrices = new NativeArray<float4x4>(0, Allocator.TempJob);
                    meshDescription.BoneInfluenceCount = 0;
                    meshConversionData.JointIndices = new NativeArray<int>(0, Allocator.TempJob);
                    meshConversionData.Weights = new NativeArray<float>(0, Allocator.TempJob);
                    meshDescription.SkinningData = new NativeArray<byte>(0, Allocator.Temp);
                }
            }

            void InitializeMeshDescriptionVertexData()
            {
                meshDescription.VertexCount = new NativeArray<int>(1, Allocator.TempJob);
                meshDescription.VertexData = new NativeArray<byte>(meshConversionData.TriangulatedIndexCount * meshDescription.VertexSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            }
        }
    }
}
