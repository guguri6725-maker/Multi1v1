using System.Collections.Generic;
using pxr;
using Unity.Importer.USD;
using Unity.Mathematics;
using UnityEngine.Importer;
using UnityEngine;
using USD.NET;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// This node will read skeletal joint xform animation from given bindings of UsdSkelAnimation to UsdSkelSkeletons
    /// The node has a single output: all the found animation for animated joints
    /// </summary>
    [NodeMetadata("ReadJointXformAnimationNode", 1)]
    public class ReadJointXformAnimationNode : Node<ReadJointXformAnimationNode.InputPort, ReadJointXformAnimationNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadJointXformAnimationNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// The animated bindings (UsdSkelAnimation prim -> UsdSkelSkeleton prims)
            /// </summary>
            public Dictionary<UsdSkelAnimation, List<UsdSkelSkeleton>> skelAnimToSkelSkeletons;

            /// <summary>
            /// USD stage metadata.
            /// </summary>
            public UsdStageMetadata usdMetadata;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadJointXformAnimationNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// All the properties that are animated on the skeleton joint transforms
            /// </summary>
            public Dictionary<EditorCurveBinding, Keyframe[]> animatedProperties = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            var skelAnimToSkelSkeletons = Input.skelAnimToSkelSkeletons;
            if (skelAnimToSkelSkeletons == null || skelAnimToSkelSkeletons.Count == 0)
                return;

            var bindingToKeyframes = CreateKeyframesFromJointXformAnim(skelAnimToSkelSkeletons);
            if (bindingToKeyframes.Count > 0)
                Output.animatedProperties = bindingToKeyframes;
        }

        private Dictionary<EditorCurveBinding, Keyframe[]> CreateKeyframesFromJointXformAnim(Dictionary<UsdSkelAnimation, List<UsdSkelSkeleton>> skelAnimToSkelSkeletons)
        {
            var timeCodesPerSecond = (float)Input.usdMetadata.timeCodesPerSecond.GetValue();
            var isStageZup = Input.usdMetadata.isStageZup;
            var scaleFactor = Input.usdMetadata.metersPerUnit;
            var typeofTransform = typeof(Transform);
            var retVal = new Dictionary<EditorCurveBinding, Keyframe[]>();
            foreach (var kvp in skelAnimToSkelSkeletons)
            {
                var skelAnim = kvp.Key;

                var jointPaths = IntrinsicTypeConverter.FromVtArray((VtTokenArray)skelAnim.GetJointsAttr().Get());
                var nbJoints = jointPaths.Length;

                if (nbJoints == 0)
                    continue;

                var tAttribute = skelAnim.GetTranslationsAttr();
                var tTimeSamples = tAttribute.GetTimeSamples();
                var tNbKeyframes = tTimeSamples.Count > 0 ? tTimeSamples.Count : 1;

                var rAttribute = skelAnim.GetRotationsAttr();
                var rTimeSamples = rAttribute.GetTimeSamples();
                var rNbKeyframes = rTimeSamples.Count > 0 ? rTimeSamples.Count : 1;

                var sAttribute = skelAnim.GetScalesAttr();
                var sTimeSamples = sAttribute.GetTimeSamples();
                var sNbKeyframes = sTimeSamples.Count > 0 ? sTimeSamples.Count : 1;

                var skelAnimData = new Keyframe[nbJoints][][];
                for (int j = 0; j < nbJoints; ++j)
                {
                    skelAnimData[j] = new Keyframe[10][];
                    skelAnimData[j][0] = new Keyframe[tNbKeyframes];
                    skelAnimData[j][1] = new Keyframe[tNbKeyframes];
                    skelAnimData[j][2] = new Keyframe[tNbKeyframes];

                    skelAnimData[j][3] = new Keyframe[rNbKeyframes];
                    skelAnimData[j][4] = new Keyframe[rNbKeyframes];
                    skelAnimData[j][5] = new Keyframe[rNbKeyframes];
                    skelAnimData[j][6] = new Keyframe[rNbKeyframes];

                    skelAnimData[j][7] = new Keyframe[sNbKeyframes];
                    skelAnimData[j][8] = new Keyframe[sNbKeyframes];
                    skelAnimData[j][9] = new Keyframe[sNbKeyframes];
                }

                if (tTimeSamples.Count > 0)
                {
                    for (var k = 0; k < tTimeSamples.Count; ++k)
                    {
                        var timeCode = tTimeSamples[k];
                        var translationValues = (VtVec3fArray)tAttribute.Get(timeCode);
                        var unityTime = (float)timeCode / timeCodesPerSecond;
                        for (int j = 0; j < nbJoints; ++j)
                        {
                            var translation =  UsdXformConversionUtils.CalculateTranslationAdjustedToStage(
                                new float3(translationValues[j][0], translationValues[j][1], translationValues[j][2]),
                                isStageZup);
                            skelAnimData[j][0][k] = new Keyframe { time = unityTime, value = translation.x * scaleFactor };
                            skelAnimData[j][1][k] = new Keyframe { time = unityTime, value = translation.y * scaleFactor };
                            skelAnimData[j][2][k] = new Keyframe { time = unityTime, value = translation.z * scaleFactor };
                        }
                    }
                }
                else
                {
                    var translationValues = (VtVec3fArray)tAttribute.Get();
                    for (int j = 0; j < nbJoints; ++j)
                    {
                        var translation =  UsdXformConversionUtils.CalculateTranslationAdjustedToStage(
                            new float3(translationValues[j][0], translationValues[j][1], translationValues[j][2]),
                            isStageZup);
                        skelAnimData[j][0][0] = new Keyframe { time = 0, value = translation.x * scaleFactor };
                        skelAnimData[j][1][0] = new Keyframe { time = 0, value = translation.y * scaleFactor };
                        skelAnimData[j][2][0] = new Keyframe { time = 0, value = translation.z * scaleFactor };
                    }
                }

                if (rTimeSamples.Count > 0)
                {
                    for (var k = 0; k < rNbKeyframes; ++k)
                    {
                        var timeCode = rTimeSamples[k];
                        var rotationValue = (VtQuatfArray)rAttribute.Get(timeCode);
                        var unityTime = (float)timeCode / timeCodesPerSecond;
                        for (int j = 0; j < nbJoints; ++j)
                        {
                            var quaternion = UsdXformConversionUtils.GfQuatfToQuaternion(rotationValue[j]);
                            quaternion = UsdXformConversionUtils.CalculateQuaternionAdjustedToStage(quaternion, isStageZup);

                            skelAnimData[j][3][k] = new Keyframe { time = unityTime, value = quaternion.value.x };
                            skelAnimData[j][4][k] = new Keyframe { time = unityTime, value = quaternion.value.y };
                            skelAnimData[j][5][k] = new Keyframe { time = unityTime, value = quaternion.value.z };
                            skelAnimData[j][6][k] = new Keyframe { time = unityTime, value = quaternion.value.w };
                        }
                    }
                }
                else
                {
                    var rotationValue = (VtQuatfArray)rAttribute.Get();
                    for (int j = 0; j < nbJoints; ++j)
                    {
                        var quaternion = UsdXformConversionUtils.GfQuatfToQuaternion(rotationValue[j]);
                        quaternion = UsdXformConversionUtils.CalculateQuaternionAdjustedToStage(quaternion, isStageZup);

                        skelAnimData[j][3][0] = new Keyframe { time = 0, value = quaternion.value.x };
                        skelAnimData[j][4][0] = new Keyframe { time = 0, value = quaternion.value.y };
                        skelAnimData[j][5][0] = new Keyframe { time = 0, value = quaternion.value.z };
                        skelAnimData[j][6][0] = new Keyframe { time = 0, value = quaternion.value.w };
                    }
                }


                if (sTimeSamples.Count > 0)
                {
                    for (var k = 0; k < sNbKeyframes; ++k)
                    {
                        var timeCode = sTimeSamples[k];
                        var scaleValue = (VtVec3hArray)sAttribute.Get(timeCode);
                        var unityTime = (float)timeCode / timeCodesPerSecond;
                        for (int j = 0; j < nbJoints; ++j)
                        {
                            var scale = UsdXformConversionUtils.CalculateScaleAdjustedToStage(
                                new float3(scaleValue[j][0], scaleValue[j][1], scaleValue[j][2]),
                                isStageZup);
                            skelAnimData[j][7][k] = new Keyframe { time = unityTime, value = scale.x };
                            skelAnimData[j][8][k] = new Keyframe { time = unityTime, value = scale.y };
                            skelAnimData[j][9][k] = new Keyframe { time = unityTime, value = scale.z };
                        }
                    }
                }
                else
                {
                    var scaleValue = (VtVec3hArray)sAttribute.Get();
                    for (int j = 0; j < nbJoints; ++j)
                    {
                        var scale = UsdXformConversionUtils.CalculateScaleAdjustedToStage(
                            new float3(scaleValue[j][0], scaleValue[j][1], scaleValue[j][2]),
                            isStageZup);
                        skelAnimData[j][7][0] = new Keyframe { time = 0, value = scale.x };
                        skelAnimData[j][8][0] = new Keyframe { time = 0, value = scale.y };
                        skelAnimData[j][9][0] = new Keyframe { time = 0, value = scale.z };
                    }
                }

                Output.animatedProperties = new Dictionary<EditorCurveBinding, Keyframe[]>();
                foreach (var skel in kvp.Value)
                {
                    var skelPath = skel.GetPath().ToString();
                    for (int j = 0; j < nbJoints; ++j)
                    {
                        var assetPath = $"{skelPath.TrimStart('/')}/{jointPaths[j]}";
                        retVal[new EditorCurveBinding {path = assetPath, propertyName = "m_LocalPosition.x", type = typeofTransform}] = skelAnimData[j][0];
                        retVal[new EditorCurveBinding {path = assetPath, propertyName = "m_LocalPosition.y", type = typeofTransform}] = skelAnimData[j][1];
                        retVal[new EditorCurveBinding {path = assetPath, propertyName = "m_LocalPosition.z", type = typeofTransform}] = skelAnimData[j][2];

                        retVal[new EditorCurveBinding {path = assetPath, propertyName = "m_LocalRotation.x", type = typeofTransform}] = skelAnimData[j][3];
                        retVal[new EditorCurveBinding {path = assetPath, propertyName = "m_LocalRotation.y", type = typeofTransform}] = skelAnimData[j][4];
                        retVal[new EditorCurveBinding {path = assetPath, propertyName = "m_LocalRotation.z", type = typeofTransform}] = skelAnimData[j][5];
                        retVal[new EditorCurveBinding {path = assetPath, propertyName = "m_LocalRotation.w", type = typeofTransform}] = skelAnimData[j][6];

                        retVal[new EditorCurveBinding {path = assetPath, propertyName = "m_LocalScale.x", type = typeofTransform}] = skelAnimData[j][7];
                        retVal[new EditorCurveBinding {path = assetPath, propertyName = "m_LocalScale.y", type = typeofTransform}] = skelAnimData[j][8];
                        retVal[new EditorCurveBinding {path = assetPath, propertyName = "m_LocalScale.z", type = typeofTransform}] = skelAnimData[j][9];
                    }
                }
            }
            return retVal;
        }
    }
}
