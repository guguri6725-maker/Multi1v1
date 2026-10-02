namespace UnityEngine.Importer.Validations
{
    internal class NodeIsNotNullValidation : INewGraphComponentValidation<INode<InputPorts, OutputPorts>>, IRemoveGraphComponentValidation<INode<InputPorts, OutputPorts>>
    {
        private const string k_ErrorMessage = "Node is null. Ensure that you are not missing a node type.";

        public void ValidateNewComponent(INode<InputPorts, OutputPorts> node, GraphValidationContext context)
        {
            ValidateNode(node, context);
        }

        public void ValidateComponentRemoval(INode<InputPorts, OutputPorts> node, GraphValidationContext context)
        {
            ValidateNode(node, context);
        }

        private void ValidateNode(INode<InputPorts, OutputPorts> node, GraphValidationContext context)
        {
            if (node == null)
            {
                var error = new NodeValidationError(node, k_ErrorMessage, typeof(NodeIsNotNullValidation));
                context.Errors.Add(error);
            }
        }
    }
}
