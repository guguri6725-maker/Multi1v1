using Unity.Collections;

namespace Unity.Importer.USD
{
    internal struct Subset
    {
        public NativeArray<int> FaceIndices;

        public void Dispose()
        {
            FaceIndices.Dispose();
        }
    }
}
