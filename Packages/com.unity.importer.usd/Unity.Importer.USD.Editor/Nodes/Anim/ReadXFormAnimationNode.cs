using System;
using System.Collections.Generic;
using pxr;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Importer.USD;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Importer;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// This node will read transform animation data from UsdGeomXformable prims in the USD stage.
    /// It does so by asking for the fused local transform at each of the USD timecodes for which keys have been
    /// set.
    /// It will then convert this animation data into a format closer to what Unity AnimationClips expect.
    /// </summary>
    [NodeMetadata("ReadXFormAnimationNode", 2)]
    public class ReadXFormAnimationNode : Node<ReadXFormAnimationNode.InputPort, ReadXFormAnimationNode.OutputPort>
    {
        private const int k_JobBatchCount = 8;

        /// <summary>
        /// Input ports of the <see cref="ReadXFormAnimationNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// The list of UsdGeomXformable prims that are present in the stage
            /// </summary>
            public List<UsdPrim> geomXformables;

            /// <summary>
            /// USD stage metadata.
            /// </summary>
            public UsdStageMetadata usdMetadata;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadXFormAnimationNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-XformAnimationDescription mapping resulting from the conversion.
            /// This data will be used downstream to create a Unity AnimationClip asset for the whole stage
            /// </summary>

            public Dictionary<EditorCurveBinding, Keyframe[]> animatedProperties;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            if (null == Input.geomXformables || 0 == Input.geomXformables.Count)
                return;

            var primTimeCodes = new List<NativeArray<double>>();
            var filteredPrims = new List<UsdPrim>();
            var timeSamples = new StdDoubleVector();
            foreach (var prim in Input.geomXformables)
            {
                UsdGeomXformable xfo = new UsdGeomXformable(prim);
                xfo.GetTimeSamples(timeSamples);
                int nbKeys = timeSamples.Count;
                if (nbKeys > 0)
                {
                    filteredPrims.Add(xfo.GetPrim());
                    primTimeCodes.Add(new NativeArray<double>(timeSamples.ToArray(), Allocator.TempJob));
                }
            }

            int nbFilteredPrims = filteredPrims.Count;
            var jobHandles = new NativeArray<JobHandle>(nbFilteredPrims, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var primNativeArrays = new NativeArray<Keyframe>[nbFilteredPrims];
            var primNbKeyframes = new int[nbFilteredPrims];
            for (int i = 0; i < nbFilteredPrims; ++i)
            {
                // We pass USD object pointers to the jobs
                var primHandle = UsdPrim.getCPtr(filteredPrims[i]);
                var timeCodes = primTimeCodes[i];
                int nbKeyframes = timeCodes.Length;

                // Since we cannot use nested NativeArrays in jobs, we create a flat array for all the 10 xform
                // properties (3T, 4R, 3S)
                // The keys will be contiguous per curve,
                //  i.e. all the keys for Tx first, then Ty, Tz, Rx, Ry, Rz, Rw, Sx, Sy, Sz

                primNativeArrays[i] = new NativeArray<Keyframe>(10 * nbKeyframes, Allocator.TempJob);
                primNbKeyframes[i] = nbKeyframes;

                jobHandles[i] = new ExtractKeyframesFromPrimJob
                {
                    primHandle = primHandle.Handle,
                    timeCodes = timeCodes,
                    isStageZup = Input.usdMetadata.isStageZup,
                    scaleFactor = Input.usdMetadata.metersPerUnit,
                    timeCodesPerSecond = Input.usdMetadata.timeCodesPerSecond.GetValue(),
                    keyframes = primNativeArrays[i],
                }.Schedule(nbKeyframes, k_JobBatchCount);
            }

            using (new ReferenceHolder<List<UsdPrim>>(filteredPrims))
                JobHandle.CompleteAll(jobHandles);

            Output.animatedProperties = new Dictionary<EditorCurveBinding, Keyframe[]>();
            for (int i = 0; i < nbFilteredPrims; ++i)
            {
                var assetPath = filteredPrims[i].GetPath().ToString().TrimStart('/');
                var typeOfTransform = typeof(Transform);
                var nativeArray = primNativeArrays[i];
                var nbKeyframes = primNbKeyframes[i];

                var binding = new EditorCurveBinding {path = assetPath, type = typeOfTransform, propertyName = "m_LocalPosition.x"};
                Output.animatedProperties[binding] = nativeArray.GetSubArray(0, nbKeyframes).ToArray();
                binding = new EditorCurveBinding {path = assetPath, type = typeOfTransform, propertyName = "m_LocalPosition.y"};
                Output.animatedProperties[binding] = nativeArray.GetSubArray(nbKeyframes, nbKeyframes).ToArray();
                binding = new EditorCurveBinding {path = assetPath, type = typeOfTransform, propertyName = "m_LocalPosition.z"};
                Output.animatedProperties[binding] = nativeArray.GetSubArray(nbKeyframes * 2, nbKeyframes).ToArray();

                binding = new EditorCurveBinding {path = assetPath, type = typeOfTransform, propertyName = "m_LocalRotation.x"};
                Output.animatedProperties[binding] = nativeArray.GetSubArray(nbKeyframes * 3, nbKeyframes).ToArray();
                binding = new EditorCurveBinding {path = assetPath, type = typeOfTransform, propertyName = "m_LocalRotation.y"};
                Output.animatedProperties[binding] = nativeArray.GetSubArray(nbKeyframes * 4, nbKeyframes).ToArray();
                binding = new EditorCurveBinding {path = assetPath, type = typeOfTransform, propertyName = "m_LocalRotation.z"};
                Output.animatedProperties[binding] = nativeArray.GetSubArray(nbKeyframes * 5, nbKeyframes).ToArray();
                binding = new EditorCurveBinding {path = assetPath, type = typeOfTransform, propertyName = "m_LocalRotation.w"};
                Output.animatedProperties[binding] = nativeArray.GetSubArray(nbKeyframes * 6, nbKeyframes).ToArray();

                binding = new EditorCurveBinding {path = assetPath, type = typeOfTransform, propertyName = "m_LocalScale.x"};
                Output.animatedProperties[binding] = nativeArray.GetSubArray(nbKeyframes * 7, nbKeyframes).ToArray();
                binding = new EditorCurveBinding {path = assetPath, type = typeOfTransform, propertyName = "m_LocalScale.y"};
                Output.animatedProperties[binding] = nativeArray.GetSubArray(nbKeyframes * 8, nbKeyframes).ToArray();
                binding = new EditorCurveBinding {path = assetPath, type = typeOfTransform, propertyName = "m_LocalScale.z"};
                Output.animatedProperties[binding] = nativeArray.GetSubArray(nbKeyframes * 9, nbKeyframes).ToArray();
            }

            jobHandles.Dispose();
            for (int i = 0; i < nbFilteredPrims; ++i)
            {
                primTimeCodes[i].Dispose();
                primNativeArrays[i].Dispose();
            }
        }

        protected struct ExtractKeyframesFromPrimJob : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction]
            [ReadOnly]
            public IntPtr primHandle;

            [ReadOnly]
            public NativeArray<double> timeCodes;

            public bool isStageZup;

            public float scaleFactor;

            public double timeCodesPerSecond;

            private static readonly VtValue FloatVtValue = new VtValue(0.0f);
            private static readonly VtValue Float3VtValue = new VtValue(new GfVec3f(0.0f));
            private static readonly VtValue QuatfVtValue = new VtValue(new GfQuatf(0.0f));

            [NativeDisableContainerSafetyRestriction]
            public NativeArray<Keyframe> keyframes;

            private float4x4 CalculateMatrixAdjustedToStage(float4x4 matrix)
            {
                return isStageZup
                    ? math.mul(UsdXformConversionUtils.k_Toggle_YZ,
                    math.mul(matrix, UsdXformConversionUtils.k_Toggle_YZ))
                    : math.mul(UsdXformConversionUtils.k_FlipZ, math.mul(matrix, UsdXformConversionUtils.k_FlipZ));
            }

            public void Execute(int i)
            {
                double timeCode = timeCodes[i];
                float time = (float)(timeCode / timeCodesPerSecond);

                var geomXformable = new UsdGeomXformable(new UsdPrim(primHandle, false));
                var ops = geomXformable.GetOrderedXformOps(out _);

                var scale = new float3(1f);
                var rotation = quaternion.identity;
                var translation = new float4(0, 0, 0, 1f);

                for (var opId = ops.Count - 1; opId >= 0; opId--)
                {
                    var op = ops[opId];
                    var opName = op.GetName().ToString();
                    var opValueAtTime = op.GetAttr().Get(timeCode);
                    var opMat = CalculateMatrixAdjustedToStage(UsdXformConversionUtils.FromMatrix(op.GetOpTransform(timeCode)));
                    translation = math.mul(opMat, translation);

                    if (opName == "xformOp:scale")
                    {
                        GfVec3f opScale = VtValue.CastToTypeOf(opValueAtTime, Float3VtValue);
                        scale *= UsdXformConversionUtils.CalculateScaleAdjustedToStage(new float3(opScale[0], opScale[1], opScale[2]), isStageZup);
                    }
                    else if (opName == "xformOp:rotateXYZ" || opName == "xformOp:rotateXZY" ||
                             opName == "xformOp:rotateYXZ" || opName == "xformOp:rotateYZX" ||
                             opName == "xformOp:rotateZXY" || opName == "xformOp:rotateZYX")
                    {
                        GfVec3f opRotation = VtValue.CastToTypeOf(opValueAtTime, Float3VtValue);
                        var eulerAngles = math.radians(new float3(opRotation[0], opRotation[1], opRotation[2]));

                        var opQuaternion = quaternion.identity;
                        switch (opName)
                        {
                            case "xformOp:rotateXYZ":
                                opQuaternion = quaternion.EulerXYZ(eulerAngles);
                                break;
                            case "xformOp:rotateXZY":
                                opQuaternion = quaternion.EulerXZY(eulerAngles);
                                break;
                            case "xformOp:rotateYXZ":
                                opQuaternion = quaternion.EulerYXZ(eulerAngles);
                                break;
                            case "xformOp:rotateYZX":
                                opQuaternion = quaternion.EulerYZX(eulerAngles);
                                break;
                            case "xformOp:rotateZXY":
                                opQuaternion = quaternion.EulerZXY(eulerAngles);
                                break;
                            case "xformOp:rotateZYX":
                                opQuaternion = quaternion.EulerZYX(eulerAngles);
                                break;
                        }
                        ;
                        opQuaternion = UsdXformConversionUtils.CalculateQuaternionAdjustedToStage(opQuaternion, isStageZup);
                        rotation = math.mul(opQuaternion, rotation);
                    }
                    else if (opName == "xformOp:rotateX" || opName == "xformOp:rotateY" || opName == "xformOp:rotateZ")
                    {
                        var opAtTime = VtValue.CastToTypeOf(opValueAtTime, FloatVtValue);
                        var opQuaternion = quaternion.identity;
                        switch (opName)
                        {
                            case "xformOp:rotateX":
                                opQuaternion = quaternion.RotateX(math.radians(opAtTime));
                                break;
                            case "xformOp:rotateY":
                                opQuaternion = quaternion.RotateY(math.radians(opAtTime));
                                break;
                            case "xformOp:rotateZ":
                                opQuaternion = quaternion.RotateZ(math.radians(opAtTime));
                                break;
                        }
                        ;
                        opQuaternion = UsdXformConversionUtils.CalculateQuaternionAdjustedToStage(opQuaternion, isStageZup);
                        rotation = math.mul(opQuaternion, rotation);
                    }
                    else if (opName == "xformOp:orient")
                    {
                        GfQuatf opAtTime = VtValue.CastToTypeOf(opValueAtTime, QuatfVtValue);
                        var opQuaternion = UsdXformConversionUtils.GfQuatfToQuaternion(opAtTime);
                        opQuaternion = UsdXformConversionUtils.CalculateQuaternionAdjustedToStage(opQuaternion, isStageZup);
                        rotation = math.mul(opQuaternion, rotation);
                    }
                    else if (opName == "xformOp:transform")
                    {
                        var trs = UsdXformConversionUtils.Decompose(opMat);
                        scale *= trs.scale;
                        rotation = math.mul(trs.rotation, rotation);
                    }
                }

                if (Math.Abs(scaleFactor - 1.0f) > float.Epsilon)
                {
                    translation.xyz *= scaleFactor;
                }

                var nbKeyframes = timeCodes.Length;

                keyframes[i] = new Keyframe { time = time, value = translation.x };
                keyframes[nbKeyframes     + i] = new Keyframe { time = time, value = translation.y };
                keyframes[nbKeyframes * 2 + i] = new Keyframe { time = time, value = translation.z };

                keyframes[nbKeyframes * 3 + i] = new Keyframe { time = time, value = rotation.value.x };
                keyframes[nbKeyframes * 4 + i] = new Keyframe { time = time, value = rotation.value.y };
                keyframes[nbKeyframes * 5 + i] = new Keyframe { time = time, value = rotation.value.z };
                keyframes[nbKeyframes * 6 + i] = new Keyframe { time = time, value = rotation.value.w };

                keyframes[nbKeyframes * 7 + i] = new Keyframe { time = time, value = scale.x };
                keyframes[nbKeyframes * 8 + i] = new Keyframe { time = time, value = scale.y };
                keyframes[nbKeyframes * 9 + i] = new Keyframe { time = time, value = scale.z };
            }
        }
    }
}
