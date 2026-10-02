using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class GraphValidationContext
    {
        public ImporterGraph graph;

        public List<BaseGraphValidationError> Errors { get; } = new();

        private HashSet<INode<InputPorts, OutputPorts>> m_CachedUniqueNodes;
        private HashSet<Edge> m_CachedUniqueEdges;
        private HashSet<ResultEdge> m_CachedUniqueResultEdges;
        private HashSet<SettingEdge> m_CachedUniqueSettingEdges;
        private HashSet<IGraphValue> m_CachedUniqueImportSettings;
        private HashSet<IConstantValue> m_CachedUniqueConstants;

        public HashSet<INode<InputPorts, OutputPorts>> UniqueNodes =>
            m_CachedUniqueNodes ??= new HashSet<INode<InputPorts, OutputPorts>>(graph.Nodes);

        public HashSet<Edge> UniqueEdges =>
            m_CachedUniqueEdges ??= new HashSet<Edge>(graph.Edges);

        public HashSet<ResultEdge> UniqueResultEdges =>
            m_CachedUniqueResultEdges ??= new HashSet<ResultEdge>(graph.ResultEdges);

        public HashSet<SettingEdge> UniqueSettingEdges =>
            m_CachedUniqueSettingEdges ??= new HashSet<SettingEdge>(graph.SettingEdges);

        public HashSet<IGraphValue> UniqueImportSettings =>
            m_CachedUniqueImportSettings ??= new HashSet<IGraphValue>(graph.ImportSettings);

        public HashSet<IConstantValue> UniqueConstants =>
            m_CachedUniqueConstants ??= new HashSet<IConstantValue>(graph.ImportConstants);

        public GraphValidationContext(ImporterGraph graph)
        {
            this.graph = graph;
        }
    }
}
