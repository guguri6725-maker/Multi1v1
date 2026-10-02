namespace UnityEngine.Importer.Validations
{
    internal class ImportConstantPresentInGraphValidation : IRemoveGraphComponentValidation<IConstantValue>
    {
        private const string k_ErrorMessage = "This import constant is not part of the Graph.";

        public void ValidateComponentRemoval(IConstantValue constant, GraphValidationContext context)
        {
            if (constant != null && !context.UniqueConstants.Contains(constant))
            {
                context.Errors.Add(new ImportConstantValidationError(constant, k_ErrorMessage, typeof(ImportConstantPresentInGraphValidation)));
            }
        }
    }
}
