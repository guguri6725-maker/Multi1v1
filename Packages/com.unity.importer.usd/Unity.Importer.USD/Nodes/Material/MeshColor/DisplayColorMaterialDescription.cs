using System;
using UnityEngine;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Describes the parameters of a specific material, created from displayColor/displayOpacity data.
    /// </summary>
    public struct DisplayColorMaterialDescription : IEquatable<DisplayColorMaterialDescription>
    {
        /// <summary>
        /// The name of the material to create.
        /// </summary>
        public string name;

        /// <summary>
        /// The opacity of the material to create.
        /// </summary>
        public float opacity;

        /// <summary>
        /// The diffuse color of the material to create.
        /// </summary>
        public Color color;

        /// <summary>
        /// See documentation for &lt;IEquatable.Equals&gt; for more details
        /// </summary>
        public bool Equals(DisplayColorMaterialDescription other)
        {
            return name == other.name && opacity.Equals(other.opacity) && color.Equals(other.color);
        }

        /// <summary>
        /// See documentation for &lt;Object.GetHashCode&gt; for more details
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(name, opacity, color);
        }
    }
}
