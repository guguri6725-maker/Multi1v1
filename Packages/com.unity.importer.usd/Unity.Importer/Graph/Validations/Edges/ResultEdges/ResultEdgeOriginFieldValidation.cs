namespace UnityEngine.Importer.Validations
{
    internal class ResultEdgeOriginFieldValidation : IGraphValidation, INewGraphComponentValidation<ResultEdge>
    {
        public void ValidateNewComponent(ResultEdge edge, GraphValidationContext context)
        {
            ValidateEdge(edge, context);
        }

        public void Validate(GraphValidationContext context)
        {
            foreach (var edge in context.graph.ResultEdges)
            {
                ValidateEdge(edge, context);
            }
        }

        private void ValidateEdge(ResultEdge edge, GraphValidationContext context)
        {
            if (edge.Origin.Node == null)
                return;

            if (edge.Origin.ToFieldInfo() == null)
            {
                context.Errors.Add(new ResultEdgeValidationError(edge,
                    $"Could not find Output field '{edge.Origin.FieldName}' on OutputPorts type '{edge.Origin.Node}'.",
                    typeof(ResultEdgeOriginFieldValidation)));
            }
        }
    }
}
