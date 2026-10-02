using System.Collections.Generic;
using System.Text;

namespace UnityEngine.Importer.Validations
{
    internal class SingleIncomingEdgeForInputPortValidation : IGraphValidation
    {
        public void Validate(GraphValidationContext context)
        {
            var collisions = GetCollisions(context);
            var errorBuilder = new StringBuilder();
            foreach (var collision in collisions)
            {
                if (collision.Value.Count <= 1)
                    continue;

                errorBuilder.Clear();
                errorBuilder.Append($"The Node field {collision.Key.Node}.{collision.Key.FieldName} has multiple incoming edge. Conflicting edges :");
                foreach (var edge in collision.Value)
                {
                    errorBuilder.Append($"\n    {edge}");
                }
                context.Errors.Add(new ConflictingComponentsValidationError<IDestinationEdge>(new List<IDestinationEdge>(collision.Value), errorBuilder.ToString(), typeof(SingleIncomingEdgeForInputPortValidation)));
            }
        }

        private Dictionary<NodePort, HashSet<IDestinationEdge>> GetCollisions(GraphValidationContext context)
        {
            var edges = new List<IDestinationEdge>();
            foreach (var uniqueEdge in context.UniqueEdges)
            {
                if (uniqueEdge.Destination.Node != null)
                    edges.Add(uniqueEdge);
            }
            foreach (var uniqueEdge in context.UniqueSettingEdges)
            {
                if (uniqueEdge.Destination.Node != null)
                    edges.Add(uniqueEdge);
            }
            foreach (var uniqueConstant in context.UniqueConstants)
            {
                if (uniqueConstant != null && uniqueConstant.Destination.Node != null)
                    edges.Add(uniqueConstant);
            }

            var collisions = new Dictionary<NodePort, HashSet<IDestinationEdge>>();
            foreach (var edge in edges)
            {
                var key = edge.Destination;
                if (!collisions.ContainsKey(key))
                {
                    collisions[key] = new HashSet<IDestinationEdge>();
                }

                collisions[key].Add(edge);
            }

            return collisions;
        }
    }
}
