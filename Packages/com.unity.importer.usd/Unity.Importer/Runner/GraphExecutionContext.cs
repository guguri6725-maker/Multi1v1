using System.Collections.Generic;

namespace UnityEngine.Importer
{
    internal class GraphExecutionContext
    {
        public readonly Dictionary<INode<InputPorts, OutputPorts>, List<Edge>> outgoingEdgeDestinations = new();
        public readonly Dictionary<INode<InputPorts, OutputPorts>, int> incomingEdgeCount = new();
        public readonly Stack<INode<InputPorts, OutputPorts>> candidates;

        public GraphExecutionContext(ImporterGraph graph)
        {
            var notACandidate = new HashSet<INode<InputPorts, OutputPorts>>();
            foreach (var edge in graph.Edges)
            {
                var originNode = edge.Origin.Node;
                var destinationNode = edge.Destination.Node;

                if (originNode == null || destinationNode == null)
                {
                    if (destinationNode != null)
                    {
                        notACandidate.Add(destinationNode);
                    }
                    continue;
                }

                if (outgoingEdgeDestinations.TryGetValue(originNode, out var entry))
                {
                    entry.Add(edge);
                }
                else
                {
                    outgoingEdgeDestinations.Add(originNode, new List<Edge> { edge });
                }

                if (incomingEdgeCount.ContainsKey(destinationNode))
                {
                    incomingEdgeCount[destinationNode]++;
                }
                else
                {
                    incomingEdgeCount.Add(destinationNode, 1);
                }
            }

            candidates = new Stack<INode<InputPorts, OutputPorts>>();
            foreach (var node in graph.Nodes)
            {
                if (node != null && !notACandidate.Contains(node) && !incomingEdgeCount.ContainsKey(node))
                {
                    candidates.Push(node);
                }
            }
        }
    }
}
