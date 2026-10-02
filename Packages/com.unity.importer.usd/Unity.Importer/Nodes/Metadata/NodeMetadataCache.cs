using System;
using System.Collections.Generic;

namespace UnityEngine.Importer
{
    /// <summary>
    /// Metadata Cache of Nodes that passed all validations.
    /// </summary>
    public static class NodeMetadataCache
    {
        /// <summary>
        /// Metadata Cache of Nodes that passed all validations.
        /// </summary>
        public static IReadOnlyDictionary<Type, NodeMetadataAttribute> Cache { get; internal set; }

        /// <summary>
        /// Is the current <see cref="NodeMetadataCache"/> valid?
        /// </summary>
        public static bool IsCacheValid => Cache is { Count : > 0 };
    }
}
