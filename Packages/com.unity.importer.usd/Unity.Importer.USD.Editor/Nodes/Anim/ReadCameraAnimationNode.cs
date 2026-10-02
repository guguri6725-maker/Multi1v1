using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    /// This node will read transform animation data from UsdGeomCamera prims in the USD stage.
    /// </summary>
    [NodeMetadata("ReadCameraAnimationNode", 0)]
    public class ReadCameraAnimationNode : Node<ReadCameraAnimationNode.InputPort, ReadCameraAnimationNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadCameraAnimationNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// The list of UsdGeomCamera prims that are present in the stage
            /// </summary>
            public List<UsdPrim> usdCameras;

            /// <summary>
            /// USD stage metadata.
            /// </summary>
            public UsdStageMetadata usdMetadata;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadCameraAnimationNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// All the properties that are animated on the camera.
            /// </summary>
            public Dictionary<EditorCurveBinding, Keyframe[]> animatedProperties;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public unsafe override void Run()
        {
            if (Input.usdCameras == null || Input.usdCameras.Count == 0)
                return;

            var propertyPathToKeyframes = new List<PropertyPathToKeyframes>(Input.usdCameras.Count);
            var millimitersPerUnit = Input.usdMetadata.metersPerUnit * 10.0f;
            var timeCodesPerSecond = Input.usdMetadata.timeCodesPerSecond.GetValue();

            JobHandle jobHandle = default;
            var requiredAttributes = new List<UsdAttribute>();
            foreach (var prim in Input.usdCameras)
            {
                var cam = new UsdGeomCamera(prim);
                var aperture = new float2(cam.GetHorizontalApertureAttr().Get(UsdTimeCode.EarliestTime()), cam.GetVerticalApertureAttr().Get(UsdTimeCode.EarliestTime()));
                var projectionAttr = cam.GetProjectionAttr();
                if (Vt.VtValueToTfToken(projectionAttr.Get(UsdTimeCode.EarliestTime())) == UsdGeomTokens.orthographic)
                {
                    jobHandle = ScheduleFloatAttributeKeyframeExtractionJob(jobHandle, propertyPathToKeyframes, cam.GetVerticalApertureAttr(), "orthographic size", timeCodesPerSecond, requiredAttributes, Input.usdMetadata.metersPerUnit * .05f);
                }
                else
                {
                    jobHandle = ScheduleFloatAttributeKeyframeExtractionJob(jobHandle, propertyPathToKeyframes, cam.GetFocalLengthAttr(), "m_FocalLength", timeCodesPerSecond, requiredAttributes, millimitersPerUnit);
                    jobHandle = ScheduleFloatAttributeKeyframeExtractionJob(jobHandle, propertyPathToKeyframes, cam.GetFocusDistanceAttr(), "m_FocusDistance", timeCodesPerSecond, requiredAttributes, millimitersPerUnit);
                    jobHandle = ScheduleFloatAttributeKeyframeExtractionJob(jobHandle, propertyPathToKeyframes, cam.GetHorizontalApertureOffsetAttr(), "m_LensShift.x", timeCodesPerSecond, requiredAttributes, 1.0f / aperture.x);
                    jobHandle = ScheduleFloatAttributeKeyframeExtractionJob(jobHandle, propertyPathToKeyframes, cam.GetVerticalApertureOffsetAttr(), "m_LensShift.y", timeCodesPerSecond, requiredAttributes, 1.0f / aperture.y);
                }
                jobHandle = ScheduleFloat2AttributeKeyframeExtractionJob(jobHandle, propertyPathToKeyframes, cam.GetClippingRangeAttr(), "near clip plane", "far clip plane", Input.usdMetadata.metersPerUnit, Input.usdMetadata.metersPerUnit, timeCodesPerSecond, requiredAttributes);
            }

            using (new ReferenceHolder<List<UsdAttribute>>(requiredAttributes))
                jobHandle.Complete();

            Output.animatedProperties = new Dictionary<EditorCurveBinding, Keyframe[]>(propertyPathToKeyframes.Count);
            foreach (var item in propertyPathToKeyframes)
            {
                Output.animatedProperties[new EditorCurveBinding { path = item.primPath.TrimStart('/') + "/camera", propertyName = item.propertyPath, type = typeof(Camera) }] = item.keyframes.ToArray();
                item.keyframes.Dispose();
            }
        }

        private JobHandle ScheduleFloatAttributeKeyframeExtractionJob(JobHandle job, List<PropertyPathToKeyframes> jobOutput, UsdAttribute attribute, string propertyPath, double timeCodesPerSecond, List<UsdAttribute> requiredAttributes, float valueScale = 1.0f)
        {
            var timeSamples = attribute.GetTimeSamples();
            if (timeSamples.Count > 0)
            {
                var primPath = attribute.GetPrimPath();
                var keyframes = new NativeArray<Keyframe>(timeSamples.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                var innerLoopBatchCount = math.max(timeSamples.Count / 200, 1);
                requiredAttributes.Add(attribute);
                job = JobHandle.CombineDependencies(job, new ExtractKeyframesFromFloatAttributeJob
                {
                    attributeHandle = UsdAttribute.getCPtr(attribute).Handle,
                    keyframes = keyframes,
                    timeCodes = new NativeArray<double>(timeSamples.ToArray(), Allocator.TempJob),
                    timeCodesPerSecond = timeCodesPerSecond,
                    valueScale = valueScale
                }.Schedule(timeSamples.Count, innerLoopBatchCount)
                );

                jobOutput.Add(new PropertyPathToKeyframes { primPath = primPath, propertyPath = propertyPath, keyframes = keyframes });
            }
            return job;
        }

        private JobHandle ScheduleFloat2AttributeKeyframeExtractionJob(JobHandle job, List<PropertyPathToKeyframes> jobOutput, UsdAttribute attribute, string propertyPathX, string propertyPathY, float valueScaleX, float valueScaleY, double timeCodesPerSecond, List<UsdAttribute> requiredAttributes)
        {
            var timeSamples = attribute.GetTimeSamples();
            if (timeSamples.Count > 0)
            {
                var primPath = attribute.GetPrimPath();
                var keyframesX = new NativeArray<Keyframe>(timeSamples.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                var keyframesY = new NativeArray<Keyframe>(timeSamples.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                var innerLoopBatchCount = math.max(timeSamples.Count / 200, 1);
                requiredAttributes.Add(attribute);
                job = JobHandle.CombineDependencies(job, new ExtractKeyframesFromFloat2AttributeJob
                {
                    attributeHandle = UsdAttribute.getCPtr(attribute).Handle,
                    keyframesX = keyframesX,
                    keyframesY = keyframesY,
                    timeCodes = new NativeArray<double>(timeSamples.ToArray(), Allocator.TempJob),
                    timeCodesPerSecond = timeCodesPerSecond,
                    valueScaleX = valueScaleX,
                    valueScaleY = valueScaleY
                }.Schedule(timeSamples.Count, innerLoopBatchCount)
                );

                jobOutput.Add(new PropertyPathToKeyframes { primPath = primPath, propertyPath = propertyPathX, keyframes = keyframesX });
                jobOutput.Add(new PropertyPathToKeyframes { primPath = primPath, propertyPath = propertyPathY, keyframes = keyframesY });
            }
            return job;
        }

        private struct PropertyPathToKeyframes
        {
            public string primPath;
            public string propertyPath;
            public NativeArray<Keyframe> keyframes;
        }

        protected struct ExtractKeyframesFromFloatAttributeJob : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction]
            [ReadOnly]
            public IntPtr attributeHandle;
            [NativeDisableContainerSafetyRestriction]
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<double> timeCodes;
            public float valueScale;
            public double timeCodesPerSecond;
            [WriteOnly]
            public NativeArray<Keyframe> keyframes;

            public void Execute(int i)
            {
                double timeCode = timeCodes[i];
                float time = (float)(timeCode / timeCodesPerSecond);
                var attr = new UsdAttribute(attributeHandle, false);
                float value = attr.Get(timeCode) * valueScale;
                keyframes[i] = new Keyframe { time = time, value = value };
            }
        }

        protected struct ExtractKeyframesFromFloat2AttributeJob : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction]
            [ReadOnly]
            public IntPtr attributeHandle;
            [NativeDisableContainerSafetyRestriction]
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<double> timeCodes;
            public float valueScaleX;
            public float valueScaleY;
            public double timeCodesPerSecond;
            [WriteOnly]
            public NativeArray<Keyframe> keyframesX;
            [WriteOnly]
            public NativeArray<Keyframe> keyframesY;

            public void Execute(int i)
            {
                double timeCode = timeCodes[i];
                float time = (float)(timeCode / timeCodesPerSecond);
                var attr = new UsdAttribute(attributeHandle, false);
                var value = Vt.VtValueToGfVec2f(attr.Get(timeCode));
                keyframesX[i] = new Keyframe { time = time, value = value[0] * valueScaleX };
                keyframesY[i] = new Keyframe { time = time, value = value[1] * valueScaleY };
            }
        }
    }
}
