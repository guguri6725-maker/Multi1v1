using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class GraphHasNoCycleValidation : IGraphValidation
    {
        private const string k_ErrorMessageAdd = "Adding this edge would create a cycle in the graph.";
        private const string k_ErrorMessage = "A cycle has been detected in the graph :";

        private class IterationContext
        {
            public readonly HashSet<INode<InputPorts, OutputPorts>> nodePool;
            public readonly Dictionary<INode<InputPorts, OutputPorts>, List<INode<InputPorts, OutputPorts>>> nodeNeighbours;
            public readonly List<INode<InputPorts, OutputPorts>> nodesToExplore;

            private readonly HashSet<INode<InputPorts, OutputPorts>> bestCandidates;

            public IterationContext(GraphValidationContext context)
            {
                nodePool = new HashSet<INode<InputPorts, OutputPorts>>();
                bestCandidates = new HashSet<INode<InputPorts, OutputPorts>>();
                var bestCandidatesHistory = new HashSet<INode<InputPorts, OutputPorts>>();

                nodeNeighbours = new Dictionary<INode<InputPorts, OutputPorts>, List<INode<InputPorts, OutputPorts>>>();
                foreach (var edge in context.graph.Edges)
                {
                    var originNode = edge.Origin.Node;
                    var destinationNode = edge.Destination.Node;

                    //TODO we should use only valid edges. Could we run this validation in the end and use the result
                    //of previous ones to only use valid edges?
                    if (originNode == null || destinationNode == null || !context.UniqueNodes.Contains(originNode) || !context.UniqueNodes.Contains(destinationNode))
                        continue;

                    nodePool.Add(originNode);
                    nodePool.Add(destinationNode);

                    if (nodeNeighbours.TryGetValue(originNode, out var nodes))
                    {
                        nodes.Add(destinationNode);
                    }
                    else
                    {
                        nodeNeighbours.Add(originNode, new List<INode<InputPorts, OutputPorts>> { destinationNode });
                    }

                    bestCandidatesHistory.Add(destinationNode);
                    bestCandidates.Remove(destinationNode);

                    if (!bestCandidatesHistory.Contains(originNode))
                    {
                        bestCandidates.Add(originNode);
                    }
                }

                nodesToExplore = new List<INode<InputPorts, OutputPorts>>(nodePool.Count);
            }

            public void ResetIteration()
            {
                nodesToExplore.Clear();

                if (nodePool.Count > 0)
                {
                    if (bestCandidates.Count > 0)
                    {
                        using var enumerator = bestCandidates.GetEnumerator();
                        enumerator.MoveNext();
                        nodesToExplore.Add(enumerator.Current);
                        bestCandidates.Remove(enumerator.Current);
                    }
                    else
                    {
                        using var enumerator = nodePool.GetEnumerator();
                        enumerator.MoveNext();
                        nodesToExplore.Add(enumerator.Current);
                    }
                }
            }
        }

        //To detect all cycles in a graph we need to :
        // - Build a list of nodes to check
        // - Choose a starting point and try to detect cycles
        // - Remove the list of node we explored during this iteration from the nodePool so we don't explore the same branch
        // - Do another iteration with the rest of nodePool
        // - Continue until we checked all the nodes
        //
        //Commonly, several iterations will be necessary if the graph has multiple starting node or part of it
        //is detached from the others.
        public void Validate(GraphValidationContext context)
        {
            var iterationContext = new IterationContext(context);

            if (iterationContext.nodePool.Count == 0)
                return;

            do
            {
                iterationContext.ResetIteration();
                DetectCycles(iterationContext, context);
            }
            while (iterationContext.nodePool.Count > 0);
        }

        //To detect a cycle, we explore the graph one neighbour at a time, going to the maximum depth and backtracking along our path
        //until we find another branch to explore.
        //We ensure to not explore the same branch multiple time, trading an exhaustive cycle detection for more performance.
        //We exit when we come back to our path starting point and no neighbour is available
        //The rest of the graph will be explored by another iteration of this algorithm.
        private void DetectCycles(IterationContext iterationContext, GraphValidationContext context)
        {
            INode<InputPorts, OutputPorts> currentNode;
            List<INode<InputPorts, OutputPorts>> neighbours;
            var currentPath = new List<INode<InputPorts, OutputPorts>>();
            var currentPathCounters = new Dictionary<INode<InputPorts, OutputPorts>, int>();

            currentNode = iterationContext.nodesToExplore[0];

            //We will exit only if we backtrack enough so that we have nothing left to explore
            while (true)
            {
                //Add currentNode to path
                currentPath.Add(currentNode);
                if (currentPathCounters.ContainsKey(currentNode))
                    currentPathCounters[currentNode]++;
                else
                    currentPathCounters.Add(currentNode, 1);

                //Prevent us from re-exploring this part of the graph in the next iteration
                iterationContext.nodePool.Remove(currentNode);

                //If we don't have any neighbour for the currentNode, backtrack on our path until we find a new neighbour to explore
                while (!iterationContext.nodeNeighbours.TryGetValue(currentNode, out neighbours))
                {
                    //We explored all neighbours and cannot backtrack more, exit this iteration
                    if (currentPath.Count == 1)
                        return;

                    currentNode = currentPath[^ 2];

                    //Remove last in path
                    var toRemove = currentPath[^ 1];
                    if (currentPathCounters[toRemove] == 1)
                        currentPathCounters.Remove(toRemove);
                    else
                        currentPathCounters[toRemove]--;
                    currentPath.RemoveAt(currentPath.Count - 1);
                }

                var index = neighbours.Count - 1;
                var candidate = neighbours[index];

                //Remove the explored neighbours from the neighbours list of the currentNode (or the list itself if it was the last neighbour)
                if (neighbours.Count == 1)
                    iterationContext.nodeNeighbours.Remove(currentNode);
                else
                    iterationContext.nodeNeighbours[currentNode].RemoveAt(index);

                //We encountered a node already explored on our path => cycle detected
                if (currentPathCounters.ContainsKey(candidate))
                {
                    ComputeCycleError(candidate, currentPath, context);
                }

                currentNode = candidate;
            }
        }

        private static void ComputeCycleError(INode<InputPorts, OutputPorts> start, List<INode<InputPorts, OutputPorts>> path, GraphValidationContext context)
        {
            var nodes = new List<INode<InputPorts, OutputPorts>> { start };

            for (var i = path.Count - 1; i >= 0; i--)
            {
                nodes.Add(path[i]);
                if (path[i] == start)
                    break;
            }

            nodes.Reverse();

            context.Errors.Add(new ConflictingComponentsValidationError<INode<InputPorts, OutputPorts>>(nodes, k_ErrorMessage, typeof(GraphHasNoCycleValidation)));
        }
    }
}
