using System.Collections.Generic;
using System.Text;

namespace UnityEngine.Importer.Validations
{
    internal class NodesHaveUniqueResultEdgeIdValidation : IGraphValidation
    {
        public void Validate(GraphValidationContext context)
        {
            var edges = context.UniqueResultEdges;
            var allEdges = new Dictionary<string, List<ResultEdge>>(edges.Count);
            foreach (var edge in edges)
            {
                if (edge.Origin.Node == null)
                    continue;

                if (!allEdges.ContainsKey(edge.Id))
                    allEdges.Add(edge.Id, new List<ResultEdge>());
                allEdges[edge.Id].Add(edge);
            }

            var sb = new StringBuilder();
            foreach (var edge in allEdges)
            {
                if (edge.Value.Count > 1)
                {
                    sb.Clear();
                    sb.AppendLine("The following nodes connect to the same result id:");
                    foreach (var node in edge.Value)
                    {
                        sb.AppendLine($"    {node}.Output:{node.Origin}");
                    }
                    context.Errors.Add(new ResultValidationError(edge.Key, edge.Value,
                        sb.ToString().TrimEnd(),
                        typeof(NodesHaveUniqueResultEdgeIdValidation)));
                }
            }
        }
    }
}
