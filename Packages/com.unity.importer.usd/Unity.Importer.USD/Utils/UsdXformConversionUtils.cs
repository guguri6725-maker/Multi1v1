using pxr;
using Unity.Mathematics;
using USD.NET;

namespace Unity.Importer.USD
{
    internal class UsdXformConversionUtils
    {
        internal static readonly float4x4 k_Toggle_YZ = new(1f, 0f, 0f, 0f, 0f, 0, 1f, 0f, 0f, 1f, 0, 0f, 0f, 0f, 0f, 1f);
        internal static readonly float4x4 k_FlipZ = float4x4.Scale(1, 1, -1);

        internal static float4x4 FromMatrix(GfMatrix4d gfMat, bool withBasisChange = false, bool ZToYUp = false)
        {
            // Note that USD is row-major and Unity is column-major, the arguments below are pivoted
            // to account for this.
            //We also change the basis from right handed to left handed.
            float4x4 ret = new float4x4();
            double[] tmp = UsdIo.ArrayAllocator.Malloc<double>(16);
            gfMat.CopyToArray(tmp);

            ret.c0.x = (float)tmp[0];
            ret.c0.y = (float)tmp[1];
            ret.c0.z = (float)tmp[2];
            ret.c0.w = (float)tmp[3];

            ret.c1.x = (float)tmp[4 + 0];
            ret.c1.y = (float)tmp[4 + 1];
            ret.c1.z = (float)tmp[4 + 2];
            ret.c1.w = (float)tmp[4 + 3];

            ret.c2.x = (float)tmp[8 + 0];
            ret.c2.y = (float)tmp[8 + 1];
            ret.c2.z = (float)tmp[8 + 2];
            ret.c2.w = (float)tmp[8 + 3];

            ret.c3.x = (float)tmp[12 + 0];
            ret.c3.y = (float)tmp[12 + 1];
            ret.c3.z = (float)tmp[12 + 2];
            ret.c3.w = (float)tmp[12 + 3];
            UsdIo.ArrayAllocator.Free(tmp.GetType(), (uint)tmp.Length, tmp);

            if (withBasisChange)
            {
                ret = ZToYUp
                    ? math.mul(k_Toggle_YZ, math.mul(ret, k_Toggle_YZ))
                    : math.mul(k_FlipZ, math.mul(ret, k_FlipZ));
            }

            return ret;
        }

        public static TransformData Decompose(float4x4 matrix)
        {
            if (matrix[3][3] == 0.0f)
            {
                return new TransformData();
            }

            var trs = new TransformData();
            // Next take care of translation (easy).
            trs.translation = matrix[3].xyz;

            var rows = new float3x3();

            // Now get scale and shear.
            rows[0] = matrix[0].xyz;
            rows[1] = matrix[1].xyz;
            rows[2] = matrix[2].xyz;

            // Compute X scale factor and normalize first row.
            trs.scale.x = math.length(rows[0]);
            rows[0] *= trs.scale.x == 0 ? 0 : 1 / trs.scale.x;

            // Compute XY shear factor and make 2nd row orthogonal to 1st.
            float3 Skew;
            Skew.z = math.dot(rows[0], rows[1]);
            rows[1] = WeightedAvg(rows[1], rows[0], 1, -Skew.z);

            // Now, compute Y scale and normalize 2nd row.
            trs.scale.y = math.length(rows[1]);
            rows[1] *= trs.scale.y == 0 ? 0 : 1 / trs.scale.y;

            // Compute XZ and YZ shears, orthogonalize 3rd row.
            Skew.y = math.dot(rows[0], rows[2]);
            rows[2] = WeightedAvg(rows[2], rows[0], 1, -Skew.y);

            Skew.x = math.dot(rows[1], rows[2]);
            rows[2] = WeightedAvg(rows[2], rows[1], 1, -Skew.x);

            // Next, get Z scale and normalize 3rd row.
            trs.scale.z = math.length(rows[2]);
            rows[2] *= trs.scale.z == 0 ? 0 : 1 / trs.scale.z;

            // At this point, the matrix (in rows[]) is orthonormal.
            // Check for a coordinate system flip.  If the determinant
            // is -1, then negate the matrix and the scaling factors.
            float3 Pdum3 = math.cross(rows[1], rows[2]);
            if (math.dot(rows[0], Pdum3) < 0)
            {
                trs.scale *= -1.0f;

                rows[0] *= -1.0f;
                rows[1] *= -1.0f;
                rows[2] *= -1.0f;
            }

            // Now, get the rotations out, as described in the gem.
            {
                float root, trace = rows[0].x + rows[1].y + rows[2].z;
                if (trace > 0)
                {
                    root = math.sqrt(trace + 1.0f);
                    trs.rotation.value.w = 0.5f * root;
                    root = 0.5f / root;
                    trs.rotation.value.x = root * (rows[1].z - rows[2].y);
                    trs.rotation.value.y = root * (rows[2].x - rows[0].z);
                    trs.rotation.value.z = root * (rows[0].y - rows[1].x);
                } // End if > 0
                else
                {
                    int i = 0, j = 1, k = 2;
                    if (rows[1].y > rows[0].x)
                    {
                        i = 1;
                        j = 2;
                        k = 0;
                    }

                    if (rows[2].z > rows[i][i])
                    {
                        i = 2;
                        j = 0;
                        k = 1;
                    }

                    root = math.sqrt(rows[i][i] - rows[j][j] - rows[k][k] + 1.0f);

                    trs.rotation.value[i] = 0.5f * root;
                    root = 0.5f / root;
                    trs.rotation.value[j] = root * (rows[i][j] + rows[j][i]);
                    trs.rotation.value[k] = root * (rows[i][k] + rows[k][i]);
                    trs.rotation.value.w = root * (rows[j][k] - rows[k][j]);
                } // End if <= 0
            }

            return trs;
        }

        public static quaternion GfQuatfToQuaternion(GfQuatf gfQuat)
        {
            var imaginaryCoeffs = gfQuat.GetImaginary();

            return new quaternion(imaginaryCoeffs[0], imaginaryCoeffs[1],
                imaginaryCoeffs[2], gfQuat.GetReal());
        }

        public static quaternion CalculateQuaternionAdjustedToStage(quaternion quaternion, bool isStageZup)
        {
            if (isStageZup)
            {
                (quaternion.value.z, quaternion.value.y) = (quaternion.value.y, quaternion.value.z);
                quaternion.value.z *= -1f;
            }
            quaternion.value.x *= -1f;
            quaternion.value.y *= -1f;

            return quaternion;
        }

        public static float3 CalculateScaleAdjustedToStage(float3 scale, bool isStageZup)
        {
            if (isStageZup)
            {
                (scale.z, scale.y) = (scale.y, scale.z);
            }

            return scale;
        }

        public static float3 CalculateTranslationAdjustedToStage(float3 translation, bool isStageZup)
        {
            if (isStageZup)
            {
                (translation.z, translation.y) = (translation.y, translation.z);
            }
            else
            {
                translation.z *= -1f;
            }

            return translation;
        }

        private static float3 WeightedAvg(
            float3 a,
            float3 b,
            float aWeight,
            float bWeight)
        {
            return (a * aWeight) + (b * bWeight);
        }
    }

    /// <summary>
    /// A set of data representing the main component of a Unity Transform.
    /// </summary>
    public struct TransformData
    {
        /// <summary>
        /// The transform translation.
        /// </summary>
        public float3 translation;

        /// <summary>
        /// The transform scale.
        /// </summary>
        public float3 scale;

        /// <summary>
        /// The transform rotation.
        /// </summary>
        public quaternion rotation;
    }
}
