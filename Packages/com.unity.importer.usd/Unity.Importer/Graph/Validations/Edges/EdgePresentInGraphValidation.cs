namespace UnityEngine.Importer.Validations
{
    internal class EdgePresentInGraphValidation : IRemoveGraphComponentValidation<Edge>
    {
        private const string k_ErrorMessage = "Edge not found in the graph.";

        public void ValidateComponentRemoval(Edge edge, GraphValidationContext context)
        {
            foreach (var graphEdge in context.graph.Edges)
            {
                if (graphEdge.Equals(edge))
                {
                    return;
                }
            }
            context.Errors.Add(new EdgeValidationError(edge, k_ErrorMessage, typeof(EdgePresentInGraphValidation)));
        }
    }
}
