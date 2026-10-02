using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class ImportConstantIsUniqueValidation : IGraphValidation, INewGraphComponentValidation<IConstantValue>
    {
        private const string k_ErrorMessage = "An identical import constant already exists in the graph.";

        public void Validate(GraphValidationContext context)
        {
            var constantOccurrences = new HashSet<IConstantValue>(context.graph.ImportConstants.Count);
            foreach (var constant in context.graph.ImportConstants)
            {
                if (constant == null || constant.Destination.Node == null)
                    continue;

                if (!constantOccurrences.Add(constant))
                {
                    context.Errors.Add(new ImportConstantValidationError(constant, k_ErrorMessage, typeof(ImportConstantIsUniqueValidation)));
                }
            }
        }

        public void ValidateNewComponent(IConstantValue constant, GraphValidationContext context)
        {
            if (constant == null)
                return;

            if (context.UniqueConstants.Contains(constant))
            {
                context.Errors.Add(new ImportConstantValidationError(constant, k_ErrorMessage, typeof(ImportConstantIsUniqueValidation)));
            }
        }
    }
}
