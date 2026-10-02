namespace UnityEngine.Importer.Validations
{
    internal class ResultEdgeExistingOriginValidation : IGraphValidation, INewGraphComponentValidation<ResultEdge>
    {
        private const string k_ErrorMessage = "The origin of the result edge is not a node in the graph.";
        private const string k_ErrorMessageNullOrigin = "Origin node can't be null.";

        public void ValidateNewComponent(ResultEdge edge, GraphValidationContext context)
        {
            if (edge.Origin.Node == null)
            {
                context.Errors.Add(new ResultEdgeValidationError(edge, k_ErrorMessageNullOrigin, typeof(ResultEdgeExistingOriginValidation)));
            }
            else if (!context.UniqueNodes.Contains(edge.Origin.Node))
            {
                context.Errors.Add(new ResultEdgeValidationError(edge, k_ErrorMessage, typeof(ResultEdgeExistingOriginValidation)));
            }
        }

        public void Validate(GraphValidationContext context)
        {
            foreach (var edge in context.UniqueResultEdges)
            {
                if (edge.Origin.Node != null && !context.UniqueNodes.Contains(edge.Origin.Node))
                {
                    context.Errors.Add(new ResultEdgeValidationError(edge, k_ErrorMessage, typeof(ResultEdgeExistingOriginValidation)));
                }
            }
        }
    }
}
