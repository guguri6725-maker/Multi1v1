using System;

namespace Unity.Importer.USD
{
    [Flags]
    internal enum VertexAttributesFlags
    {
        None = 0,
        Normal = 1,
        Color = 2,
        TexCoord0 = 4,
        TexCoord1 = 8,
        TexCoord2 = 16,
        TexCoord3 = 32,
        TexCoord4 = 64,
        TexCoord5 = 128,
        TexCoord6 = 256,
        TexCoord7 = 512,
        BlendWeight = 1024,
        BlendIndices = 2048
    }
}
