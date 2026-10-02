using System;
using System.Collections.Generic;
using pxr;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will read XForm data from each prim in the USD file.
    /// It will then convert them into a format closer to Unity transform description.
    /// </summary>
    [NodeMetadata("ReadXFormNode", 1)]
    public class ReadXFormNode : BaseMatrixConversionNode<ReadXFormNode.InputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadXFormNode"/>.
        /// </summary>
        public class InputPort : BaseInputPort
        {
            /// <summary>
            /// A list of XFormable prims, to be read and converted to <see cref="XFormableDescription"/>.
            /// </summary>
            public List<UsdPrim> prims;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            using var TRSs = new NativeArray<TransformData>(Input.prims.Count, Allocator.TempJob);
            ConvertXformsToTransformData(Input.prims, TRSs, UsdTimeCode.EarliestTime(),
                Input.usdMetadata.isStageZup, Input.usdMetadata.metersPerUnit);

            var primPaths = new string[TRSs.Length];
            for (var i = 0; i < Input.prims.Count; i++)
            {
                primPaths[i] = Input.prims[i].GetPath();
            }

            TRSToXFormableDescriptions(primPaths, TRSs);
        }

        private void ConvertXformsToTransformData(List<UsdPrim> prims, NativeArray<TransformData> TRSs, UsdTimeCode timeCode, bool ZupToYup = false, float scaleFactor = 1.0f)
        {
            var batchCount = prims.Count / k_JobBatchCount;
            var matrices = new NativeArray<float4x4>(prims.Count, Allocator.TempJob);
            var primIntPtrs = new NativeArray<IntPtr>(prims.Count, Allocator.TempJob);
            for (int i = 0; i < prims.Count; i++)
            {
                primIntPtrs[i] = UsdPrim.getCPtr(prims[i]).Handle;
            }

            var jobHandle = new ExtractMatrixFromPrimJob
            {
                prims = primIntPtrs,
                matrices = matrices,
                timeCode = timeCode.GetValue()
            }.Schedule(prims.Count, batchCount);
            jobHandle.Complete();

            ConvertMatrixToTRS(matrices, TRSs, ZupToYup, scaleFactor);

            matrices.Dispose();
            primIntPtrs.Dispose();
        }
    }
}
