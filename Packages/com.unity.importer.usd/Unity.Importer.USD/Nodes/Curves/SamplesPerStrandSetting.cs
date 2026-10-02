using System;
using UnityEngine;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Containing struct for the SamplesPerStrand value.
    /// </summary>
    [Serializable]
    public struct SamplesPerStrandSetting
    {
        internal const int MIN_STRAND_PARTICLE_COUNT = 3;
        internal const int MAX_STRAND_PARTICLE_COUNT = 128;

        /// <summary>
        /// the value of this SamplesPerStrandSetting container.
        /// </summary>
        [Range(MIN_STRAND_PARTICLE_COUNT, MAX_STRAND_PARTICLE_COUNT)]
        public int value;
    }
}
