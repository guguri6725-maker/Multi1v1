using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Importer;

namespace UnityEditor.Importer.NodeDeclaration.Validation
{
    internal class NoNodeCollisionValidation : BaseNodeDeclarationValidation
    {
        private const string k_ErrorMessageEnd = "\nAll declared nodes must have a unique combination of NodeId and Version.\nMore info at INSERT DOCUMENTATION LINK";
        private readonly StringBuilder errorBuilder = new();
        private IReadOnlyDictionary<string, List<Type>> collisionCache;

        internal override void Setup(NodeDeclarationValidationContext context)
        {
            var collisionTrackers = new Dictionary<string, List<Type>>();

            foreach (var typeToAttribute in context.NodeTypeToAttributes)
            {
                if (typeToAttribute.Value == null)
                    continue;

                var id = GetCollisionKey(typeToAttribute.Value);
                if (!collisionTrackers.ContainsKey(id))
                {
                    collisionTrackers.Add(id, new List<Type>());
                }

                collisionTrackers[id].Add(typeToAttribute.Key);
            }

            collisionCache = collisionTrackers;
        }

        private static string GetCollisionKey(NodeMetadataAttribute attribute) => $"{attribute.version} {attribute.nodeId}";

        internal override void Validate(Type typeToValidate, NodeDeclarationValidationContext context)
        {
            var attribute = context.NodeTypeToAttributes[typeToValidate];
            if (attribute == null)
                return;

            var id = GetCollisionKey(attribute);

            if (!collisionCache.ContainsKey(id) || collisionCache[id].Count == 1)
                return;

            errorBuilder.Clear();
            errorBuilder.Append($"Combination of NodeID '{attribute.nodeId}' and version '{attribute.version}' is not unique. Colliding with nodes :");
            foreach (var type in collisionCache[id].Where(type => type != typeToValidate))
            {
                errorBuilder.Append($"\n- {type.Name}");
            }
            errorBuilder.Append(k_ErrorMessageEnd);

            var message = errorBuilder.ToString();
            var error = new NodeDeclarationError(message, typeToValidate);
            context.Errors.Add(error);
            context.ValidTypes.Remove(typeToValidate);
        }
    }
}
