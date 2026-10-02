namespace UnityEngine.Importer.Validations
{
    internal class SettingEdgePresentInGraphValidation : IRemoveGraphComponentValidation<SettingEdge>
    {
        private const string k_ErrorMessage = "Setting edge not found in the graph.";

        public void ValidateComponentRemoval(SettingEdge edge, GraphValidationContext context)
        {
            if (!context.UniqueSettingEdges.Contains(edge))
            {
                var error = new SettingEdgeValidationError(edge, k_ErrorMessage, typeof(SettingEdgePresentInGraphValidation));
                context.Errors.Add(error);
            }
        }
    }
}
