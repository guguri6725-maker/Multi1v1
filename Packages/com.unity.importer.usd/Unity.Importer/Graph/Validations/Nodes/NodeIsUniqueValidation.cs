using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class NodeIsUniqueValidation : IGraphValidation, INewGraphComponentValidation<INode<InputPorts, OutputPorts>>
    {
        private const string k_ErrorMessage = "An identical node is already existing in the graph.";

        public void Validate(GraphValidationContext context)
        {
            var nodeOccurrences = new HashSet<INode<InputPorts, OutputPorts>>(context.graph.Nodes.Count);
            foreach (var node in context.graph.Nodes)
            {
                if (node == null)
                    continue;

                if (!nodeOccurrences.Add(node))
                {
                    context.Errors.Add(new NodeValidationError(node, k_ErrorMessage, typeof(NodeIsUniqueValidation)));
                }
            }
        }

        public void ValidateNewComponent(INode<InputPorts, OutputPorts> node, GraphValidationContext context)
        {
            if (node == null)
                return;

            if (context.UniqueNodes.Contains(node))
            {
                context.Errors.Add(new NodeValidationError(node, k_ErrorMessage, typeof(NodeIsUniqueValidation)));
            }
        }
    }
}
