namespace UnityEngine.Importer.Validations
{
    internal class ResultEdgeHasUniqueIdValidation : INewGraphComponentValidation<ResultEdge>
    {
        private const string k_ErrorMessage = "A result edge with the same id already exists in the graph: {0}";

        public void ValidateNewComponent(ResultEdge edge, GraphValidationContext context)
        {
            foreach (var e in context.UniqueResultEdges)
            {
                if (e.Id == edge.Id && !e.Origin.Equals(edge.Origin))
                {
                    context.Errors.Add(new ResultEdgeValidationError(edge, string.Format(k_ErrorMessage, e), typeof(ResultEdgeHasUniqueIdValidation)));
                    return;
                }
            }
        }
    }
}
