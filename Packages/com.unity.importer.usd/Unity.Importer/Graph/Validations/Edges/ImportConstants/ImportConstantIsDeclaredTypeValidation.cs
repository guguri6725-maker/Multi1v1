namespace UnityEngine.Importer.Validations
{
    internal class ImportConstantIsDeclaredTypeValidation : IGraphValidation, INewGraphComponentValidation<IConstantValue>
    {
        public void Validate(GraphValidationContext context)
        {
            foreach (var constant in context.graph.ImportConstants)
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
            if (constant == null || constant.Value == null)
                return;

            if (!constant.Type.IsInstanceOfType(constant.Value))
            {
                var message =
                    $"ImportConstant value is not of the right type (actual '{constant.Value.GetType()}', expected '{constant.Type}').";
                context.Errors.Add(new ImportConstantValidationError(constant, message,
                    typeof(ImportConstantIsDeclaredTypeValidation)));
            }
        }
    }
}
