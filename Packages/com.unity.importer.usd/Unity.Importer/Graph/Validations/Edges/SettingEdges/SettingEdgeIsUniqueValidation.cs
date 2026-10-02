using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class SettingEdgeIsUniqueValidation : IGraphValidation, INewGraphComponentValidation<SettingEdge>
    {
        private const string k_ErrorMessage = "An identical setting edge is already existing in the graph.";

        public void Validate(GraphValidationContext context)
        {
            var edgeOccurrences = new HashSet<SettingEdge>(context.graph.Edges.Count);
            foreach (var edge in context.graph.SettingEdges)
            {
                if (edge.Destination.Node == null)
                    continue;

                if (!edgeOccurrences.Add(edge))
                {
                    context.Errors.Add(new SettingEdgeValidationError(edge, k_ErrorMessage, typeof(SettingEdgeIsUniqueValidation)));
                }
            }
        }

        public void ValidateNewComponent(SettingEdge edge, GraphValidationContext context)
        {
            if (edge.Destination.Node == null)
                return;

            if (context.UniqueSettingEdges.Contains(edge))
            {
                context.Errors.Add(new SettingEdgeValidationError(edge, k_ErrorMessage, typeof(SettingEdgeIsUniqueValidation)));
            }
        }
    }
}
