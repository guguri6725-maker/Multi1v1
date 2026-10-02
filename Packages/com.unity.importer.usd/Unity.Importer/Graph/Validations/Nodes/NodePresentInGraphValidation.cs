namespace UnityEngine.Importer.Validations
{
    internal class NodePresentInGraphValidation : IRemoveGraphComponentValidation<INode<InputPorts, OutputPorts>>
    {
        private const string k_ErrorMessage = "Node not found in the graph.";

        public void ValidateComponentRemoval(INode<InputPorts, OutputPorts> node, GraphValidationContext context)
        {
            if (node != null && !context.UniqueNodes.Contains(node))
            {
                var error = new NodeValidationError(node, k_ErrorMessage, typeof(NodePresentInGraphValidation));
                context.Errors.Add(error);
            }
        }
    }
}
