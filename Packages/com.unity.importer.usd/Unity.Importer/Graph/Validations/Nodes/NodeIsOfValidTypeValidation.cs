namespace UnityEngine.Importer.Validations
{
    internal class NodeIsOfValidTypeValidation : IGraphValidation, INewGraphComponentValidation<INode<InputPorts, OutputPorts>>
    {
        private const string k_ErrorMessage = "Node is not valid. Check for node declaration errors after a recompilation.";

        public void Validate(GraphValidationContext context)
        {
            foreach (var node in context.graph.Nodes)
            {
                ValidateNode(node, context);
            }
        }

        public void ValidateNewComponent(INode<InputPorts, OutputPorts> node, GraphValidationContext context)
        {
            ValidateNode(node, context);
        }

        private void ValidateNode(INode<InputPorts, OutputPorts> node, GraphValidationContext context)
        {
            if (node == null)
                return;

            var nodeType = node.GetType();
            if (NodeMetadataCache.IsCacheValid && !NodeMetadataCache.Cache.ContainsKey(nodeType)
                && (!nodeType.IsGenericType || !NodeMetadataCache.Cache.ContainsKey(nodeType.GetGenericTypeDefinition())))
            {
                var error = new NodeValidationError(node, k_ErrorMessage, typeof(NodeIsOfValidTypeValidation));
                context.Errors.Add(error);
            }
        }
    }
}
