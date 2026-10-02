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
    /// This node will read in generalized data from a UsdGeomNurbsCurves prim and sample it.
    /// For more info on UsdGeomNurbsCurves, look at https://graphics.pixar.com/usd/dev/api/class_usd_geom_nurbs_curves.html
    /// </summary>
    [NodeMetadata("SampleNurbsCurvesNode", 4, description = "", displayName = "SampleNurbsCurvesNode")]
    public class SampleNurbsCurvesNode : Node<SampleNurbsCurvesNode.InputPort, SampleNurbsCurvesNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="SampleNurbsCurvesNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// List of UsdGeomCurves to convert. This can include UsdGeomCurves prims that are USD references.
            /// </summary>
            public List<UsdPrim> prims;

            /// <summary>
            /// The density to use for sampling curves (the targeted count of point per curve)
            /// </summary>
            public SamplesPerStrandSetting samplesPerStrand;

            /// <summary>
            /// USD stage metadata.
            /// </summary>
            public UsdStageMetadata usdMetadata;
        }

        /// <summary>
        /// Output ports of the <see cref="SampleNurbsCurvesNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-CurvesDescription mapping resulting from the UsdGeomCurves conversion.
            /// </summary>
            public Dictionary<string, CurvesDescription> curveData;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.curveData = new Dictionary<string, CurvesDescription>(Input.prims.Count);
            for (var i = 0; i < Input.prims.Count; i++)
            {
                var primPath = Input.prims[i].GetPath();
                var description = new CurvesDescription
                {
                    basis = CurveBasis.Undefined,
                    type = CurveType.Nurbs,
                    wrap = CurveWrap.NonPeriodic
                };

                var nurbsCurves = new UsdGeomNurbsCurves(Input.prims[i]);

                using var orderBuffer = GetNurbsCurvesOrders(nurbsCurves);
                var ranges = GetNurbsCurvesRanges(nurbsCurves);

                if (!CurvesImportUtils.TryGetCurveBuffer(nurbsCurves,
                    vertexCount => vertexCount > 0,
                    out NativeArray<int> curveBufferHead,
                    out NativeArray<float3> curveBuffer,
                    Input.usdMetadata,
                    Input.GraphLogger))
                    continue;

                using (curveBufferHead)
                using (curveBuffer)
                {
                    if (!IsNurbsCurvesDataValid(curveBufferHead, orderBuffer, ranges, primPath, Input.GraphLogger))
                        continue;

                    GetCurveKnotBuffer(nurbsCurves, orderBuffer, out NativeArray<int> curveKnotBufferHead, out NativeArray<double> curveKnotBuffer);

                    using (curveKnotBufferHead)
                    using (curveKnotBuffer)
                    {
                        if (!IsNurbsCurvesKnotDataValid(curveBuffer, curveBufferHead, curveKnotBufferHead, curveKnotBuffer, orderBuffer, ranges, primPath, Input.GraphLogger))
                            continue;

                        GetEmptySampledCurveBuffer(curveBufferHead, Input.samplesPerStrand.value, out NativeArray<int> sampledCurveBufferHead,
                            out NativeArray<float3> sampledCurveBuffer);

                        new PopulateSampledNurbsCurveBuffer(curveBufferHead, curveBuffer, orderBuffer,
                            curveKnotBufferHead, curveKnotBuffer, Input.samplesPerStrand.value, sampledCurveBuffer, sampledCurveBufferHead)
                            .Schedule(curveBufferHead.Length, 1).Complete();

                        description.curveData = sampledCurveBuffer;
                        description.curveDataHeads = sampledCurveBufferHead;
                        Output.curveData.Add(primPath, description);
                    }
                }
            }
        }

        private static unsafe void GetCurveKnotBuffer(UsdGeomNurbsCurves curves, NativeArray<int> orderBuffer, out NativeArray<int> curveKnotBufferHead, out NativeArray<double> curveKnotBuffer)
        {
            VtIntArray curveLengthWithinBufferMarshalled = curves.GetCurveVertexCountsAttr().Get();
            var knotsAttr = curves.GetKnotsAttr();
            VtDoubleArray knotsVtArr = knotsAttr.Get();

            curveKnotBuffer = new NativeArray<double>((int)knotsVtArr.size(),  Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            knotsVtArr.CopyToArray((IntPtr)curveKnotBuffer.GetUnsafePtr());

            curveKnotBufferHead = new NativeArray<int>((int)curveLengthWithinBufferMarshalled.size(),  Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var head = 0;
            for (var i = 0; i < curveLengthWithinBufferMarshalled.size(); i++)
            {
                curveKnotBufferHead[i] = head;
                head += curveLengthWithinBufferMarshalled[i] + orderBuffer[i];
            }
        }

        //Follows the same end-to-end buffer and head strategy as GetCurveBuffer
        private static void GetEmptySampledCurveBuffer(NativeArray<int> curveBufferHead, int strandLength, out NativeArray<int> sampledCurveBufferHead, out NativeArray<float3> sampledCurveBuffer)
        {
            sampledCurveBufferHead = new NativeArray<int>(curveBufferHead.Length,  Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var head = 0;
            for (var i = 0; i < curveBufferHead.Length; i++)
            {
                sampledCurveBufferHead[i] = head;
                head += strandLength + 1;
            }

            sampledCurveBuffer = new NativeArray<float3>(head,  Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }

        private static bool IsNurbsCurvesDataValid(NativeArray<int> curveBufferHead, NativeArray<int> orders, (double, double)[] ranges,
            string primPath, GraphLogger GraphLogger)
        {
            if (orders.Length == 0)
            {
                GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomNurbsCurves. UsdGeomNurbsCurves has an empty or non-existent Order array.", null, NodeWarnings.EmptyOrderForNurbsCurve);
                return false;
            }

            if (ranges.Length == 0)
            {
                GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomNurbsCurves. UsdGeomNurbsCurves has an empty or non-existent Ranges array.", null, NodeWarnings.EmptyRangeForNurbsCurve);
                return false;
            }

            if (orders.Length != curveBufferHead.Length)
            {
                GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomNurbsCurves. UsdGeomNurbsCurves order data is not the same length as the number of curves.", null, NodeWarnings.OrderNotMatchingNurbsCurveCount);
                return false;
            }

            // next, verify ranges data.
            if (ranges.Length != curveBufferHead.Length)
            {
                GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomNurbsCurves. UsdGeomNurbsCurves ranges data is not the same length as the number of curves. Expected {curveBufferHead.Length} Actual {ranges.Length}.", null, NodeWarnings.RangeNotMatchingNurbsCurveCount);
                return false;
            }

            return true;
        }

        private static bool IsNurbsCurvesKnotDataValid(NativeArray<float3> curveBuffer, NativeArray<int> curveBufferHead, NativeArray<int> curveKnotBufferHead, NativeArray<double> curveKnotBuffer, NativeArray<int> orders, (double, double)[] ranges, string primPath, GraphLogger GraphLogger)
        {
            if (curveKnotBuffer.Length == 0)
            {
                GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomNurbsCurves. UsdGeomNurbsCurves has an empty or non-existent Knots array.", null, NodeWarnings.EmptyKnotForNurbsCurve);
                return false;
            }

            var numKnotsNeeded = 0;
            for (var i = 0; i < curveBufferHead.Length; i++)
            {
                numKnotsNeeded += orders[i];
            }
            numKnotsNeeded += curveBuffer.Length;

            // verify that the number of knots is sufficient
            if (curveKnotBuffer.Length < numKnotsNeeded)
            {
                GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomNurbsCurves. UsdGeomNurbsCurves has too few knots. Expected {numKnotsNeeded} Actual {curveKnotBuffer.Length}.", null, NodeWarnings.TooFewKnotForNurbsCurve);
                return false;
            }

            for (var curveNum = 0; curveNum < curveBufferHead.Length; curveNum++)
            {
                //next, verify that knot values don't decrease from left to right.
                var endKnotIndex = CurvesImportUtils.GetLastIndexOfSlice(curveNum, curveKnotBufferHead, curveKnotBuffer);
                for (var knotNum = curveKnotBufferHead[curveNum] + 1; knotNum <= endKnotIndex; knotNum++)
                {
                    if (curveKnotBuffer[knotNum - 1] > curveKnotBuffer[knotNum])
                    {
                        GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomNurbsCurves. UsdGeomNurbsCurves knots values must not decrease.", null, NodeWarnings.DecreasingKnotForNurbsCurve);
                        return false;
                    }
                }

                // verify order is valid (positive and <= to vertex count for each curve)
                if (orders[curveNum] <= 0 || orders[curveNum] > CurvesImportUtils.GetLengthOfSlice(curveNum, curveBufferHead, curveBuffer))
                {
                    GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomNurbsCurves. UsdGeomNurbsCurves has an invalid order value (negative or greater than vertex count).", null, NodeWarnings.InvalidOrderValueForNurbsCurve);
                    return false;
                }

                if (ranges[curveNum].Item1 >= ranges[curveNum].Item2)
                {
                    GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomNurbsCurves. UsdGeomNurbsCurves ranges data has an invalid interpolation at index {curveNum}.", null, NodeWarnings.InvalidRangeInterpolationForNurbsCurve);
                    return false;
                }

                //This part is a bit confusing so see USD docs: https://graphics.pixar.com/usd/release/api/class_usd_geom_nurbs_curves.html#a4ec889e254cd1aef43049fbd54b44243
                if (ranges[curveNum].Item1 < curveKnotBuffer[curveKnotBufferHead[curveNum]])
                {
                    GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomNurbsCurves. UsdGeomNurbsCurves ranges data has a minimum that is too low at index {curveNum}.", null, NodeWarnings.TooLowRangeForNurbsCurve);
                    return false;
                }

                if (ranges[curveNum].Item2 > curveKnotBuffer[endKnotIndex])
                {
                    GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomNurbsCurves. UsdGeomNurbsCurves ranges data has a maximum that is too large at index {curveNum}.", null, NodeWarnings.TooHighRangeForNurbsCurve);
                    return false;
                }
            }

            return true;
        }

        private static unsafe NativeArray<int> GetNurbsCurvesOrders(UsdGeomNurbsCurves curves)
        {
            var ordersAttr = curves.GetOrderAttr();
            VtIntArray ordersVtArr = ordersAttr.Get();

            var orderBuffer = new NativeArray<int>((int)ordersVtArr.size(),  Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            ordersVtArr.CopyToArray((IntPtr)orderBuffer.GetUnsafePtr());
            return orderBuffer;
        }

        private static (double, double)[] GetNurbsCurvesRanges(UsdGeomNurbsCurves curves) {
            var rangesAttr = curves.GetRangesAttr();
            VtVec2dArray rangesVtAttr = rangesAttr.Get();
            var ranges = new(double, double)[rangesVtAttr.size()];
            for (var i = 0; i < rangesVtAttr.size(); i++)
            {
                ranges[i] = (rangesVtAttr[i][0], rangesVtAttr[i][1]);
            }
            return ranges;
        }

        [BurstCompile]
        private struct PopulateSampledNurbsCurveBuffer : IJobParallelFor
        {
            [ReadOnly] private NativeArray<int> m_CurveBufferHead;
            [ReadOnly] private NativeArray<float3> m_CurveBuffer;
            [ReadOnly] private NativeArray<int> m_CurveOrders;

            [ReadOnly] private NativeArray<int> m_CurveKnotsBufferHead;
            [ReadOnly] private NativeArray<double> m_CurveKnotsBuffer;
            [ReadOnly] private NativeArray<int> m_SampledCurveBufferHead;
            private readonly int m_StrandLength;

            [NativeDisableContainerSafetyRestriction]
            private NativeArray<float3> m_SampledCurveBuffer;

            public PopulateSampledNurbsCurveBuffer(NativeArray<int> curveBufferHead, NativeArray<float3> curveBuffer,
                                                   NativeArray<int> curveOrders, NativeArray<int> curveKnotsBufferHead, NativeArray<double> curveKnotsBuffer,
                                                   int strandLength, NativeArray<float3> sampledCurveBuffer, NativeArray<int> sampledCurveBufferHead)
            {
                m_CurveBufferHead = curveBufferHead;
                m_CurveBuffer = curveBuffer;
                m_CurveOrders = curveOrders;
                m_CurveKnotsBufferHead = curveKnotsBufferHead;
                m_CurveKnotsBuffer = curveKnotsBuffer;
                m_StrandLength = strandLength;
                m_SampledCurveBuffer = sampledCurveBuffer;
                m_SampledCurveBufferHead = sampledCurveBufferHead;
            }

            public void Execute(int curveIndex)
            {
                var startKnotIndex = m_CurveKnotsBufferHead[curveIndex];
                var start = (float)m_CurveKnotsBuffer[startKnotIndex + m_CurveOrders[curveIndex] - 1];
                var endKnotIndex = CurvesImportUtils.GetLastIndexOfSlice(curveIndex, m_CurveKnotsBufferHead, m_CurveKnotsBuffer);
                var end = (float)m_CurveKnotsBuffer[endKnotIndex + 1 - m_CurveOrders[curveIndex]];
                var range = end - start;

                var firstKnot = m_CurveKnotsBuffer[m_CurveKnotsBufferHead[curveIndex]];
                var lastKnot = m_CurveKnotsBuffer[endKnotIndex];
                using var normalizedKnots = GetNormalizedKnots(startKnotIndex, endKnotIndex);

                using var basisFunction = new NativeArray<float>(m_CurveOrders[curveIndex], Allocator.Temp);
                for (var i = 0; i <= m_StrandLength; i++)
                {
                    var samplePointIndex = m_SampledCurveBufferHead[curveIndex] + i;
                    var t = (i * range / m_StrandLength + start - (float)firstKnot) / (float)(lastKnot - firstKnot);
                    EvaluateNurbs(t, curveIndex, samplePointIndex, basisFunction, normalizedKnots);
                }
            }

            private NativeArray<double> GetNormalizedKnots(int startKnotIndex, int endKnotIndex)
            {
                var knotLength = endKnotIndex - startKnotIndex + 1;
                var normalizedKnots = new NativeArray<double>(knotLength, Allocator.Temp);
                if (m_CurveKnotsBuffer[startKnotIndex] != 0 || m_CurveKnotsBuffer[endKnotIndex] > 1)
                {
                    var originalFirstKnot = m_CurveKnotsBuffer[startKnotIndex];
                    var fullKnotSpan = m_CurveKnotsBuffer[endKnotIndex] - originalFirstKnot;
                    for (var i = 0; i < knotLength; ++i)
                    {
                        normalizedKnots[i] = (m_CurveKnotsBuffer[startKnotIndex + i] - originalFirstKnot) / fullKnotSpan;
                    }
                }
                else
                {
                    for (var i = 0; i < knotLength; ++i)
                    {
                        normalizedKnots[i] = m_CurveKnotsBuffer[startKnotIndex + i];
                    }
                }

                return normalizedKnots;
            }

            private void EvaluateNurbs(float t, int curveIndex, int samplePointIndex, NativeArray<float> basisFunction, NativeArray<double> normalizedKnots)
            {
                var position = float3.zero;
                var span = m_CurveOrders[curveIndex];
                var pointLength = CurvesImportUtils.GetLengthOfSlice(curveIndex, m_CurveBufferHead, m_CurveBuffer);

                while (span < pointLength && normalizedKnots[span] <= t)
                {
                    span++;
                }

                span--;

                GetNurbsBasisFunctions(m_CurveOrders[curveIndex], t, normalizedKnots, span, basisFunction);

                var startPointIndex = m_CurveBufferHead[curveIndex];
                for (var i = 0; i < m_CurveOrders[curveIndex]; ++i)
                {
                    position += basisFunction[i] * m_CurveBuffer[startPointIndex + span - m_CurveOrders[curveIndex] + 1 + i];
                }

                m_SampledCurveBuffer[samplePointIndex] = position;
            }

            static void GetNurbsBasisFunctions(int degree, float t, NativeArray<double> knotVector, int span, NativeArray<float> basisFunction)
            {
                //Constructs the Basis functions at t for the nurbs curve.
                //The nurbs basis function form can be found at this link under the section
                //"Construction of the basis functions": https://en.wikipedia.org/wiki/Non-uniform_rational_B-spline
                //This is an iterative way of computing the same thing.
                var left = new NativeArray<float>(degree, Allocator.Temp);
                var right = new NativeArray<float>(degree, Allocator.Temp);

                for (var j = 0; j < degree; ++j)
                {
                    basisFunction[j] = 1f;
                }

                for (var j = 1; j < degree; ++j)
                {
                    left[j] = (float)(t - knotVector[span + 1 - j]);
                    right[j] = (float)(knotVector[span + j] - t);
                    var saved = 0f;
                    for (var k = 0; k < j; k++)
                    {
                        var temp = basisFunction[k] / (right[k + 1] + left[j - k]);
                        basisFunction[k] = saved + right[k + 1] * temp;
                        saved = left[j - k] * temp;
                    }

                    basisFunction[j] = saved;
                }
            }
        }
    }
}
