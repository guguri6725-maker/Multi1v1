using System;
using System.Collections.Generic;

namespace UnityEngine.Importer
{
    /// <summary>
    /// A graph runner that will traverse and execute all nodes of a provided <see cref="ImporterGraph"/>.
    /// </summary>
    /// <remarks>
    /// At the end of the execution, the <see cref="ResultEdge"/> list is used to gather a list of result.
    /// These results are returned to the caller, along with a success flag and the result of the graph validation.
    /// </remarks>
    public static class GraphRunner
    {
        /// <summary>
        /// Traverse and execute all nodes of the provided <see cref="ImporterGraph"/>.
        /// </summary>
        /// <remarks>
        /// Ensure that your graph is valid before running it to prevent unexpected behaviours or import results.
        /// </remarks>
        /// <param name="graph">The <see cref="ImporterGraph"/> to execute.</param>
        /// <param name="assetLoading">An optional <see cref="AssetLoading"/> to be provided to each node of the graph during the execution.</param>
        /// <param name="graphLogger">An optional <see cref="GraphLogger"/> to be provided to each node of the graph during the execution.</param>
        /// <param name="settingOverride">An optional collection of <see cref="ImportSetting{T}"/> to use as override of some of the <see cref="ImporterGraph"/> settings.</param>
        /// <returns>The <see cref="GraphExecutionResult"/> of this run.</returns>
        public static GraphExecutionResult Run(ImporterGraph graph, AssetLoading assetLoading = null, GraphLogger graphLogger = null, IEnumerable<IGraphValue> settingOverride = null)
        {
            try
            {
                SetNodesDefaultInputAndOutput(graph, assetLoading ?? new RuntimeAssetLoading(string.Empty), graphLogger ?? new RuntimeGraphLogger());
                SetInputFromSettingsAndConstants(graph, settingOverride);

                var executionContext = new GraphExecutionContext(graph);

                TraverseAll(executionContext);
                return new GraphExecutionResult(GetImporterResults(graph));
            }
            catch (Exception e)
            {
                Debug.LogError($"Graph execution failed due to the following exception. Ensure that your graph is valid with 'graph.{nameof(ImporterGraph.Validate)}'.\n{e}");
                return new GraphExecutionResult(e);
            }
        }

        private static void SetNodesDefaultInputAndOutput(ImporterGraph graph, AssetLoading assetLoading, GraphLogger graphLogger)
        {
            foreach (var node in graph.Nodes)
            {
                if (node == null)
                    continue;

                node.Input = (InputPorts)Activator.CreateInstance(node.GetInputType);
                node.Input.AssetLoading = assetLoading;
                node.Input.GraphLogger = graphLogger;

                node.Output = (OutputPorts)Activator.CreateInstance(node.GetOutputType);
            }
        }

        private static void SetInputFromSettingsAndConstants(ImporterGraph graph, IEnumerable<IGraphValue> settingOverride)
        {
            var settings = new Dictionary<string, IGraphValue>();
            var settingsIds = new HashSet<string>();

            if (settingOverride != null)
            {
                foreach (var setting in settingOverride)
                {
                    settings.Add(setting.Id, setting);
                    settingsIds.Add(setting.Id);
                }
            }

            foreach (var setting in graph.ImportSettings)
            {
                if (setting != null && settings.TryAdd(setting.Id, setting))
                {
                    settingsIds.Add(setting.Id);
                }
            }

            foreach (var edge in graph.SettingEdges)
            {
                if (settingsIds.Contains(edge.Id) && edge.Destination.Node != null)
                {
                    edge.Destination.SetPortValue(settings[edge.Id].Value);
                }
            }

            foreach (var constant in graph.ImportConstants)
            {
                if (constant != null && constant.Destination.Node != null)
                {
                    constant.Destination.SetPortValue(constant.Value);
                }
            }
        }

        private static void TraverseAll(GraphExecutionContext context)
        {
            while (context.candidates.Count > 0)
            {
                var candidate = context.candidates.Pop();

                try
                {
                    candidate.Run();
                }
                catch (Exception e)
                {
                    Debug.LogError($"An exception occured during the execution of node {candidate}:\n{e}");
                    throw;
                }

                if (context.outgoingEdgeDestinations.TryGetValue(candidate, out var nextEdges))
                {
                    foreach (var edge in nextEdges)
                    {
                        var destinationNode = edge.Destination.Node;
                        SetNodeInputs(edge);
                        if (context.incomingEdgeCount.TryGetValue(destinationNode, out var count))
                        {
                            if (count == 1)
                            {
                                context.candidates.Push(destinationNode);
                            }
                            else
                            {
                                context.incomingEdgeCount[destinationNode]--;
                            }
                        }
                    }
                }
            }
        }

        private static void SetNodeInputs(Edge edge)
        {
            edge.Destination.SetPortValue(edge.Origin.GetPortValue());
        }

        private static List<IGraphValue> GetImporterResults(ImporterGraph graph)
        {
            var results = new List<IGraphValue>();
            foreach (var graphResult in graph.ResultEdges)
            {
                if (graphResult.Origin.Node == null)
                    continue;

                results.Add(graphResult.GetImportResultValue());
            }
            return results;
        }
    }
}
