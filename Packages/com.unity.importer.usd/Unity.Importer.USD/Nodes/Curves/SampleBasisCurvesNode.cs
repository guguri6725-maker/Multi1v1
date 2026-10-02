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
    /// This node will read in generalized data from a UsdGeomBasisCurves prim and sample it when needed.
    /// For more info on UsdGeomBasisCurves, look at https://graphics.pixar.com/usd/dev/api/class_usd_geom_basis_curves.html
    /// </summary>
    [NodeMetadata("SampleBasisCurvesNode", 4, description = "Read in generalized data from a UsdGeomBasisCurves prim and sample it.", displayName = "SampleBasisCurvesNode")]
    public class SampleBasisCurvesNode : Node<SampleBasisCurvesNode.InputPort, SampleBasisCurvesNode.OutputPort>
    {
        private const int MIN_SAMPLING_PER_SEGMENT = 2;

        /// <summary>
        /// Input ports of the <see cref="SampleBasisCurvesNode"/>.
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
        /// Output ports of the <see cref="SampleBasisCurvesNode"/>.
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

            for (int i = 0; i < Input.prims.Count; i++)
            {
                UsdGeomBasisCurves basisCurves = new UsdGeomBasisCurves(Input.prims[i]);

                string primPath = Input.prims[i].GetPath();
                if (!ValidateCurveParameters(basisCurves, out var basis, out var type, out var wrap, Input.GraphLogger))
                    continue;

                /*
                 * Allocates one end-to-end buffer containing the curves of the current prim.
                 * See method for Head + Buffer usage
                 */
                if (!CurvesImportUtils.TryGetCurveBuffer(basisCurves,
                    vertexCount => ValidateCurveVertexCount(basis, type, wrap, vertexCount, primPath, Input.GraphLogger),
                    out NativeArray<int> curveBufferHead,
                    out NativeArray<float3> curveBuffer,
                    Input.usdMetadata,
                    Input.GraphLogger))
                    continue;

                using (curveBufferHead)
                using (curveBuffer)
                {
                    /*
                     * Allocates one *empty* end-to-end buffer for the sample job results.
                     * Follows the same Head + Buffer rules as curve buffer.
                     */
                    GetEmptySampledCurveBuffer(curveBufferHead, curveBuffer, type, basis, wrap, Input.samplesPerStrand.value, out NativeArray<int> sampledCurveBufferHead, out NativeArray<float3> sampledCurveBuffer);

                    //Run the job to sample the "curve buffer" into the "sampled curve buffer"
                    switch (type)
                    {
                        case CurveType.Cubic:
                            new PopulateCubicSampledCurveBuffer(curveBufferHead, curveBuffer, sampledCurveBufferHead, sampledCurveBuffer, basis, wrap, type, Input.samplesPerStrand.value)
                                .Schedule(curveBufferHead.Length, 1).Complete();
                            break;
                        case CurveType.Linear:
                            new PopulateLinearSampledCurveBuffer(curveBufferHead, curveBuffer, sampledCurveBufferHead, sampledCurveBuffer, wrap)
                                .Schedule(curveBufferHead.Length, 1).Complete();
                            break;
                    }

                    Output.curveData.Add(primPath, new CurvesDescription
                    {
                        curveData = sampledCurveBuffer,
                        curveDataHeads = sampledCurveBufferHead,
                        wrap = wrap,
                        basis = basis,
                        type = type
                    });
                }
            }
        }

        //Follows the same end-to-end buffer and head strategy as GetCurveBuffer
        private static void GetEmptySampledCurveBuffer(NativeArray<int> curveBufferHead, NativeArray<float3> curveBuffer, CurveType type, CurveBasis basis, CurveWrap wrap, int samplesPerStrand, out NativeArray<int> sampledCurveBufferHead, out NativeArray<float3> sampledCurveBuffer)
        {
            sampledCurveBufferHead = new NativeArray<int>(curveBufferHead.Length,  Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            int head = 0;
            for (int i = 0; i < curveBufferHead.Length; i++)
            {
                sampledCurveBufferHead[i] = head;

                /*
                 * A segment is a section of a curve - each curve is made up of multiple segments
                 * We add '1' at the end of linear curves because it takes N + 1 vertices to define N segments
                 */
                var segmentCount = GetSegmentCount(type, wrap, CurvesImportUtils.GetLengthOfSlice(i, curveBufferHead, curveBuffer), GetVStep(basis));
                var sampleDensity = (int)math.ceil(math.max(MIN_SAMPLING_PER_SEGMENT, samplesPerStrand / segmentCount));
                head += type == CurveType.Cubic ? segmentCount * sampleDensity - segmentCount + 1 : segmentCount + 1;
            }

            sampledCurveBuffer = new NativeArray<float3>(head,  Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }

        /*
         * Basis Matrix is defined by the USD Geom Specification at:
         * https://graphics.pixar.com/usd/dev/api/class_usd_geom_basis_curves.html
         */
        private static float4x4 GetCubicMatrix(CurveBasis basis)
        {
            switch (basis)
            {
                case CurveBasis.Bezier:
                    return new float4x4(-1, 3, -3, 1, 3, -6, 3, 0, -3, 3, 0, 0, 1, 0, 0, 0);
                case CurveBasis.BSpline:
                    return new float4x4(-0.16666f, 0.5f, -0.5f, 0.16666f, 0.5f, -1, 0.5f, 0, -0.5f, 0, 0.5f, 0, 0.16666f, 0.66666f, 0.16666f, 0.0f);
                case CurveBasis.CatmullRom:
                    return new float4x4(-0.5f, 1.5f, -1.5f, 0.5f, 1.0f, -2.5f, 2.0f, -0.5f, -0.5f, 0, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
            }

            return default;
        }

        /*
         * Segment count is defined by the USD Geom Specification at:
         * https://graphics.pixar.com/usd/dev/api/class_usd_geom_basis_curves.html
         */
        private static int GetSegmentCount(CurveType type, CurveWrap wrap, int vertexCount, int vstep)
        {
            switch (type)
            {
                case CurveType.Linear:
                    switch (wrap)
                    {
                        case CurveWrap.NonPeriodic:
                            return vertexCount - 1;

                        case CurveWrap.Periodic:
                            return vertexCount;
                    }
                    break;

                case CurveType.Cubic:
                    switch (wrap)
                    {
                        //Curve is "-4" since we don't loop (there are 4 vertices in a segment), add an additional "1" at the end to account for the first segment
                        case CurveWrap.NonPeriodic:
                            return (vertexCount - 4) / vstep + 1;

                        //Count is "vertex count / vstep" - the loop adds an additional vertex
                        case CurveWrap.Periodic:
                            return vertexCount / vstep;

                        case CurveWrap.Pinned:
                            return vertexCount - 1;
                    }
                    break;
            }

            return default;
        }

        [BurstCompile]
        private struct PopulateCubicSampledCurveBuffer : IJobParallelFor
        {
            //Defines where in the curve buffer the i-th curve starts
            [ReadOnly]
            private NativeArray<int> m_CurveBufferHead;

            [ReadOnly]
            private NativeArray<float3> m_CurveBuffer;

            //Defines where in the sampled curve buffer the i-th curve starts
            [ReadOnly]
            private NativeArray<int> m_SampledCurveBufferHead;

            /*
             * The output buffer of sampled curves.
             * It is pre-calculated to be the precise size required to hold 'n' curves of 'm' segments
             */
            [NativeDisableContainerSafetyRestriction]
            private NativeArray<float3> m_SampledCurveBuffer;

            [ReadOnly]
            private CurveBasis m_Basis;

            [ReadOnly]
            private CurveWrap m_Wrap;

            [ReadOnly]
            private CurveType m_Type;

            [ReadOnly]
            private int m_VStep;

            [ReadOnly]
            private float4x4 m_CubicMatrix;

            [ReadOnly]
            private int m_SamplesPerStrand;

            public PopulateCubicSampledCurveBuffer(NativeArray<int> curveBufferHead, NativeArray<float3> curveBuffer, NativeArray<int> sampledCurveBufferHead, NativeArray<float3> sampledCurveBuffer, CurveBasis basis, CurveWrap wrap, CurveType type, int samplesPerSamplesPerStrand)
            {
                m_CurveBufferHead = curveBufferHead;
                m_CurveBuffer = curveBuffer;

                m_SampledCurveBufferHead = sampledCurveBufferHead;
                m_SampledCurveBuffer = sampledCurveBuffer;

                m_Basis = basis;
                m_Wrap = wrap;
                m_Type = type;
                m_VStep = GetVStep(m_Basis);
                m_CubicMatrix = GetCubicMatrix(m_Basis);

                m_SamplesPerStrand = samplesPerSamplesPerStrand;
            }

            float3 SampleCubicCurve(float3x4 points, float t)
            {
                float t2 = math.pow(t, 2);
                float t3 = math.pow(t, 3);

                float3 result = 0;

                for (int col = 0; col < 4; col++)
                    result += (m_CubicMatrix[col][0] * t3 + m_CubicMatrix[col][1] * t2 + m_CubicMatrix[col][2] * t + m_CubicMatrix[col][3]) * points[col];

                return result;
            }

            /*
             * A pinned curve injects two phantom points - one at the start, & one at the end, to ensure the interpolated curve begins / ends at p[0] & p[n-1]
             * As the USD spec states, clients are responsible for implementing them using a particular formular
             * https://graphics.pixar.com/usd/dev/api/class_usd_geom_basis_curves.html
             * Note - all of the indices in this function are offset by -1 because of the phantom points.
             */
            float3x4 GetPinnedCurvePoints(int currentCurve, int currentSegment, int totalSectionCount)
            {
                int readHead = m_CurveBufferHead[currentCurve] + (m_VStep * currentSegment);
                if (currentSegment == 0)
                {
                    //Return special case based upon phantom point #1
                    float3 p1 = m_CurveBuffer[readHead];
                    float3 p2 = m_CurveBuffer[readHead + 1];

                    return new float3x4(2 * p1 - p2, p1, p2, m_CurveBuffer[readHead + 2]);
                }

                if (currentSegment == totalSectionCount - 1)
                {
                    //Return special case based upon phantom point #2
                    float3 p1 = m_CurveBuffer[readHead];
                    float3 p2 = m_CurveBuffer[readHead + 1];

                    return new float3x4(m_CurveBuffer[readHead - 1], p1, p2,  2 * p2 - p1);
                }

                //Regular points - adjusted to account for phantom points
                return new float3x4(m_CurveBuffer[readHead - 1], m_CurveBuffer[readHead], m_CurveBuffer[readHead + 1], m_CurveBuffer[readHead + 2]);
            }

            /*
            * A periodic curve has at least one of its initial vertices repeated to "close the loop".
            * See https://graphics.pixar.com/usd/dev/api/class_usd_geom_basis_curves.html for more
            */
            float3x4 GetPeriodicCurvePoints(int currentCurve, int currentSegment)
            {
                var curveLength = CurvesImportUtils.GetLengthOfSlice(currentCurve, m_CurveBufferHead, m_CurveBuffer);

                var p0 = m_CurveBufferHead[currentCurve] + m_VStep * currentSegment % curveLength;
                var p1 = m_CurveBufferHead[currentCurve] + (m_VStep * currentSegment + 1) % curveLength;
                var p2 = m_CurveBufferHead[currentCurve] + (m_VStep * currentSegment + 2) % curveLength;
                var p3 = m_CurveBufferHead[currentCurve] + (m_VStep * currentSegment + 3) % curveLength;

                //Regular points
                return new float3x4(m_CurveBuffer[p0], m_CurveBuffer[p1], m_CurveBuffer[p2], m_CurveBuffer[p3]);
            }

            float3x4 GetNonPeriodicCurvePoints(int currentCurve, int currentSegment)
            {
                //Regular points
                var readHead = m_CurveBufferHead[currentCurve] + (m_VStep * currentSegment);
                return new float3x4(m_CurveBuffer[readHead], m_CurveBuffer[readHead + 1], m_CurveBuffer[readHead + 2], m_CurveBuffer[readHead + 3]);
            }

            public void Execute(int currentCurve)
            {
                var segmentCount = GetSegmentCount(m_Type, m_Wrap, CurvesImportUtils.GetLengthOfSlice(currentCurve, m_CurveBufferHead, m_CurveBuffer), m_VStep);
                var sampleDensity = (int)math.ceil(math.max(MIN_SAMPLING_PER_SEGMENT, m_SamplesPerStrand / segmentCount));

                var curveLength = CurvesImportUtils.GetLengthOfSlice(currentCurve, m_CurveBufferHead, m_CurveBuffer);
                var totalSegmentCount = GetSegmentCount(m_Type, m_Wrap, curveLength, m_VStep);
                var writeHead = m_SampledCurveBufferHead[currentCurve];

                for (var currentSegment = 0; currentSegment < totalSegmentCount; currentSegment++)
                {
                    var points = m_Wrap == CurveWrap.NonPeriodic ? GetNonPeriodicCurvePoints(currentCurve, currentSegment) :
                        m_Wrap == CurveWrap.Periodic ? GetPeriodicCurvePoints(currentCurve, currentSegment) :
                        GetPinnedCurvePoints(currentCurve, currentSegment, totalSegmentCount);

                    var start = currentSegment == 0 ? 0 : 1;
                    for (var k = start; k < sampleDensity; k++)
                    {
                        m_SampledCurveBuffer[writeHead] = SampleCubicCurve(points, (float)k / (sampleDensity - 1));
                        writeHead++;
                    }
                }
            }
        }

        [BurstCompile]
        private struct PopulateLinearSampledCurveBuffer : IJobParallelFor
        {
            //Defines where in the curve buffer the i-th curve starts
            [ReadOnly]
            private NativeArray<int> m_CurveBufferHead;

            [ReadOnly]
            private NativeArray<float3> m_CurveBuffer;

            //Defines where in the sampled curve buffer the i-th curve starts
            [ReadOnly]
            private NativeArray<int> m_SampledCurveBufferHead;

            /*
             * The output buffer of sampled curves.
             * It is pre-calculated to be the precise size required to hold 'n' curves of 'm' segments
             */
            [NativeDisableContainerSafetyRestriction]
            private NativeArray<float3> m_SampledCurveBuffer;

            [ReadOnly]
            private CurveWrap m_Wrap;


            public PopulateLinearSampledCurveBuffer(NativeArray<int> curveBufferHead, NativeArray<float3> curveBuffer, NativeArray<int> sampledCurveBufferHead, NativeArray<float3> sampledCurveBuffer, CurveWrap wrap)
            {
                m_CurveBufferHead = curveBufferHead;
                m_CurveBuffer = curveBuffer;

                m_SampledCurveBufferHead = sampledCurveBufferHead;
                m_SampledCurveBuffer = sampledCurveBuffer;

                m_Wrap = wrap;
            }

            public void Execute(int currentCurve)
            {
                var totalSegmentCount = CurvesImportUtils.GetLengthOfSlice(currentCurve, m_SampledCurveBufferHead, m_SampledCurveBuffer);
                for (var currentSegment = 0; currentSegment < totalSegmentCount; currentSegment++)
                {
                    if (m_Wrap != CurveWrap.Periodic || currentSegment < totalSegmentCount - 1)
                        //Handle base case - copy element from curve buffer
                        m_SampledCurveBuffer[m_SampledCurveBufferHead[currentCurve] + currentSegment] = m_CurveBuffer[m_CurveBufferHead[currentCurve] + currentSegment];
                    else
                        //Last element of periodic, handle special case - loop around
                        m_SampledCurveBuffer[m_SampledCurveBufferHead[currentCurve] + currentSegment] = m_CurveBuffer[m_CurveBufferHead[currentCurve]];
                }
            }
        }

        /// <summary>
        /// Validate an UsdGeomBasisCurves in function of its properties.
        /// </summary>
        /// <param name="basisCurves">The curves to validate.</param>
        /// <param name="basis">The returned basis of the curves.</param>
        /// <param name="type">The returned type of the curves.</param>
        /// <param name="wrap">The returned wrap of the curves.</param>
        /// <returns>Is the validation successful or not.</returns>
        private static bool ValidateCurveParameters(UsdGeomBasisCurves basisCurves, out CurveBasis basis, out CurveType type, out CurveWrap wrap, GraphLogger GraphLogger)
        {
            var primPath = basisCurves.GetPath();
            basis = CurveBasis.Undefined;
            wrap = CurveWrap.Undefined;
            type = GetType(basisCurves);
            if (type == CurveType.Undefined)
            {
                GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomBasisCurves. UsdGeomBasisCurves type data is not one of '{CurveType.Linear}' or '{CurveType.Cubic}'.", null, NodeWarnings.InvalidBasisCurveType);
                return false;
            }

            basis = CurveBasis.Undefined;

            //Only read basis info where the curve is cubic
            if (type == CurveType.Cubic)
            {
                basis = GetBasis(basisCurves);
                if (basis == CurveBasis.Undefined)
                {
                    GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomBasisCurves. UsdGeomBasisCurves basis data is not one of '{CurveBasis.Bezier}', '{CurveBasis.CatmullRom}', or '{CurveBasis.BSpline}'.", null, NodeWarnings.InvalidBasisCurveType);
                    return false;
                }
            }

            wrap = GetWrap(basisCurves);
            if (wrap == CurveWrap.Undefined)
            {
                GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomBasisCurves. UsdGeomBasisCurves wrap data is not one of '{CurveWrap.NonPeriodic}', '{CurveWrap.Periodic}' or '{CurveWrap.Pinned}'.", null, NodeWarnings.InvalidBasisCurveWrap);
                return false;
            }

            if ((basis == CurveBasis.Bezier || type == CurveType.Linear) && wrap == CurveWrap.Pinned)
            {
                GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomCurves. '{CurveBasis.Bezier}' or '{CurveType.Linear}' curves cannot have a '{CurveWrap.Pinned}' wrap. Switching to '{CurveWrap.NonPeriodic}' wrap.", null, NodeWarnings.InvalidBasisCurveWrapForCurveType);
                wrap = CurveWrap.NonPeriodic;
            }

            return true;
        }

        /// <summary>
        /// Vertex count validity is defined by the USD Geom Specification at:
        /// https://graphics.pixar.com/usd/dev/api/class_usd_geom_basis_curves.html
        /// We're not taking into account periodic or pinned wrap since we are creating the duplicated/phantom points during the sampling (ide. they are not in the usd file).
        /// </summary>
        /// <param name="basis">The basis of the curves.</param>
        /// <param name="type">The type of the curves.</param>
        /// <param name="wrap">The wrap of the curves.</param>
        /// <param name="vertexCount">Amount of vertex for this curve.</param>
        /// <param name="primPath">The curves prim path.</param>
        /// <returns>Is the validation successful or not.</returns>
        private static bool ValidateCurveVertexCount(CurveBasis basis, CurveType type, CurveWrap wrap, int vertexCount, string primPath, GraphLogger GraphLogger)
        {
            if ((type == CurveType.Linear && vertexCount < 2)
                || (type == CurveType.Cubic && vertexCount < 4)
                || (type == CurveType.Cubic && wrap == CurveWrap.NonPeriodic && (vertexCount - 4) % GetVStep(basis) != 0)
                || (type == CurveType.Cubic && wrap == CurveWrap.Periodic && vertexCount % GetVStep(basis) != 0)
                || (type == CurveType.Cubic && wrap == CurveWrap.Pinned && vertexCount < 2))
            {
                GraphLogger.LogImportWarning($"'{primPath}' UsdGeomCurves prim contains an invalid curve. VertexCount of '{vertexCount}' is invalid for a {type}-{basis}-{wrap} curve. It will be ignored.", null, NodeWarnings.InvalidVertexCountForBasisCurve);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Return the VStep for a curve basis.
        /// V-Step is defined by the USD Geom Specification at:
        /// https://graphics.pixar.com/usd/dev/api/class_usd_geom_basis_curves.html
        /// </summary>
        /// <param name="basis">The curve basis.</param>
        /// <returns>The VStep for this basis</returns>
        private static int GetVStep(CurveBasis basis)
        {
            switch (basis)
            {
                case CurveBasis.Bezier:
                    return 3;
                case CurveBasis.BSpline:
                case CurveBasis.CatmullRom:
                    return 1;

                //(Linear curves have no basis)
                case CurveBasis.Undefined:
                    return 0;
            }

            return default;
        }

        /// <summary>
        /// Extract the curve Type property from a UsdGeomBasisCurves.
        /// </summary>
        /// <param name="curves">The curve to extract the data from.</param>
        /// <returns>The curve Type.</returns>
        private static CurveType GetType(UsdGeomBasisCurves curves)
        {
            UsdAttribute type = curves.GetTypeAttr();
            TfToken typeToken = type.Get();
            switch (typeToken.GetText())
            {
                case "linear":
                    return CurveType.Linear;
                case "cubic":
                    return CurveType.Cubic;
                default:
                    return CurveType.Undefined;
            }
        }

        /// <summary>
        /// Extract the curve Basis property from a UsdGeomBasisCurves.
        /// </summary>
        /// <param name="curves">The curve to extract the data from.</param>
        /// <returns>The curve Basis.</returns>
        private static CurveBasis GetBasis(UsdGeomBasisCurves curves)
        {
            UsdAttribute basis = curves.GetBasisAttr();
            TfToken basisToken = basis.Get();
            switch (basisToken.GetText())
            {
                case "bezier":
                    return CurveBasis.Bezier;
                case "catmullRom":
                    return CurveBasis.CatmullRom;
                case "bspline":
                    return CurveBasis.BSpline;
                default:
                    return CurveBasis.Undefined;
            }
        }

        /// <summary>
        /// Extract the curve Wrap property from a UsdGeomBasisCurves.
        /// </summary>
        /// <param name="curves">The curve to extract the data from.</param>
        /// <returns>The curve Wrap.</returns>
        private static CurveWrap GetWrap(UsdGeomBasisCurves curves)
        {
            UsdAttribute wrap = curves.GetWrapAttr();
            TfToken wrapToken = wrap.Get();
            switch (wrapToken.GetText())
            {
                case "nonperiodic":
                    return CurveWrap.NonPeriodic;
                case "periodic":
                    return CurveWrap.Periodic;
                case "pinned":
                    return CurveWrap.Pinned;
                default:
                    return CurveWrap.Undefined;
            }
        }
    }
}
