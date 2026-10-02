using System.Collections.Generic;
using System.Text;

namespace UnityEngine.Importer.Validations
{
    internal class EdgeConnectedToFreePortValidation<TComponent> : INewGraphComponentValidation<TComponent>
        where TComponent : IDestinationEdge
    {
        public void ValidateNewComponent(TComponent edge, GraphValidationContext context)
        {
            if (edge == null)
                return;

            var conflicts = new List<IDestinationEdge>();

            var edges = new List<IDestinationEdge>();
            foreach (var uniqueEdge in context.UniqueEdges)
            {
                edges.Add(uniqueEdge);
            }
            foreach (var uniqueSettingEdge in context.UniqueSettingEdges)
            {
                edges.Add(uniqueSettingEdge);
            }
            foreach (var uniqueConstant in context.UniqueConstants)
            {
                edges.Add(uniqueConstant);
            }

            var errorBuilder = new StringBuilder("Nodes can only have one incoming edge per input. Conflicting edges :");

            foreach (var graphEdge in edges)
            {
                if (!graphEdge.Equals(edge) && graphEdge.Destination.Equals(edge.Destination))
                {
                    conflicts.Add(graphEdge);
                    errorBuilder.Append($"\n    {graphEdge}");
                }
            }

            if (conflicts.Count > 0)
            {
                context.Errors.Add(new ConflictingComponentsValidationError<IDestinationEdge>(new List<IDestinationEdge> { edge }, errorBuilder.ToString(), typeof(EdgeConnectedToFreePortValidation<TComponent>)));
            }
        }
    }
}
