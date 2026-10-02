namespace UnityEngine.Importer.Validations
{
    internal class ImportConstantConnectsExistingNodeValidation : IGraphValidation, INewGraphComponentValidation<IConstantValue>
    {
        private const string k_ErrorMessageDestination = "Destination node could not be found in the graph.";
        private const string k_ErrorMessageNullDestination = "Destination node can't be null.";

        public void Validate(GraphValidationContext context)
        {
            foreach (var constant in context.UniqueConstants)
            {
                if (constant == null)
                    continue;

                if (constant.Destination.Node != null && !context.UniqueNodes.Contains(constant.Destination.Node))
                {
                    context.Errors.Add(new ImportConstantValidationError(constant, k_ErrorMessageDestination, typeof(ImportConstantConnectsExistingNodeValidation)));
                }
            }
        }

        public void ValidateNewComponent(IConstantValue constant, GraphValidationContext context)
        {
            if (constant == null)
                return;

            if (constant.Destination.Node == null)
            {
                context.Errors.Add(new ImportConstantValidationError(constant, k_ErrorMessageNullDestination, typeof(ImportConstantConnectsExistingNodeValidation)));
            }
            else if (!context.UniqueNodes.Contains(constant.Destination.Node))
            {
                context.Errors.Add(new ImportConstantValidationError(constant, k_ErrorMessageDestination, typeof(ImportConstantConnectsExistingNodeValidation)));
            }
        }
    }
}
