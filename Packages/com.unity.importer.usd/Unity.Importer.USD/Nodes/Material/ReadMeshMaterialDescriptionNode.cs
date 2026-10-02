using System;
using System.Collections.Generic;
using pxr;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Importer;
using USD.NET;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will read the mesh and sub-mesh information from a list of USD Prims and extract the info of
    /// which materials and display colors are assigned to each of them.
    /// </summary>
    [NodeMetadata("ReadMeshMaterialDescriptionNode", 3)]
    public class ReadMeshMaterialDescriptionNode : Node<ReadMeshMaterialDescriptionNode.InputPort, ReadMeshMaterialDescriptionNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadMeshMaterialDescriptionNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// List of USD meshes to convert. This can include mesh prims that are USD references.
            /// </summary>
            public List<UsdPrim> prims;

            /// <summary>
            /// The current Usd Stage.
            /// </summary>
            public UsdStage stage;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadMeshMaterialDescriptionNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A MeshPrimPath-MaterialPrimPaths mapping that will be used to map materials to their respective meshes.
            /// </summary>
            public Dictionary<string, List<string>> meshMaterialsPaths = new();

            /// <summary>
            /// A MeshPrimPath-DisplayColorMaterialDescription mapping that will be used to create materials.
            /// </summary>
            public Dictionary<string, DisplayColorMaterialDescription> displayColorMaterialDescriptions = new();
        }

        private readonly TfToken interpolationToken = new("interpolation");
        private readonly Dictionary<string, string> masterPathToPrimPathCache = new();

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            foreach (var prim in Input.prims)
            {
                if (!TryReadMaterialRef(prim))
                {
                    ExtractAndAssignDisplayColorMaterial(prim);
                }
            }
        }

        private bool TryReadMaterialRef(UsdPrim prim)
        {
            var api = new UsdShadeMaterialBindingAPI(prim);

            var mat = api.ComputeBoundMaterial(UsdShadeTokens.full);
            if (!mat.GetPrim().IsValid())
                mat = api.ComputeBoundMaterial(UsdShadeTokens.preview);
            if (!mat.GetPrim().IsValid())
                mat = api.ComputeBoundMaterial(UsdShadeTokens.allPurpose);

            var meshMaterialPath = mat.GetPath();
            //consider only the master prim path of each material to prevent duplicates
            var materialPrim = Input.stage.GetPrimAtPath(meshMaterialPath);
            meshMaterialPath = materialPrim.IsInstanceProxy() ? materialPrim.GetPrimInPrototype().GetPath() : meshMaterialPath;

            var materialPaths = ReadMaterialPaths(prim, meshMaterialPath);

            if (!IsMeshMaterialPathValid(materialPaths, meshMaterialPath))
                return false;

            if (materialPaths.Count == 0)
            {
                if (!meshMaterialPath.IsEmpty())
                {
                    Output.meshMaterialsPaths.Add(prim.GetPath(), new List<string> {meshMaterialPath});
                    return true;
                }
            }
            else
            {
                var materials = new List<string>();
                for (var index = 0; index < materialPaths.Count; index++)
                {
                    materials.Add(materialPaths[index]);
                }
                Output.meshMaterialsPaths.Add(prim.GetPath(), materials);
                return true;
            }

            return false;
        }

        private bool IsMeshMaterialPathValid(List<string> materialPaths, SdfPath meshMaterialPath)
        {
            foreach (var path in materialPaths)
            {
                if (!string.IsNullOrEmpty(path))
                    return true;
            }

            return !meshMaterialPath.IsEmpty();
        }

        private List<string> ReadMaterialPaths(UsdPrim prim, string fallbackMaterialPath)
        {
            var result = new List<string>();

            if (!new UsdGeomXformable(prim))
            {
                return result;
            }

            var binding = new UsdShadeMaterialBindingAPI(prim);

            var subsets = binding.GetMaterialBindSubsets();

            var value = new VtValue();
            var defaultTime = UsdTimeCode.EarliestTime();

            foreach (var subset in subsets)
            {
                if (!subset)
                {
                    continue;
                }

                var indices = subset.GetIndicesAttr();
                if (!indices.IsValid())
                {
                    continue;
                }

                if (!indices.Get(value, defaultTime))
                {
                    continue;
                }

                var intValue = Array.Empty<int>();
                IntrinsicTypeConverter.FromVtArray(value, ref intValue);

                var materialPath = "";
                var rel = new UsdShadeMaterialBindingAPI(subset).GetDirectBindingRel();
                if (rel.GetTargets().Count > 0)
                {
                    //consider only the master prim path of each material to prevent duplicates
                    var materialPrim = Input.stage.GetPrimAtPath(rel.GetTargets()[0].GetPrimPath());
                    materialPath = materialPrim.IsInstanceProxy() ? materialPrim.GetPrimInPrototype().GetPath() : materialPrim.GetPath();
                }
                else
                {
                    materialPath = fallbackMaterialPath;
                }

                result.Add(materialPath);
            }

            return result;
        }

        private void ExtractAndAssignDisplayColorMaterial(UsdPrim prim)
        {
            var primPath = prim.GetPath().ToString();

            //Check if this master prim path was handled previously and if so, reuse the materials path list and abort.
            var masterPrimPath = prim.IsInstanceProxy() ? prim.GetPrimInPrototype().GetPath() : prim.GetPath();
            if (masterPathToPrimPathCache.ContainsKey(masterPrimPath))
            {
                var cacheEntry = masterPathToPrimPathCache[masterPrimPath];
                Output.meshMaterialsPaths.Add(primPath, Output.meshMaterialsPaths[cacheEntry]);
                return;
            }

            var geomMesh = new UsdGeomMesh(prim);
            if (!TryExtractDisplayColors(geomMesh, out var colors, out var colorInterpolation))
                return;

            ExtractDisplayOpacities(geomMesh, out var opacities, out var opacityInterpolation);

            var subSets = ExtractSubsets(prim, out var faceCount);
            if (TryFlattenPrimVarIndices(prim, colorInterpolation, "primvars:displayColor:indices", faceCount, colors.Length, out var colorIndices)
                && TryFlattenPrimVarIndices(prim, opacityInterpolation, "primvars:displayOpacity:indices", faceCount, opacities.Length, out var opacityIndices))
            {
                AssignMaterials(prim, subSets, colors, colorIndices, opacities, opacityIndices);
                masterPathToPrimPathCache.Add(masterPrimPath, primPath);
            }
            else
            {
                Input.GraphLogger.LogImportWarning(
                    $"There was an interpolation error for displayColor and/or displayOpacity for prim '{primPath}'. Skipping display color extraction.",
                    null, NodeWarnings.WrongInterpolationForDisplayColorOpacity);
            }

            DisposeSubSets(subSets);
        }

        private NativeArray<Subset> ExtractSubsets(UsdPrim prim, out int faceCount)
        {
            var geomMesh = new UsdGeomMesh(prim);
            VtIntArray faceCounts = geomMesh.GetFaceVertexCountsAttr().Get(UsdTimeCode.EarliestTime());
            faceCount = (int)faceCounts.size();
            return MeshConversionUtils.ReadGeomSubsets(geomMesh, UsdTimeCode.EarliestTime(), faceCount);
        }

        private bool TryExtractDisplayColors(UsdGeomMesh geomMesh, out Color[] color, out PrimVarInterpolation interpolation)
        {
            color = null;
            var displayColorAttr = geomMesh.GetDisplayColorAttr();
            interpolation = PrimVarInterpolation.Constant;

            if (displayColorAttr.IsValid() && displayColorAttr.HasValue())
            {
                interpolation = MeshConversionUtils.TfTokenToPrimvarInterpolation(Vt.VtValueToTfToken(displayColorAttr.GetMetadata(interpolationToken)));
                var usdColor = Vt.VtValueToVtVec3fArray(displayColorAttr.Get(UsdTimeCode.EarliestTime()));

                if (usdColor.size() == 0)
                    return false;

                color = new Color[usdColor.size()];
                for (var i = 0; i < usdColor.size(); i++)
                {
                    color[i] = new Color(usdColor[i][0], usdColor[i][1], usdColor[i][2]);
                }
                return true;
            }
            return false;
        }

        private void ExtractDisplayOpacities(UsdGeomMesh geomMesh, out float[] opacities, out PrimVarInterpolation interpolation)
        {
            var displayOpacityAttr = geomMesh.GetDisplayOpacityAttr();
            if (displayOpacityAttr.IsValid() && displayOpacityAttr.HasValue())
            {
                interpolation = MeshConversionUtils.TfTokenToPrimvarInterpolation(Vt.VtValueToTfToken(displayOpacityAttr.GetMetadata(interpolationToken)));
                var opacityArray = Vt.VtValueToVtFloatArray(displayOpacityAttr.Get());

                if (opacityArray.size() > 0)
                {
                    opacities = new float[opacityArray.size()];
                    opacityArray.CopyToArray(opacities);
                    return;
                }
            }

            //If display opacities were not provided, just default to a constant opacity of 1 for all faces
            opacities = new[] {1f};
            interpolation = PrimVarInterpolation.Constant;
        }

        private bool TryFlattenPrimVarIndices(UsdPrim prim, PrimVarInterpolation interpolation, string primVarPath, int faceCount, int indexedValueCount, out int[] indices)
        {
            switch (interpolation)
            {
                case PrimVarInterpolation.Uniform:
                    return TryExtractIndices(prim, primVarPath, faceCount, indexedValueCount, out indices);

                default: //Assume constant interpolation by default
                    indices = null;
                    return true;
            }
        }

        private bool TryExtractIndices(UsdPrim prim, string primVarPath, int faceCount, int indexedValueCount, out int[] indices)
        {
            //Try to read the indices array
            var indicesAttribute = prim.GetAttribute(new TfToken(primVarPath));
            if (indicesAttribute.IsValid() && indicesAttribute.HasValue())
            {
                var indicesArray = Vt.VtValueToVtIntArray(indicesAttribute.Get());

                //Check if index array is valid for uniform interpolation (one value per face)
                if (indicesArray.size() == faceCount)
                {
                    indices = new int[indicesArray.size()];
                    indicesArray.CopyToArray(indices);
                    return true;
                }

                //Check if the array was empty. If not, it means that the size is invalid for a uniform interpolation, abort!
                if (indicesArray.size() > 0)
                {
                    indices = null;
                    return false;
                }
            }

            //If we could not find an index array or if it was empty, check if we have one value per face.
            //If so, assume usd means to match each indexed value to the faces, in the same order.
            if (indexedValueCount == faceCount)
            {
                indices = new int[faceCount];
                for (var i = 0; i < indices.Length; i++)
                {
                    indices[i] = i;
                }
                return true;
            }

            //We didn't manage to produce any valid indices array, abort!
            indices = null;
            return false;
        }

        private void DisposeSubSets(NativeArray<Subset> subSets)
        {
            if (subSets.IsCreated)
            {
                for (var i = 0; i < subSets.Length; i++)
                {
                    subSets[i].Dispose();
                }
                subSets.Dispose();
            }
        }

        private void AssignMaterials(UsdPrim prim, NativeArray<Subset> subSets, Color[] colors, int[] colorIndices, float[] opacities, int[] opacityIndices)
        {
            if (subSets.Length == 0)
            {
                AssignMaterialToMesh(prim, colors, colorIndices, opacities, opacityIndices);
            }
            else
            {
                AssignMaterialToSubMesh(prim, subSets, colors, colorIndices, opacities, opacityIndices);
            }
        }

        private void AssignMaterialToMesh(UsdPrim prim, Color[] colors, int[] colorIndices, float[] opacities, int[] opacityIndices)
        {
            var masterPrimPath = prim.IsInstanceProxy() ? prim.GetPrimInPrototype().GetPath() : prim.GetPath();

            Output.meshMaterialsPaths.Add(prim.GetPath(), new List<string> {masterPrimPath});

            //Create one material for this color, using the color and transparency of the first face
            var materialDescription = new DisplayColorMaterialDescription
            {
                name = masterPrimPath.GetName(),
                color = colors[colorIndices != null ? colorIndices[0] : 0],
                opacity = opacities[opacityIndices != null ? opacityIndices[0] : 0]
            };
            Output.displayColorMaterialDescriptions.Add(masterPrimPath, materialDescription);
        }

        private void AssignMaterialToSubMesh(UsdPrim prim, NativeArray<Subset> subSets, Color[] colors, int[] colorIndices, float[] opacities, int[] opacityIndices)
        {
            var masterPrimPath = prim.IsInstanceProxy() ? prim.GetPrimInPrototype().GetPath() : prim.GetPath();

            //For each subset, check if we already pointed to the same combination of color and transparency indices.
            //If so, we consider the subset is using the same material as a previous one and reuse it.
            var subMeshMaterials = new List<string>(subSets.Length);
            var materialCache = new HashSet<MaterialCacheEntry>();
            var materialIndex = 0;
            for (var i = 0; i < subSets.Length; i++)
            {
                //We are only using the first face of each subset. We do not support different colors per subset!
                var firstFace = subSets[i].FaceIndices[0];
                var colorIndex = colorIndices != null ? colorIndices[firstFace] : 0;
                var opacityIndex = opacityIndices != null ? opacityIndices[firstFace] : 0;

                //Check if we can reuse a previous material
                var entry = new MaterialCacheEntry(colorIndex, opacityIndex, $"{masterPrimPath}_{materialIndex}");
                if (materialCache.TryGetValue(entry, out var cacheEntry))
                {
                    subMeshMaterials.Add(cacheEntry.path);
                    continue;
                }
                materialCache.Add(entry);

                var materialDescription = new DisplayColorMaterialDescription
                {
                    name = $"{masterPrimPath.GetName()}_{materialIndex}",
                    color = colors[colorIndex],
                    opacity = opacities[opacityIndex]
                };
                Output.displayColorMaterialDescriptions.Add($"{masterPrimPath}_{materialIndex}", materialDescription);
                subMeshMaterials.Add($"{masterPrimPath}_{materialIndex}");
                materialIndex++;
            }

            Output.meshMaterialsPaths.Add(prim.GetPath(), subMeshMaterials);
        }

        private struct MaterialCacheEntry : IEquatable<MaterialCacheEntry>
        {
            public string path;
            private int opacityIndex;
            private int colorIndex;

            public MaterialCacheEntry(int opacityIndex, int colorIndex, string path)
            {
                this.opacityIndex = opacityIndex;
                this.colorIndex = colorIndex;
                this.path = path;
            }

            public bool Equals(MaterialCacheEntry other)
            {
                return opacityIndex == other.opacityIndex && colorIndex == other.colorIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is MaterialCacheEntry other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(opacityIndex, colorIndex);
            }
        }
    }
}
