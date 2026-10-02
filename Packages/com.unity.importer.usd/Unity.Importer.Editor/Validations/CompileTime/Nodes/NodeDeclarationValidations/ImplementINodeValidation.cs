using System;
using System.Linq;
using UnityEngine.Importer;

namespace UnityEditor.Importer.NodeDeclaration.Validation
{
    internal class ImplementINodeValidation : BaseNodeDeclarationValidation
    {
        private readonly string k_ErrorMessage = "Node must inherit from 'Node<TInput, TOutput>' or implement 'INode<out TInput, out TOutput>'.";

        internal override void Validate(Type typeToValidate, NodeDeclarationValidationContext context)
        {
            if (typeToValidate.GetInterfaces().All(i => !i.IsGenericType || i.GetGenericTypeDefinition() != typeof(INode<,>)))
            {
                var error = new NodeDeclarationError(k_ErrorMessage, typeToValidate);
                context.Errors.Add(error);
                context.ValidTypes.Remove(typeToValidate);
            }
        }
    }
}
