#if HAIR_0_OR_HIGHER
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.DemoTeam.Hair;
using Unity.Mathematics;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// A custom data set used to provide USD curve to the hair system.
    /// </summary>
    public class UsdHairAssetCustomData : HairAssetCustomData, IDisposable
    {
        /// <summary>
        /// The length of each curve in the <see cref="curveData"/> buffer.
        /// </summary>
        public NativeArray<int> curveLengths;

        /// <summary>
        /// A buffer containing curves point, written consecutively.
        /// </summary>
        public NativeArray<float3> curveData;

        public override bool AcquireCurves(out HairAssetProvisional.CurveSet curveSet, Allocator allocator)
        {
            curveSet = new HairAssetProvisional.CurveSet(curveLengths.Length, curveData.Length, allocator);
            curveSet.vertexFeatures = HairAssetProvisional.CurveSet.VertexFeatures.Position;
            curveSet.curveCount = curveLengths.Length;

            unsafe
            {
                curveSet.curveVertexCount.AddRange(curveLengths.GetUnsafePtr(), curveLengths.Length);
                curveSet.vertexDataPosition.AddRange(curveData.GetUnsafePtr(), curveData.Length);
            }

            return true; // success
        }

        public void Dispose()
        {
            curveLengths.Dispose();
            curveData.Dispose();
        }
    }
}
#endif
