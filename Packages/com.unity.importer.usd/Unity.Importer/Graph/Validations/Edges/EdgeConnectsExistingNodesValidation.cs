namespace UnityEngine.Importer.Validations
{
    internal class EdgeConnectsExistingNodesValidation : IGraphValidation, INewGraphComponentValidation<Edge>
    {
        private const string k_ErrorMessageOrigin = "Origin node could not be found in the graph.";
        private const string k_ErrorMessageNullOrigin = "Origin node can't be null.";
        private const string k_ErrorMessageDestination = "Destination node could not be found in the graph.";
        private const string k_ErrorMessageNullDestination = "Destination node can't be null.";

        public void Validate(GraphValidationContext context)
        {
            foreach (var edge in context.UniqueEdges)
            {
                if (edge.Origin.Node != null && !context.UniqueNodes.Contains(edge.Origin.Node))
                {
                    context.Errors.Add(new EdgeValidationError(edge, k_ErrorMessageOrigin, typeof(EdgeConnectsExistingNodesValidation)));
                }

                if (edge.Destination.Node != null && !context.UniqueNodes.Contains(edge.Destination.Node))
                {
                    context.Errors.Add(new EdgeValidationError(edge, k_ErrorMessageDestination, typeof(EdgeConnectsExistingNodesValidation)));
                }
            }
        }

        public void ValidateNewComponent(Edge edge, GraphValidationContext context)
        {
            if (edge.Origin.Node == null)
            {
                context.Errors.Add(new EdgeValidationError(edge, k_ErrorMessageNullOrigin, typeof(EdgeConnectsExistingNodesValidation)));
            }
            else if (!context.UniqueNodes.Contains(edge.Origin.Node))
            {
                context.Errors.Add(new EdgeValidationError(edge, k_ErrorMessageOrigin, typeof(EdgeConnectsExistingNodesValidation)));
            }

            if (edge.Destination.Node == null)
            {
                context.Errors.Add(new EdgeValidationError(edge, k_ErrorMessageNullDestination, typeof(EdgeConnectsExistingNodesValidation)));
            }
            else if (!context.UniqueNodes.Contains(edge.Destination.Node))
            {
                context.Errors.Add(new EdgeValidationError(edge, k_ErrorMessageDestination, typeof(EdgeConnectsExistingNodesValidation)));
            }
        }
    }
}
