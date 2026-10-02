namespace UnityEngine.Importer.Validations
{
    internal class SettingEdgeConnectsToExistingNodeValidation : IGraphValidation, INewGraphComponentValidation<SettingEdge>
    {
        private const string k_ErrorMessageDestination = "Destination node could not be found in the graph.";
        private const string k_ErrorMessageNullDestination = "Destination node can't be null.";

        public void Validate(GraphValidationContext context)
        {
            foreach (var edge in context.UniqueSettingEdges)
            {
                if (edge.Destination.Node != null && !context.UniqueNodes.Contains(edge.Destination.Node))
                {
                    context.Errors.Add(new SettingEdgeValidationError(edge, k_ErrorMessageDestination, typeof(SettingEdgeConnectsToExistingNodeValidation)));
                }
            }
        }

        public void ValidateNewComponent(SettingEdge edge, GraphValidationContext context)
        {
            if (edge.Destination.Node == null)
            {
                context.Errors.Add(new SettingEdgeValidationError(edge, k_ErrorMessageNullDestination, typeof(SettingEdgeConnectsToExistingNodeValidation)));
            }
            else if (!context.UniqueNodes.Contains(edge.Destination.Node))
            {
                context.Errors.Add(new SettingEdgeValidationError(edge, k_ErrorMessageDestination, typeof(SettingEdgeConnectsToExistingNodeValidation)));
            }
        }
    }
}
