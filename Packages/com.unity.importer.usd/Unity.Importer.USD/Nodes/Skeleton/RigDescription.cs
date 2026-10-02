using System.Collections.Generic;
using Unity.Mathematics;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Represents a skeleton transform data
    /// </summary>
    public struct RigDescription
    {
        /// <summary>
        /// The transform matrices of this skeleton joints, used to create the hierarchy Transform and initialise them.
        /// </summary>
        public Dictionary<string, float4x4> jointTransformMatrix;
    }
}
