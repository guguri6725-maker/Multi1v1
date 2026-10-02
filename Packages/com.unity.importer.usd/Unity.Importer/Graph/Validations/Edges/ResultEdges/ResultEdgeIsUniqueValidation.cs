using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class ResultEdgeIsUniqueValidation : IGraphValidation, INewGraphComponentValidation<ResultEdge>
    {
        private const string k_ErrorMessage = "An identical result edge is already existing in the graph.";

        public void Validate(GraphValidationContext context)
        {
            var edgeOccurrences = new HashSet<ResultEdge>(context.graph.Edges.Count);
            foreach (var edge in context.graph.ResultEdges)
            {
                if (edge.Origin.Node == null)
                    continue;

                if (!edgeOccurrences.Add(edge))
                {
                    context.Errors.Add(new ResultEdgeValidationError(edge, k_ErrorMessage, typeof(ResultEdgeIsUniqueValidation)));
                }
            }
        }

        public void ValidateNewComponent(ResultEdge edge, GraphValidationContext context)
        {
            if (edge.Origin.Node == null)
                return;

            if (context.UniqueResultEdges.Contains(edge))
            {
                context.Errors.Add(new ResultEdgeValidationError(edge, k_ErrorMessage, typeof(ResultEdgeIsUniqueValidation)));
            }
        }
    }
}
