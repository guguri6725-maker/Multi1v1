using System;
using UnityEngine.Importer;

namespace UnityEditor.Importer.NodeDeclaration.Validation
{
    internal class MissingAttributeValidation : BaseNodeDeclarationValidation
    {
        private readonly string k_ErrorMessage = $"Node must be decorated with a '{nameof(NodeMetadataAttribute)}' with a unique combination of '{nameof(NodeMetadataAttribute.nodeId)}' and '{nameof(NodeMetadataAttribute.version)}'. More info at INSERT DOCUMENTATION LINK";

        internal override void Validate(Type typeToValidate, NodeDeclarationValidationContext context)
        {
            if (context.NodeTypeToAttributes[typeToValidate] == null && !typeToValidate.IsGenericType)
            {
                var error = new NodeDeclarationError(k_ErrorMessage, typeToValidate);
                context.Errors.Add(error);
                context.ValidTypes.Remove(typeToValidate);
            }
        }
    }
}
