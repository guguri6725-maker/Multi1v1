using System;
using System.Collections.Generic;
using Unity.Collections;
using pxr;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Importer;


namespace Unity.Importer.USD
{
    /// <summary>
    /// Structure holding all the information required to create blend shapes in Unity
    /// </summary>
    public struct BlendShape : IDisposable
    {
        /// <summary>
        /// The point indices
        /// </summary>
        [NoAlias, NativeDisableParallelForRestriction]
        public NativeArray<int> PointIndices;

        /// <summary>
        /// The point offsets
        /// </summary>
        [NoAlias, NativeDisableParallelForRestriction]
        public NativeArray<float3> Offsets;

        /// <summary>
        /// The normal offsets
        /// </summary>
        [NoAlias, NativeDisableParallelForRestriction]
        public NativeArray<float3> NormalOffsets;

        /// <summary>
        /// The weights
        /// </summary>
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> Weights;

        /// <summary>
        /// The map between the point indexes and their location
        /// </summary>
        [NativeDisableContainerSafetyRestriction]
        public UnsafeParallelHashMap<int, int> PointIndexToLocation;

        /// <summary>
        /// The number of elements that the structure hold
        /// </summary>
        public readonly int FrameSize;

        /// <summary>
        /// Constructs a BlendShape data structure from Usd data
        /// </summary>
        public BlendShape(UsdSkelBlendShape blendShapeIn, UsdTimeCode timeCode, GraphLogger GraphLogger = null)
        {
            var inBetweensVector = blendShapeIn.GetInbetweens();
            var inBetweens = OrderInBetween(inBetweensVector, blendShapeIn.GetPath(), GraphLogger);
            var inBetweenCount = inBetweens.Count;

            // We will write each blendshape frame consecutively in our array.
            // considering a blendshape : frame_0 with offsets off_00, off_01, off_02 AND frame_1 with offsets off_10, off_11, off_12
            // the resulting offsets array will be : off_00, off_01, off_02, off_10, off_11, off_12
            VtIntArray fromPointIndices = blendShapeIn.GetPointIndicesAttr().Get(timeCode);
            VtVec3fArray fromOffsets = blendShapeIn.GetOffsetsAttr().Get(timeCode);
            VtVec3fArray fromNormalOffsets = blendShapeIn.GetNormalOffsetsAttr().Get(timeCode);

            FrameSize = (int)math.max(fromPointIndices.size(), fromOffsets.size());

            //Total size must take into account any in-betweens + the default last frame
            var totalSize = (inBetweenCount + 1) * FrameSize;

            PointIndices = new NativeArray<int>(FrameSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            PointIndexToLocation = new UnsafeParallelHashMap<int, int>(FrameSize, Allocator.TempJob);
            Offsets = new NativeArray<float3>(totalSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NormalOffsets = new NativeArray<float3>(totalSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            Weights = new NativeArray<float>(inBetweenCount + 1, Allocator.TempJob);  //in-between count + default last frame

            WriteInBetweens(inBetweens);
            WriteLastFrame(inBetweenCount, fromPointIndices, fromOffsets, fromNormalOffsets);
        }

        private void WriteLastFrame(int inBetweenCount, VtIntArray fromPointIndices, VtVec3fArray fromOffsets, VtVec3fArray fromNormalOffsets)
        {
            var pointIndicesSize = fromPointIndices.size();
            var normalOffsetsSize = fromNormalOffsets.size();

            var startIndex = inBetweenCount * FrameSize;
            for (var i = 0; i < FrameSize; i++)
            {
                var index = i < pointIndicesSize ? fromPointIndices[i] : i;
                PointIndices[i] = index;
                PointIndexToLocation[index] = i;

                Offsets[startIndex + i] = new float3(fromOffsets[i][0], fromOffsets[i][1], fromOffsets[i][2]);
                NormalOffsets[startIndex + i] = i < normalOffsetsSize ? new float3(fromNormalOffsets[i][0], fromNormalOffsets[i][1], fromNormalOffsets[i][2]) : float3.zero;
            }
            Weights[inBetweenCount] = 1f;
        }

        private void WriteInBetweens(List<UsdSkelInbetweenShape> inBetweens)
        {
            for (var i = 0; i < inBetweens.Count; i++)
            {
                var inBetween = inBetweens[i];
                var offsetsIB = new VtVec3fArray();
                inBetween.GetOffsets(offsetsIB);
                var normalOffsetsIB = new VtVec3fArray();
                inBetween.GetNormalOffsets(normalOffsetsIB);
                var normalOffsetsSize = normalOffsetsIB.size();

                var startIndex = FrameSize * i;
                for (var j = 0; j < offsetsIB.size(); j++)
                {
                    var index = startIndex + j;
                    Offsets[index] = new float3(offsetsIB[j][0], offsetsIB[j][1], offsetsIB[j][2]);
                    NormalOffsets[index] = j < normalOffsetsSize ? new float3(normalOffsetsIB[j][0], normalOffsetsIB[j][1], normalOffsetsIB[j][2]) : float3.zero;
                }

                inBetween.GetWeight(out var weight);
                Weights[i] = weight;
            }
        }

        private static List<UsdSkelInbetweenShape> OrderInBetween(UsdSkelInbetweenShapeVector inBetweens, string blendShapeName, GraphLogger GraphLogger)
        {
            var result = new List<UsdSkelInbetweenShape>();
            var found = new HashSet<float>();
            for (var i = 0; i < inBetweens.Count; i++)
            {
                var inBetween = inBetweens[i];
                if (!inBetween.GetWeight(out var weight))
                {
                    if (GraphLogger != null)
                    {
                        GraphLogger.LogImportWarning($"Could not extract weight for blendshape in-between '{blendShapeName}' at index {i}. Cancelling in-between import.", null, NodeWarnings.CouldNotExtractBlendShapeInBetweenWeight);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not extract weight for blendshape in-between '{blendShapeName}' at index {i}. Cancelling in-between import.");
                    }
                    return new List<UsdSkelInbetweenShape>(0);
                }

                if (weight < 0 || weight >= 1f)
                {
                    if (GraphLogger != null)
                    {
                        GraphLogger.LogImportWarning($"Found out-of-bounds [0,1[ weight '{weight}' for blendshape in-between '{blendShapeName}' at index {i}. Cancelling in-between import.", null, NodeWarnings.OutOfBoundBlendShapeWeight);
                    }
                    else
                    {
                        Debug.LogWarning($"Found out-of-bounds [0,1[ weight '{weight}' for blendshape in-between '{blendShapeName}' at index {i}. Cancelling in-between import.");
                    }
                    return new List<UsdSkelInbetweenShape>(0);
                }

                if (found.Contains(weight) || Math.Abs(weight - 1f) < 0.0001f)
                {
                    if (GraphLogger != null)
                    {
                        GraphLogger.LogImportWarning($"Found duplicated weight '{weight}' for blendshape in-between '{blendShapeName}' at index {i}. Cancelling in-between import.", null, NodeWarnings.DuplicateBlendShapeWeight);
                    }
                    else
                    {
                        Debug.LogWarning($"Found duplicated weight '{weight}' for blendshape in-between '{blendShapeName}' at index {i}. Cancelling in-between import.");
                    }
                    return new List<UsdSkelInbetweenShape>(0);
                }

                found.Add(weight);
                result.Add(inBetween);
            }

            result.Sort(new UsdSkelInbetweenShapeComparer());
            return result;
        }

        private struct UsdSkelInbetweenShapeComparer : IComparer<UsdSkelInbetweenShape>
        {
            public int Compare(UsdSkelInbetweenShape x, UsdSkelInbetweenShape y)
            {
                x.GetWeight(out var xWeight);
                y.GetWeight(out var yWeight);
                return xWeight < yWeight ? -1 : 1;
            }
        }

        /// <summary>
        /// See documentation for &lt;IDisposable.Dispose&gt; for more details
        /// </summary>
        public void Dispose()
        {
            PointIndices.Dispose();
            Offsets.Dispose();
            NormalOffsets.Dispose();
            Weights.Dispose();
            PointIndexToLocation.Dispose();
        }
    }
}
