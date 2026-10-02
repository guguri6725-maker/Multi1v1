using System;
using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class EdgeNotCreatingCycleValidation : INewGraphComponentValidation<Edge>
    {
        private const string k_ErrorMessageAdd = "Adding this edge would create a cycle in the graph.";

        public void ValidateNewComponent(Edge edge, GraphValidationContext context)
        {
            if (edge.Origin.Node == null || edge.Destination.Node == null)
                return;

            var nodeExplored = new HashSet<INode<InputPorts, OutputPorts>>(context.graph.Nodes.Count);
            var nodesToExplore = new List<INode<InputPorts, OutputPorts>>(context.graph.Nodes.Count) { edge.Destination.Node };
            var nodeNeighbours = ComputeNodeNeighbours(context.graph);
            INode<InputPorts, OutputPorts> currentNode;
            List<INode<InputPorts, OutputPorts>> neighbours;

            while (nodesToExplore.Count > 0)
            {
                var nodeIndex = nodesToExplore.Count - 1;
                currentNode = nodesToExplore[nodeIndex];

                //If we come back to our edge, we detected a cycle
                if (currentNode == edge.Origin.Node)
                {
                    context.Errors.Add(new EdgeValidationError(edge, k_ErrorMessageAdd, typeof(EdgeNotCreatingCycleValidation)));
                    return;
                }

                nodesToExplore.RemoveAt(nodeIndex);
                nodeExplored.Add(currentNode);

                if (nodeNeighbours.TryGetValue(currentNode, out neighbours))
                {
                    foreach (var neighbour in neighbours)
                    {
                        //We're ignoring node already explored. A node already explored means we found an unrelated existing cycle
                        if (nodeExplored.Contains(neighbour))
                            continue;

                        nodesToExplore.Add(neighbour);
                    }
                }
            }
        }

        private static Dictionary<INode<InputPorts, OutputPorts>, List<INode<InputPorts, OutputPorts>>> ComputeNodeNeighbours(ImporterGraph graph,
            Action<INode<InputPorts, OutputPorts>> onNeighbourDiscovered = null)
        {
            var nodeNeighbours = new Dictionary<INode<InputPorts, OutputPorts>, List<INode<InputPorts, OutputPorts>>>();
            foreach (var e in graph.Edges)
            {
                var node = e.Origin.Node;
                if (node == null || e.Destination.Node == null)
                    continue;

                if (nodeNeighbours.TryGetValue(node, out var nodes))
                {
                    nodes.Add(e.Destination.Node);
                }
                else
                {
                    nodeNeighbours.Add(node, new List<INode<InputPorts, OutputPorts>> { e.Destination.Node });
                }
                onNeighbourDiscovered?.Invoke(e.Destination.Node);
            }

            return nodeNeighbours;
        }
    }
}
