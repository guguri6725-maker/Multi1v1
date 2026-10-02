using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Allow to provide a PrimVar name mapping for mesh components.
    /// </summary>
    /// <remarks>
    /// e.g : If a DCC is using an unsupported PrimVar name for TextCoord0, a user can add it using this graph setting.
    /// </remarks>
    [Serializable]
    public class PrimvarToVertexAttributeMapping : ISerializationCallbackReceiver
    {
        /// <summary>
        /// Color set
        /// </summary>
        public string[] Color = { "colorSet1" , "vertexColor" };

        /// <summary>
        /// Texture Coordinates #0
        /// </summary>
        public string[] TextCoord0 = { "map1", "st", "st0", "UVMap", "Texture_uv" }; // Maya ( map1), Houdini (st), Blender (UVMap)

        /// <summary>
        /// Texture Coordinates #1
        /// </summary>
        public string[] TextCoord1 = { "map2", "st1", "uvSet", "UVMap_001" };

        /// <summary>
        /// Texture Coordinates #2
        /// </summary>
        public string[] TextCoord2 = { "map3", "st2", "uvSet1", "UVMap_002" };

        /// <summary>
        /// Texture Coordinates #3
        /// </summary>
        public string[] TextCoord3 = { "map4", "st3", "uvSet2", "UVMap_003" };

        /// <summary>
        /// Texture Coordinates #4
        /// </summary>
        public string[] TextCoord4 = { "map5", "st4", "uvSet3", "UVMap_004" };

        /// <summary>
        /// Texture Coordinates #5
        /// </summary>
        public string[] TextCoord5 = { "map6", "st5", "uvSet4", "UVMap_005" };

        /// <summary>
        /// Texture Coordinates #6
        /// </summary>
        public string[] TextCoord6 = { "map7", "st6", "uvSet5", "UVMap_006" };

        /// <summary>
        /// Texture Coordinates #7
        /// </summary>
        public string[] TextCoord7 = { "st7", "uvSet6", "UVMap_007" };

        private Dictionary<string, string> vertexAttributeToPrimvarMap;

        /// <summary>
        /// Mapping from vertex attributes' names to Color and Texture Coordinates
        /// </summary>
        public Dictionary<string, string> VertexAttributeToPrimvarMap => vertexAttributeToPrimvarMap;

        public PrimvarToVertexAttributeMapping(string[] color = null, string[] textCoord0 = null, string[] textCoord1 = null,
                                               string[] textCoord2 = null, string[] textCoord3 = null, string[] textCoord4 = null, string[] textCoord5 = null, string[] textCoord6 = null,
                                               string[] textCoord7 = null)
        {
            if (color != null) Color = color;
            if (textCoord0 != null) TextCoord0 = textCoord0;
            if (textCoord1 != null) TextCoord1 = textCoord1;
            if (textCoord2 != null) TextCoord2 = textCoord2;
            if (textCoord3 != null) TextCoord3 = textCoord3;
            if (textCoord4 != null) TextCoord4 = textCoord4;
            if (textCoord5 != null) TextCoord5 = textCoord5;
            if (textCoord6 != null) TextCoord6 = textCoord6;
            if (textCoord7 != null) TextCoord7 = textCoord7;

            vertexAttributeToPrimvarMap = ComputeVertexAttributeToPrimvarMap(Color, TextCoord0, TextCoord1, TextCoord2, TextCoord3,
                TextCoord4, TextCoord5, TextCoord6, TextCoord7);
        }

        public static Dictionary<string, string> ComputeVertexAttributeToPrimvarMap(string[] colors, string[] textCoord0, string[] textCoord1,
            string[] textCoord2, string[] textCoord3, string[] textCoord4, string[] textCoord5, string[] textCoord6,
            string[] textCoord7)
        {
            var vertexAttributeToPrimvar = new Dictionary<string, string>();
            foreach (var color in colors)
            {
                vertexAttributeToPrimvar.Add(color, "Color");
            }
            foreach (var uv0 in textCoord0)
            {
                vertexAttributeToPrimvar.Add(uv0, "TextCoord0");
            }
            foreach (var uv1 in textCoord1)
            {
                vertexAttributeToPrimvar.Add(uv1, "TextCoord1");
            }
            foreach (var uv2 in textCoord2)
            {
                vertexAttributeToPrimvar.Add(uv2, "TextCoord2");
            }
            foreach (var uv3 in textCoord3)
            {
                vertexAttributeToPrimvar.Add(uv3, "TextCoord3");
            }
            foreach (var uv4 in textCoord4)
            {
                vertexAttributeToPrimvar.Add(uv4, "TextCoord4");
            }
            foreach (var uv5 in textCoord5)
            {
                vertexAttributeToPrimvar.Add(uv5, "TextCoord5");
            }
            foreach (var uv6 in textCoord6)
            {
                vertexAttributeToPrimvar.Add(uv6, "TextCoord6");
            }
            foreach (var uv7 in textCoord7)
            {
                vertexAttributeToPrimvar.Add(uv7, "TextCoord7");
            }

            return vertexAttributeToPrimvar;
        }

        public void OnAfterDeserialize()
        {
            vertexAttributeToPrimvarMap = ComputeVertexAttributeToPrimvarMap(Color, TextCoord0, TextCoord1, TextCoord2, TextCoord3,
                TextCoord4, TextCoord5, TextCoord6, TextCoord7);
        }

        public void OnBeforeSerialize()
        {
        }
    }
}
