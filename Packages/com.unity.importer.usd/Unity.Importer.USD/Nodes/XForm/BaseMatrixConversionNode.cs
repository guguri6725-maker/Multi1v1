using System;
using System.Collections.Generic;
using pxr;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Basis for node reading transform matrices from a USD file and converting them to <see cref="XFormableDescription"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseMatrixConversionNode<T> : Node<T, BaseMatrixConversionNode<T>.OutputPort> where T : InputPorts
    {
        /// <summary>
        /// Number of workers to use per job
        /// </summary>
        protected const int k_JobBatchCount = 8;

        /// <summary>
        /// Base Input ports of the <see cref="BaseMatrixConversionNode{T}"/>.
        /// </summary>
        public class BaseInputPort : InputPorts
        {
            /// <summary>
            /// USD stage metadata.
            /// </summary>
            public UsdStageMetadata usdMetadata;
        }

        /// <summary>
        /// Output ports of the <see cref="BaseMatrixConversionNode{T}"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-XFormableDescription mapping resulting from the transformation.
            /// Commonly, this data is used to create Unity Transform from USD XForm Prims.
            /// </summary>
            public Dictionary<string, XFormableDescription> xFormableDescriptions;
        }

        /// <summary>
        /// Helper method that populates the xFormableDescriptions output
        /// </summary>
        protected void TRSToXFormableDescriptions(string[] transformPaths, NativeArray<TransformData> TRSs)
        {
            Output.xFormableDescriptions = new Dictionary<string, XFormableDescription>(TRSs.Length);
            for (var i = 0; i < TRSs.Length; i++)
            {
                var xFormableDescription = new XFormableDescription
                {
                    name = GetTransformName(transformPaths[i]),
                    transformData = TRSs[i]
                };
                Output.xFormableDescriptions.Add(transformPaths[i], xFormableDescription);
            }
        }

        static string GetTransformName(string path)
        {
            var lastIndex = path.LastIndexOf("/", StringComparison.InvariantCulture);
            var name = path;
            if (lastIndex != -1)
                name = path.Substring(lastIndex).TrimStart('/');
            return name;
        }

        /// <summary>
        /// Converts 4x4 matrices to TransformData structures while applying required axis conversion and scale factor
        /// </summary>
        protected static void ConvertMatrixToTRS(NativeArray<float4x4> matrices, NativeArray<TransformData> TRSs, bool ZupToYup = false, float scaleFactor = 1.0f)
        {
            var batchCount = matrices.Length / k_JobBatchCount;
            var jobHandle = ZupToYup
                ? new ConvertMatrixToggleYZJob {matrices = matrices}.Schedule(matrices.Length, batchCount)
            : new ConvertMatrixFlipZJob { matrices = matrices }.Schedule(matrices.Length, batchCount);

            jobHandle = new ConvertMatrixToTRSJob {TRSs = TRSs, matrices = matrices}.Schedule(matrices.Length, batchCount, jobHandle);

            if (Math.Abs(scaleFactor - 1.0f) > float.Epsilon)
                jobHandle = new ApplyScaleFactorToTRSJob {TRSs = TRSs, scaleFactor = scaleFactor}.Schedule(matrices.Length, batchCount, jobHandle);
            jobHandle.Complete();
        }

        /// <summary>
        /// Job worker that extracts Usd transforms for prims and converts them to 4x4 matrices
        /// </summary>
        protected struct ExtractMatrixFromPrimJob : IJobParallelFor
        {
            /// <summary>
            /// The prims to extract transforms from
            /// </summary>
            [NativeDisableContainerSafetyRestriction]
            [ReadOnly]
            public NativeArray<IntPtr> prims;

            /// <summary>
            /// The extracted matrices
            /// </summary>
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly]
            public NativeArray<float4x4> matrices;

            /// <summary>
            /// The timecode at which to extract matrices
            /// </summary>
            [ReadOnly]
            public double timeCode;

            /// <summary>
            /// See the documentation for &lt;IJobParallelFor.Execute&gt; in the com.unity.importer package documentation for details
            /// </summary>
            public void Execute(int i)
            {
                var transformAttr = UsdCs.GetFusedTransform(new UsdPrim(prims[i], false), timeCode);
                matrices[i] = UsdXformConversionUtils.FromMatrix(transformAttr);
            }
        }

        [BurstCompile]
        struct ConvertMatrixToggleYZJob : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<float4x4> matrices;

            public void Execute(int i)
            {
                matrices[i] = math.mul(UsdXformConversionUtils.k_Toggle_YZ, math.mul(matrices[i], UsdXformConversionUtils.k_Toggle_YZ));
            }
        }

        [BurstCompile]
        struct ConvertMatrixFlipZJob : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<float4x4> matrices;

            public void Execute(int i)
            {
                matrices[i] = math.mul(UsdXformConversionUtils.k_FlipZ, math.mul(matrices[i], UsdXformConversionUtils.k_FlipZ));
            }
        }

        [BurstCompile]
        struct ConvertMatrixToTRSJob : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [ReadOnly]
            public NativeArray<float4x4> matrices;
            [NativeDisableContainerSafetyRestriction]
            [WriteOnly]
            public NativeArray<TransformData> TRSs;

            public void Execute(int i)
            {
                TRSs[i] = UsdXformConversionUtils.Decompose(matrices[i]);
            }
        }

        [BurstCompile]
        struct ApplyScaleFactorToTRSJob : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<TransformData> TRSs;
            [ReadOnly]
            public float scaleFactor;

            public void Execute(int i)
            {
                var trs = TRSs[i];
                trs.translation *= scaleFactor;
                TRSs[i] = trs;
            }
        }
    }
}
