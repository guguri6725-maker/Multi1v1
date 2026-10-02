using System;

namespace UnityEditor.Importer.NodeDeclaration.Validation
{
    internal abstract class BaseNodeDeclarationValidation
    {
        internal virtual void Setup(NodeDeclarationValidationContext context) {}

        internal abstract void Validate(Type typeToValidate, NodeDeclarationValidationContext context);
    }
}
