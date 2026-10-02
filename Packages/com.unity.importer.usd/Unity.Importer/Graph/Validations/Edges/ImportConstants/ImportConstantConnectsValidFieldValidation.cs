using System.Reflection;

namespace UnityEngine.Importer.Validations
{
    internal class ImportConstantConnectsValidFieldValidation : IGraphValidation, INewGraphComponentValidation<IConstantValue>
    {
        public void Validate(GraphValidationContext context)
        {
            foreach (var constant in context.UniqueConstants)
            {
                ValidateConstant(constant, context);
            }
        }

        public void ValidateNewComponent(IConstantValue constant, GraphValidationContext context)
        {
            ValidateConstant(constant, context);
        }

        private void ValidateConstant(IConstantValue constant, GraphValidationContext context)
        {
            if (constant == null)
                return;

            FieldInfo destinationField = null;
            if (constant.Destination.Node != null)
            {
                destinationField = constant.Destination.ToFieldInfo();
                if (destinationField == null)
                {
                    context.Errors.Add(new ImportConstantValidationError(constant,
                        $"Could not find Input field '{constant.Destination.FieldName}' on InputPorts type '{constant.Destination.Node}'.",
                        typeof(ImportConstantConnectsValidFieldValidation)));
                }
            }

            if (destinationField != null && !destinationField.FieldType.IsAssignableFrom(constant.Type))
            {
                var message = $"Types of import constant and destination field are different. ImportConstant : '{constant.Type.Name}' Destination : '{destinationField.FieldType.Name}'";
                context.Errors.Add(new ImportConstantValidationError(constant, message, typeof(ImportConstantConnectsValidFieldValidation)));
            }
        }
    }
}
