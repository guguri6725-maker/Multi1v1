using System.Collections.Generic;
using pxr;
using Unity.Mathematics;
using UnityEngine.Importer;
using USD.NET;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will read skeleton roots data from the skeleton root prims and output
    /// their blend shape and rig data.
    /// </summary>
    [NodeMetadata("ReadSkeletonRootNode", 4)]
    public class ReadSkeletonRootNode : Node<ReadSkeletonRootNode.InputPort, ReadSkeletonRootNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadSkeletonRootNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// A list of SkelRoot prims, to be read and converted to &lt;pxr.UsdSkelBlendShape&gt; and <see cref="UsdSkinnedMeshData"/>.
            /// </summary>
            public List<UsdPrim> skeletonPrims;

            /// <summary>
            /// A list of UsdSkelBlendShape prims, to be read, converted to &lt;pxr.UsdSkelBlendShape&gt; and linked to their respective skeleton root path.
            /// </summary>
            public List<UsdPrim> blendShapes;

            /// <summary>
            /// USD stage metadata.
            /// </summary>
            public UsdStageMetadata usdMetadata;

            /// <summary>
            /// UsdSkelSkeleton prims that are not skinned. The UsdSkelCache will not return bindings for these but
            /// we still want to create the joint hierarchy in Unity
            /// </summary>
            public List<UsdSkelSkeleton> skinlessSkeletons;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadSkeletonRootNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A mapping of mesh Prim paths to blend shapes prims.
            /// To be used to correctly set blend shape values to the skinned mesh.
            /// </summary>
            public Dictionary<string, UsdSkelBlendShape[]> usdSkelBlendShapes = new();

            /// <summary>
            /// A mapping of mesh Prim paths to skinned mesh data.
            /// To be used to correctly bind skeleton to the skinned meshes.
            /// </summary>
            public Dictionary<string, UsdSkinnedMeshData> skinnedMeshData = new();

            /// <summary>
            /// A mapping of SkelRoot prim paths to <see cref="RigDescription"/> describing the hierarchy of a skeleton.
            /// To be used to create the skeletons hierarchies.
            /// </summary>
            public Dictionary<string, RigDescription> rigDescriptions = new();

            /// <summary>
            /// A list of the SkelRoot prims and their skeleton root joint.
            /// To be used to create this part of the skeleton hierarchy.
            /// </summary>
            public List<UsdPrim> skeletonRootPrims = new();
        }

        private Dictionary<string, UsdPrim> pathToBlendShapePrim = new();

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            CacheBlendShapePaths();

            // Skinned skeletons
            var skeletonCache = new UsdSkelCache();
            foreach (var skeletonRootPrim in Input.skeletonPrims)
            {
                var bindings = SkelUtils.GetSkelBindingsFromCache(skeletonRootPrim, skeletonCache, Input.GraphLogger);
                if (bindings == null)
                    continue;

                Output.skeletonRootPrims.Add(skeletonRootPrim);

                foreach (var skeletonBinding in bindings)
                {
                    var skeleton = skeletonBinding.GetSkeleton();
                    var bindTransforms = ReadSkeletonData(skeleton, out var joints);
                    var skinningQueries = skeletonBinding.GetSkinningTargetsAsVector();

                    foreach (var skinningQuery in skinningQueries)
                    {
                        var skinnedMeshPath = skinningQuery.GetPrim().GetPath();
                        ReadJointInfluences(skinnedMeshPath, skinningQuery, bindTransforms, skeleton.GetPath(), joints);
                        ReadBlendShapes(skinnedMeshPath, skinningQuery);
                    }
                }
            }
            // Skinless skeletons
            if (Input.skinlessSkeletons != null)
                foreach (var skelSkeleton in Input.skinlessSkeletons)
                    ReadSkeletonData(skelSkeleton, out _);
        }

        private void CacheBlendShapePaths()
        {
            foreach (var blendShape in Input.blendShapes)
            {
                pathToBlendShapePrim.Add(blendShape.GetPath(), blendShape);
            }
        }

        private void ReadJointInfluences(string skinnedMeshPath, UsdSkelSkinningQuery skinningQuery,
            float4x4[] bindTransforms, string skeletonRootPath, string[] joints)
        {
            if (!skinningQuery.HasJointInfluences())
                return;

            var geomXf = UsdXformConversionUtils.FromMatrix(skinningQuery.GetGeomBindTransform(), true, Input.usdMetadata.isStageZup);
            var bindTransformsForSkin = InvertTransforms(bindTransforms, geomXf);

            ConvertInfluences(skinnedMeshPath, skinningQuery, bindTransformsForSkin, skeletonRootPath, joints);
        }

        private void ReadBlendShapes(string skinnedMeshPath, UsdSkelSkinningQuery skinningQuery)
        {
            if (!skinningQuery.HasBlendShapes())
                return;

            VtTokenArray blendShapesOrder = new VtTokenArray();
            skinningQuery.GetBlendShapeOrder(blendShapesOrder);

            var targetPaths = skinningQuery.GetBlendShapeTargetsRel().GetTargets();

            var blendShapes = new UsdSkelBlendShape[blendShapesOrder.size()];
            for (var i = 0; i < blendShapesOrder.size(); i++)
            {
                if (pathToBlendShapePrim.TryGetValue(targetPaths[i], out var prim))
                {
                    blendShapes[i] = new UsdSkelBlendShape(prim);
                }
                else
                {
                    Input.GraphLogger.LogImportWarning($"Could not find blendshape prim at path {targetPaths[i]}",
                        null, NodeWarnings.CouldNotFindBlendShapePrim);
                }
            }
            Output.usdSkelBlendShapes.Add(skinnedMeshPath, blendShapes);
        }

        private void ConvertInfluences(string skinnedMeshPath, UsdSkelSkinningQuery skinningQuery,
            float4x4[] bindTransformsForSkin, string skeletonRootPath, string[] joints)
        {
            UsdGeomPrimvar jointIndices = skinningQuery.GetJointIndicesPrimvar();
            int[] indices = IntrinsicTypeConverter.FromVtArray((VtIntArray)jointIndices.GetAttr().Get());
            int indicesElementSize = jointIndices.GetElementSize();
            TfToken indicesInterpolation = jointIndices.GetInterpolation();

            if (indices.Length == 0
                || indicesElementSize == 0
                || indices.Length % indicesElementSize != 0
                || !UsdGeomPrimvar.IsValidInterpolation(indicesInterpolation))
            {
                Input.GraphLogger.LogImportError(
                    $"Joints indices data are invalid or empty for skinned mesh '{skinnedMeshPath}' and SkelRoot '{skeletonRootPath}'.",
                    null, NodeErrors.InvalidJointIndices);
                return;
            }

            UsdGeomPrimvar jointWeights = skinningQuery.GetJointWeightsPrimvar();
            float[] weights = IntrinsicTypeConverter.FromVtArray((VtFloatArray)jointWeights.GetAttr().Get());
            int weightsElementSize = jointWeights.GetElementSize();
            TfToken weightsInterpolation = jointWeights.GetInterpolation();

            if (weights.Length == 0
                || weightsElementSize == 0
                || weights.Length % weightsElementSize != 0
                || !UsdGeomPrimvar.IsValidInterpolation(weightsInterpolation))
            {
                Input.GraphLogger.LogImportError(
                    $"Joints weights data are invalid or empty for skinned mesh '{skinnedMeshPath}' and SkelRoot '{skeletonRootPath}'.",
                    null, NodeErrors.InvalidJointWeights);
                return;
            }

            var boneWeight = new UsdSkinnedMeshData
            {
                skeletonRootPath = skeletonRootPath,
                isWeightInterpolationConstant = weightsInterpolation.GetString() == UsdGeomTokens.constant,
                jointIndices = indices,
                weights = weights,
                weightsElementSize = weightsElementSize,
                jointsBindingMatrices = bindTransformsForSkin,
                meshToSkeletonJointIndices = GetMeshToSkeletonJointIndices(skinnedMeshPath, skinningQuery, joints)
            };
            Output.skinnedMeshData.Add(skinnedMeshPath, boneWeight);
        }

        private Dictionary<int, int> GetMeshToSkeletonJointIndices(string skinnedMeshPath, UsdSkelSkinningQuery skinningQuery, string[] joints)
        {
            var jointOrderTokens = new VtTokenArray();
            if (!skinningQuery.GetJointOrder(jointOrderTokens))
                return null;

            var jointNameToIndex = new Dictionary<string, int>(joints.Length);
            for (var i = 0; i < joints.Length; i++)
            {
                jointNameToIndex.Add(joints[i], i);
            }

            var jointOrder = IntrinsicTypeConverter.FromVtArray(jointOrderTokens);
            var meshToSkeletonJointIndices = new Dictionary<int, int>(jointOrder.Length);
            for (var i = 0; i < jointOrder.Length; i++)
            {
                if (jointNameToIndex.TryGetValue(jointOrder[i], out var index))
                {
                    meshToSkeletonJointIndices.TryAdd(i, index);
                }
                else
                {
                    Input.GraphLogger.LogImportWarning(
                        $"Could not find joint '{jointOrder[i]}' when remapping mesh '{skinnedMeshPath}' joints.", null,
                        NodeWarnings.CouldNotFindJoint);
                }
            }
            return meshToSkeletonJointIndices;
        }

        private float4x4[] ReadSkeletonData(UsdSkelSkeleton skeleton, out string[] joints)
        {
            Output.skeletonRootPrims.Add(skeleton.GetPrim());

            var restTransforms = ConvertTransforms(skeleton.GetRestTransformsAttr().Get(), false, false);
            var bindTransforms = ConvertTransforms(skeleton.GetBindTransformsAttr().Get(), true, Input.usdMetadata.isStageZup);
            var inputJoints = IntrinsicTypeConverter.FromVtArray(Vt.VtValueToVtTokenArray(skeleton.GetJointsAttr().Get()));
            var jointsHashSet = new HashSet<string>(inputJoints);
            var allRestTransforms = new List<float4x4>(restTransforms);
            var allBindTransforms = new List<float4x4>(bindTransforms);
            var allJoints = new List<string>(inputJoints);

            // fill in missing joints
            foreach (var joint in inputJoints)
            {
                var jointPath = new SdfPath(joint);
                var jointParentPath = jointPath.GetParentPath();

                if (jointsHashSet.Contains(jointParentPath.GetAsString())) continue;
                var ancestors = jointPath.GetPrefixes();

                foreach (var parent in ancestors)
                {
                    if (jointsHashSet.Contains(parent.GetAsString())) continue;
                    jointsHashSet.Add(parent.GetAsString());
                    allJoints.Add(parent.GetAsString());
                    allRestTransforms.Add(UsdXformConversionUtils.FromMatrix(new GfMatrix4d(1)));
                    allBindTransforms.Add(UsdXformConversionUtils.FromMatrix(new GfMatrix4d(1), true, Input.usdMetadata.isStageZup));
                }
            }

            joints = allJoints.ToArray();

            if (allJoints.Count == 0)
                return allBindTransforms.ToArray();

            var rigDescription = new RigDescription
            {
                jointTransformMatrix = new Dictionary<string, float4x4>()
            };

            var jointsRootPrimPath = skeleton.GetPath();
            for (var i = 0; i < joints.Length; i++)
            {
                var jointPath = $"{jointsRootPrimPath}/{joints[i]}";
                rigDescription.jointTransformMatrix.Add(jointPath, allRestTransforms[i]);
            }

            Output.rigDescriptions.Add(skeleton.GetPath(), rigDescription);

            return allBindTransforms.ToArray();
        }

        private static float4x4[] ConvertTransforms(VtMatrix4dArray bindTransforms, bool withBasisChange, bool ZToYUp)
        {
            var mats = new float4x4[bindTransforms.size()];
            for (var i = 0; i < bindTransforms.size(); i++)
            {
                mats[i] = UsdXformConversionUtils.FromMatrix(bindTransforms[i], withBasisChange, ZToYUp);
            }

            return mats;
        }

        private static float4x4[] InvertTransforms(float4x4[] bindTransforms, float4x4 geomXForm)
        {
            var outXForms = new float4x4[bindTransforms.Length];
            for (var i = 0; i < bindTransforms.Length; i++)
            {
                outXForms[i] = math.mul(math.inverse(bindTransforms[i]), geomXForm);
            }

            return outXForms;
        }
    }
}
