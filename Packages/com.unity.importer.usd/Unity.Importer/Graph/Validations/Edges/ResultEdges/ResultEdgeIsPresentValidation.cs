namespace UnityEngine.Importer.Validations
{
    internal class ResultEdgeIsPresentValidation : IRemoveGraphComponentValidation<ResultEdge>, INewGraphComponentValidation<ResultEdge>
    {
        private const string k_ErrorMessage = "The result edge doesn't exist in the graph.";

        public void ValidateComponentRemoval(ResultEdge edge, GraphValidationContext context)
        {
            ValidateEdge(edge, context);
        }

        public void ValidateNewComponent(ResultEdge edge, GraphValidationContext context)
        {
            ValidateEdge(edge, context);
        }

        void ValidateEdge(ResultEdge edge, GraphValidationContext context)
        {
            if (!context.UniqueResultEdges.Contains(edge))
            {
                context.Errors.Add(new ResultEdgeValidationError(edge, k_ErrorMessage, typeof(ResultEdgeIsPresentValidation)));
            }
        }
    }
}
