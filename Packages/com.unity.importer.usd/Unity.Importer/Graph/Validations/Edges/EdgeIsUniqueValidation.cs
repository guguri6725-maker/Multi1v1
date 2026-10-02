using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class EdgeIsUniqueValidation : IGraphValidation, INewGraphComponentValidation<Edge>
    {
        private const string k_ErrorMessage = "An identical edge is already existing in the graph.";

        public void Validate(GraphValidationContext context)
        {
            var edgeOccurrences = new HashSet<Edge>(context.graph.Edges.Count);
            foreach (var edge in context.graph.Edges)
            {
                if (edge.Origin.Node == null || edge.Destination.Node == null)
                    continue;

                if (!edgeOccurrences.Add(edge))
                {
                    context.Errors.Add(new EdgeValidationError(edge, k_ErrorMessage, typeof(EdgeIsUniqueValidation)));
                }
            }
        }

        public void ValidateNewComponent(Edge edge, GraphValidationContext context)
        {
            if (edge.Origin.Node == null || edge.Destination.Node == null)
                return;

            if (context.UniqueEdges.Contains(edge))
            {
                context.Errors.Add(new EdgeValidationError(edge, k_ErrorMessage, typeof(EdgeIsUniqueValidation)));
            }
        }
    }
}
