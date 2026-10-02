using System;

namespace UnityEngine.Importer
{
    /// <summary>
    /// Decorate your nodes with this attribute to specify the metadata of your node.
    /// </summary>
    /// <remarks>
    /// <see cref="nodeId"/> and <see cref="version"/> must always be specified and their combination need to be unique across all nodes.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NodeMetadataAttribute : Attribute
    {
        /// <summary>
        /// Node ID used to regroup node of different types.
        /// </summary>
        /// <remarks>
        /// When a new version of a node is required (breaking changes, retro-compatibility), users will create a new type.
        /// For more clarity, the editor's node browser will display them grouped by their ID and ordered by their version.
        /// </remarks>
        public string nodeId;

        /// <summary>
        /// Version used to order nodes with the same ID.
        /// </summary>
        /// <remarks>
        /// When a new version of a node is required (breaking changes, retro-compatibility), users will create a new type.
        /// For more clarity, the editor's node browser will display them grouped by their ID and ordered by their version.
        /// </remarks>
        public int version;

        /// <summary>
        /// Display name of the node, displayed in the graph editor.
        /// </summary>
        public string displayName;

        /// <summary>
        /// Description of the node, displayed in the graph editor.
        /// </summary>
        public string description;

        /// <summary>
        /// <inheritdoc cref="NodeMetadataAttribute"/>
        /// </summary>
        /// <param name="_nodeId">Node ID used to regroup node of different types.</param>
        /// <param name="_version">Version used to order nodes with the same ID.</param>
        public NodeMetadataAttribute(string _nodeId, int _version)
        {
            nodeId = _nodeId;
            version = _version;
        }
    }
}
