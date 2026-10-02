using System;
using Unity.Collections;

namespace Unity.Importer.USD
{
    internal struct PrimVarData<T> where T : struct, IEquatable<T>, IFormattable
    {
        public NativeArray<T> Values;
        public PrimVarInterpolation Interpolation;
        public NativeArray<T> OutValues;

        public void Dispose()
        {
            if (Values.IsCreated) Values.Dispose();
            if (OutValues.IsCreated) OutValues.Dispose();
        }
    }
}
