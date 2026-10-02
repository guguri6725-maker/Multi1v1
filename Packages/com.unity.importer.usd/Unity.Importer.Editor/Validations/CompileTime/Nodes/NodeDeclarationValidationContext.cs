using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Importer;

namespace UnityEditor.Importer.NodeDeclaration.Validation
{
    internal class NodeDeclarationValidationContext
    {
        public IReadOnlyDictionary<Type, NodeMetadataAttribute> NodeTypeToAttributes { get; }

        public HashSet<Type> ValidTypes { get; }

        public List<NodeDeclarationError> Errors { get; } = new();

        public NodeDeclarationValidationContext(List<Type> nodeTypes)
        {
            var attributes = new Dictionary<Type, NodeMetadataAttribute>(nodeTypes.Count);
            foreach (var type in nodeTypes)
            {
                var attribute = type.GetCustomAttributes(false).FirstOrDefault(a => a is NodeMetadataAttribute);
                attributes.Add(type, (NodeMetadataAttribute)attribute);
            }

            NodeTypeToAttributes = attributes;
            ValidTypes = new HashSet<Type>(nodeTypes);
        }
    }
}
