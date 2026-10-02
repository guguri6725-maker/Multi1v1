using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Represents a set of curve data buffers.
    /// </summary>
    public struct CurvesDescription : IDisposable
    {
        /// <summary>
        /// The starting index of each curve set of point in the <see cref="curveData"/> buffer.
        /// </summary>
        public NativeArray<int> curveDataHeads;

        /// <summary>
        /// A buffer containing curve points, written consecutively.
        /// </summary>
        public NativeArray<float3> curveData;

        /// <summary>
        /// Basis of the curves.
        /// </summary>
        public CurveBasis basis;

        /// <summary>
        /// Wrap of the curves.
        /// </summary>
        public CurveWrap wrap;

        /// <summary>
        /// A buffer containing curves point, written consecutively.
        /// </summary>
        public CurveType type;

        /// <summary>
        /// See documentation for &lt;IDisposable.Dispose&gt; for more details
        /// </summary>
        public void Dispose()
        {
            curveDataHeads.Dispose();
            curveData.Dispose();
        }
    }
}
