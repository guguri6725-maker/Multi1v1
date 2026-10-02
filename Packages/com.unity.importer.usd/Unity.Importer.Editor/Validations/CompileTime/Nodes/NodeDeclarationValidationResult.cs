using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Importer;

namespace UnityEditor.Importer.NodeDeclaration.Validation
{
    internal class NodeDeclarationValidationResult
    {
        public IReadOnlyDictionary<Type, NodeMetadataAttribute> NodeMetadata { get; }

        public IReadOnlyCollection<NodeDeclarationError> Errors { get; }

        public NodeDeclarationValidationResult(NodeDeclarationValidationContext context)
        {
            Errors = context.Errors;
            NodeMetadata = context.ValidTypes.ToDictionary(t => t, t => context.NodeTypeToAttributes[t]);
        }

        public void DisplayErrors()
        {
            foreach (var error in Errors)
            {
                Debug.LogError(error);
            }
        }
    }
}
