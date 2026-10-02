namespace UnityEngine.Importer.Validations
{
    internal class ImportConstantIsNotNullValidation : IRemoveGraphComponentValidation<IConstantValue>, INewGraphComponentValidation<IConstantValue>
    {
        private const string k_ErrorMessage = "ImportConstant is null.";

        public void ValidateNewComponent(IConstantValue constant, GraphValidationContext context)
        {
            ValidateConstant(constant, context);
        }

        public void ValidateComponentRemoval(IConstantValue constant, GraphValidationContext context)
        {
            ValidateConstant(constant, context);
        }

        private void ValidateConstant(IConstantValue constant, GraphValidationContext context)
        {
            if (constant == null)
            {
                var error = new ImportConstantValidationError(constant, k_ErrorMessage, typeof(ImportConstantIsNotNullValidation));
                context.Errors.Add(error);
            }
        }
    }
}
