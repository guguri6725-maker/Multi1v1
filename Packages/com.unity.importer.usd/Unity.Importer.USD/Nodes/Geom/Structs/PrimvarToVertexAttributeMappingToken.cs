using pxr;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Allow to provide a PrimVar name mapping for mesh components, using tokens.
    /// </summary>
    public class PrimvarToVertexAttributeMappingToken
    {
        /// <summary>
        /// Color set
        /// </summary>
        public TfToken[] Color = {};

        /// <summary>
        /// Texture Coordinates #0
        /// </summary>
        public TfToken[] TextCoord0 = {};

        /// <summary>
        /// Texture Coordinates #1
        /// </summary>
        public TfToken[] TextCoord1 = {};

        /// <summary>
        /// Texture Coordinates #2
        /// </summary>
        public TfToken[] TextCoord2 = {};

        /// <summary>
        /// Texture Coordinates #3
        /// </summary>
        public TfToken[] TextCoord3 = {};

        /// <summary>
        /// Texture Coordinates #4
        /// </summary>
        public TfToken[] TextCoord4 = {};

        /// <summary>
        /// Texture Coordinates #5
        /// </summary>
        public TfToken[] TextCoord5 = {};

        /// <summary>
        /// Texture Coordinates #6
        /// </summary>
        public TfToken[] TextCoord6 = {};

        /// <summary>
        /// Texture Coordinates #7
        /// </summary>
        public TfToken[] TextCoord7 = {};

        /// <summary>
        /// Constructor
        /// </summary>
        public PrimvarToVertexAttributeMappingToken(PrimvarToVertexAttributeMapping from)
        {
            StringArrayToTfTokenArray(from.Color, ref Color);
            StringArrayToTfTokenArray(from.TextCoord0, ref TextCoord0);
            StringArrayToTfTokenArray(from.TextCoord1, ref TextCoord1);
            StringArrayToTfTokenArray(from.TextCoord2, ref TextCoord2);
            StringArrayToTfTokenArray(from.TextCoord3, ref TextCoord3);
            StringArrayToTfTokenArray(from.TextCoord4, ref TextCoord4);
            StringArrayToTfTokenArray(from.TextCoord5, ref TextCoord5);
            StringArrayToTfTokenArray(from.TextCoord6, ref TextCoord6);
            StringArrayToTfTokenArray(from.TextCoord7, ref TextCoord7);
        }

        private static void StringArrayToTfTokenArray(string[] from, ref TfToken[] to)
        {
            to = new TfToken[from.Length];
            for (var i = 0; i < from.Length; i++)
                to[i] = new TfToken(from[i]);
        }
    }
}
