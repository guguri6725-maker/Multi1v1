using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using pxr;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Helper class used by nodes importing UsdGeomCurves
    /// </summary>
    public class CurvesImportUtils
    {
        /// <summary>
        /// Simple helper function to return the last point index of a curve at index "index" - force it to be inlined.
        /// </summary>
        /// <param name="index">Current curve index.</param>
        /// <param name="bufferHead">A buffer of curves head (eg. bufferHead[i] => index in buffer where the ith curve points starts).</param>
        /// <param name="buffer">A buffer containing the points of all the curves, written consecutively.</param>
        /// <typeparam name="T">Type used by the buffer.</typeparam>
        /// <returns>The last point index in buffer for the index th curve.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLastIndexOfSlice<T>(int index, NativeArray<int> bufferHead, NativeArray<T> buffer) where T : struct
        {
            return (index < bufferHead.Length - 1 ? bufferHead[index + 1] : buffer.Length) - 1;
        }

        /// <summary>
        /// Simple helper function to return the buffer length of a curve at index "index" - force it to be inlined.
        /// </summary>
        /// <param name="index">Current curve index.</param>
        /// <param name="bufferHead">A buffer of curves head (eg. bufferHead[i] => index in buffer where the ith curve points starts).</param>
        /// <param name="buffer">A buffer containing the points of all the curves, written consecutively.</param>
        /// <typeparam name="T">Type used by the buffer.</typeparam>
        /// <returns>The length of the point array that comprised the index th curve.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLengthOfSlice<T>(int index, NativeArray<int> bufferHead, NativeArray<T> buffer) where T : struct
        {
            //Length of a curve is the element count between the i-th curve, and the i-th + 1
            var endPointIndex = GetLastIndexOfSlice(index, bufferHead, buffer);
            return endPointIndex - bufferHead[index] + 1;
        }

        /// <summary>
        /// This extracts curve data from a prim and allocates several buffer used for curve sampling :
        /// - curveBuffer, an end-to-end buffer for all curves vertices within this USD Prim
        /// - curveBufferHead, an array which point at the start location of each curve
        /// - vertexCounts, an array describing the length of each curves
        /// </summary>
        /// <remarks>
        /// Usd metadata, like scale and axis conversion, are directly applied to the point for more efficiency.
        ///
        /// E.g. Considering two curves curve #1 = [(0,0,0), (1,0,0), (1,0,0)] &amp; curve #2 = [(0,0,0), (1,1,1), (2,2,2)] in a prim
        /// This yields :
        /// - curveBufferHead = [0, 3]
        /// - curveBuffer = [(0,0,0), (1,0,0), (1,0,0), (0,0,0), (1,1,1), (2,2,2)]
        /// - vertexCounts = [3, 3]
        /// </remarks>
        /// <param name="curves">The UsdGeomCurves to be parsed.</param>
        /// <param name="vertexCountValidation">A function used to validate each curves in function of their vertex count.</param>
        /// <param name="curveBufferHead">A returned buffer of curves head (eg. bufferHead[i] => index in buffer where the ith curve points starts).</param>
        /// <param name="curveBuffer">A returned buffer containing the points of all the curves, written consecutively.</param>
        /// <param name="usdMetadata">The imported usd metadata.</param>
        /// <param name="GraphLogger">Optional graph logger.</param>
        /// <returns>The result of the data extraction process.</returns>
        public static bool TryGetCurveBuffer(UsdGeomCurves curves, Func<int, bool> vertexCountValidation, out NativeArray<int> curveBufferHead,
            out NativeArray<float3> curveBuffer, UsdStageMetadata usdMetadata, GraphLogger GraphLogger = null)
        {
            VtVec3fArray curveBufferMarshalled = new VtVec3fArray();
            curveBufferHead = new NativeArray<int>();
            curveBuffer = new NativeArray<float3>();

            //If no points can be extracted from the prim, abort
            var timeCode = UsdTimeCode.EarliestTime();
            if (!curves.ComputePointsAtTime(curveBufferMarshalled, timeCode, timeCode) || curveBufferMarshalled.size() == 0)
            {
                if (GraphLogger != null)
                {
                    GraphLogger.LogImportWarning($"'{curves.GetPath()}' is an invalid UsdGeomCurves prim. It has an empty or non-existent Points array.", null, NodeWarnings.EmptyPointsForCurve);
                }
                else
                {
                    Debug.LogWarning($"'{curves.GetPath()}' is an invalid UsdGeomCurves prim. It has an empty or non-existent Points array.");
                }
                return false;
            }

            //If no vertexCount attribute exists, all points will be considered to be part of a single curve and we'll use the size of the vertexArray (curveBufferMarshalled)
            var originVertexCounts = GetVertexCounts(curves, (int)curveBufferMarshalled.size());

            //Ensure the extracted vertexArray and the vertexCount array are valid compared to each other
            if (!ValidateCurvesVertices(curveBufferMarshalled, originVertexCounts, curves.GetPath(), GraphLogger))
                return false;

            //Parse all curves in this batch but ignore curves that do not have a valid vertexCount
            TrimInvalidCurves(curveBufferMarshalled, originVertexCounts, vertexCountValidation, out var headBufferList, out var curveBufferList, usdMetadata);

            //If all curves were discarded, abort
            if (headBufferList.Count == 0)
            {
                if (GraphLogger != null)
                {
                    GraphLogger.LogImportWarning($"'{curves.GetPath()}' is an invalid UsdGeomCurves prim. All curves were ignored due to invalid data.", null, NodeWarnings.InvalidCurve);
                }
                else
                {
                    Debug.LogWarning($"'{curves.GetPath()}' is an invalid UsdGeomCurves prim. All curves were ignored due to invalid data.");
                }
                return false;
            }

            curveBuffer = new NativeArray<float3>(curveBufferList.ToArray(), Allocator.TempJob);
            curveBufferHead = new NativeArray<int>(headBufferList.ToArray(), Allocator.TempJob);
            return true;
        }

        /// <summary>
        /// Extract the vertexCount usd property from given curves.
        /// If the property is missing, will return a single vertexCount entry equals to the count of points.
        /// (ide. we consider that the points represent a single curve)
        /// </summary>
        /// <param name="curves">The curve data to be parsed.</param>
        /// <param name="totalPointCount">The count of points present in this prim.</param>
        /// <returns>A vertex count array describing how much point comprised each consecutive curves in this prim (derived from the vertexCount usd property)</returns>
        public static int[] GetVertexCounts(UsdGeomCurves curves, int totalPointCount)
        {
            var vertexCountsAttr = curves.GetCurveVertexCountsAttr();
            VtIntArray vertexCountsArr = vertexCountsAttr.Get();

            if (vertexCountsArr.size() <= 0)
                return new[] {totalPointCount};

            var vertexCounts = new int[vertexCountsArr.size()];
            vertexCountsArr.CopyToArray(vertexCounts);
            return vertexCounts;
        }

        /// <summary>
        /// Validate a set of curve based on its VertexCount and the provided vertices.
        /// </summary>
        /// <param name="curvesVertices">The curves vertices, written consecutively.</param>
        /// <param name="vertexCounts">The vertexCount array for this prim.</param>
        /// <param name="primPath">The prim path those curves belong to.</param>
        /// <param name="GraphLogger">Optional graph logger.</param>
        /// <returns>The result of this validation.</returns>
        public static bool ValidateCurvesVertices(VtVec3fArray curvesVertices, int[] vertexCounts, string primPath, GraphLogger GraphLogger = null)
        {
            var curvesVerticesSize = curvesVertices.size();
            var totalVertex = 0;
            foreach (var count in vertexCounts)
            {
                totalVertex += count;
            }

            if (totalVertex != curvesVerticesSize)
            {
                if (GraphLogger != null)
                {
                    GraphLogger.LogImportWarning($"'{primPath}' is an invalid UsdGeomCurves prim. UsdGeomCurves VertexCount sum is different than the count of provided vertices. Expected {totalVertex} Actual {curvesVerticesSize}.", null, NodeWarnings.NonMatchingVertexCountForCurve);
                }
                else
                {
                    Debug.LogWarning($"'{primPath}' is an invalid UsdGeomCurves prim. UsdGeomCurves VertexCount sum is different than the count of provided vertices. Expected {totalVertex} Actual {curvesVerticesSize}.");
                }
                return false;
            }

            return true;
        }

        private static void TrimInvalidCurves(VtVec3fArray curveBufferMarshalled, int[] originVertexCounts, Func<int, bool> vertexCountValidation, out List<int> headBufferList,
            out List<float3> curveBufferList, UsdStageMetadata usdMetadata)
        {
            //Parse all curves in this batch but ignore curves that do not have a valid vertexCount
            headBufferList = new List<int>(originVertexCounts.Length);
            curveBufferList = new List<float3>((int)curveBufferMarshalled.size());
            int totalVertexAdded = 0;
            var vertexHead = 0;
            for (var curveIndex = 0; curveIndex < originVertexCounts.Length; curveIndex++)
            {
                //If the curve has a valid vertex count, add its vertexCount, head and vertices to the resulting buffers.
                //If not, only advance our vertexHead to the next curve first vertex and ignore this curve.
                if (vertexCountValidation(originVertexCounts[curveIndex]))
                {
                    headBufferList.Add(totalVertexAdded);
                    totalVertexAdded += originVertexCounts[curveIndex];

                    var maxVertex = vertexHead + originVertexCounts[curveIndex];
                    for (var vertexIndex = vertexHead; vertexIndex < maxVertex; vertexIndex++)
                    {
                        var v = curveBufferMarshalled[vertexIndex];
                        if (usdMetadata.isStageZup)
                        {
                            curveBufferList.Add(new float3(v[0], v[2], v[1]) * usdMetadata.metersPerUnit);
                        }
                        else
                        {
                            curveBufferList.Add(new float3(v[0], v[1], -v[2]) * usdMetadata.metersPerUnit);
                        }
                    }
                }

                vertexHead += originVertexCounts[curveIndex];
            }
        }
    }
}
