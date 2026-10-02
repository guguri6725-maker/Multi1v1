using System;
using System.Collections.Generic;
using UnityEngine.Importer.Validations;

namespace UnityEngine.Importer
{
    /// <summary>
    /// This class represents an import process as a graph.
    /// <seealso cref="INSERT-DOCUMENTATION-LINK"/>
    /// </summary>
    public class ImporterGraph : ScriptableObject
    {
        [SerializeReference] private List<INodeSerialization> m_Nodes = new();
        [SerializeField] private List<Edge> m_Edges = new();
        [SerializeField] private List<ResultEdge> m_ResultEdges = new();
        [SerializeField] private List<SettingEdge> m_SettingEdges = new();
        [SerializeReference] private List<IGraphValue> m_ImportSettings = new();
        [SerializeReference] private List<IConstantValue> m_ImportConstants = new();

        /// <summary>
        /// List of all <see cref="Node{TInput,TOutput}"/> added to the <see cref="ImporterGraph"/>.
        /// </summary>
        public IReadOnlyList<INode<InputPorts, OutputPorts>> Nodes => m_Nodes.ConvertAll(n => (INode<InputPorts, OutputPorts>)n);

        /// <summary>
        /// List of all <see cref="Edge"/> added to the <see cref="ImporterGraph"/>.
        /// </summary>
        public IReadOnlyList<Edge> Edges => m_Edges;

        /// <summary>
        /// List of all <see cref="ResultEdge"/> added to the <see cref="ImporterGraph"/>.
        /// </summary>
        public IReadOnlyList<ResultEdge> ResultEdges => m_ResultEdges;

        /// <summary>
        /// The first <see cref="ResultEdge"/> of the <see cref="ResultEdges"/> or default if empty.
        /// </summary>
        public ResultEdge MainResult => m_ResultEdges.Count > 0 ? m_ResultEdges[0] : default;

        /// <summary>
        /// List of all <see cref="SettingEdge"/> added to the <see cref="ImporterGraph"/>.
        /// </summary>
        public IReadOnlyList<SettingEdge> SettingEdges => m_SettingEdges;

        /// <summary>
        /// List of all <see cref="ImportSetting{T}"/> added to the <see cref="ImporterGraph"/>.
        /// </summary>
        public IReadOnlyList<IGraphValue> ImportSettings => m_ImportSettings;

        /// <summary>
        /// List of all <see cref="ImportConstant{T}"/> added to the <see cref="ImporterGraph"/>.
        /// </summary>
        public IReadOnlyList<IConstantValue> ImportConstants => m_ImportConstants;

        /// <summary>
        /// Validate this <see cref="ImporterGraph"/>.
        /// </summary>
        /// <returns>The <see cref="GraphValidationResult"/> of this validation.</returns>
        public GraphValidationResult Validate()
        {
            var validator = new GraphValidator();
            var validationResult = validator.ValidateGraph(this);
            if (!validationResult.IsValid)
            {
                validationResult.DisplayErrors();
            }

            return validationResult;
        }

        /// <summary>
        /// Add a valid <see cref="INode{TInput,TOutput}"/> to the <see cref="ImporterGraph"/>.
        /// </summary>
        /// <param name="node">The <see cref="INode{TInput,TOutput}"/> to add.</param>
        /// <returns>The <see cref="GraphValidationResult"/> of this command.</returns>
        public GraphValidationResult AddNode(INode<InputPorts, OutputPorts> node)
        {
            return Add<INode<InputPorts, OutputPorts>, NodeValidator>(node, () => m_Nodes.Add(node));
        }

        /// <summary>
        /// Remove a valid <see cref="INode{TInput,TOutput}"/> from the <see cref="ImporterGraph"/>.
        /// </summary>
        /// <param name="node">The <see cref="INode{TInput,TOutput}"/> to remove.</param>
        /// <returns>The <see cref="GraphValidationResult"/> of this command.</returns>
        public GraphValidationResult RemoveNode(INode<InputPorts, OutputPorts> node)
        {
            return Remove<INode<InputPorts, OutputPorts>, NodeValidator>(node,
                () =>
                {
                    m_Nodes.Remove(node);
                    RemoveNodeDependencies(node);
                });
        }

        private void RemoveNodeDependencies(INode<InputPorts, OutputPorts> node)
        {
            for (var i = m_Edges.Count - 1; i >= 0; i--)
            {
                if (m_Edges[i].Origin.Node == node || m_Edges[i].Destination.Node == node)
                {
                    RemoveEdge(m_Edges[i]);
                }
            }

            for (var i = m_ResultEdges.Count - 1; i >= 0; i--)
            {
                if (m_ResultEdges[i].Origin.Node == node)
                {
                    RemoveResultEdge(m_ResultEdges[i]);
                }
            }

            for (var i = m_SettingEdges.Count - 1; i >= 0; i--)
            {
                if (m_SettingEdges[i].Destination.Node == node)
                {
                    RemoveSettingEdge(m_SettingEdges[i]);
                }
            }

            for (var i = m_ImportConstants.Count - 1; i >= 0; i--)
            {
                if (m_ImportConstants[i].Destination.Node == node)
                {
                    RemoveImportConstant(m_ImportConstants[i]);
                }
            }
        }

        /// <summary>
        /// Add a valid <see cref="Edge"/> to the <see cref="ImporterGraph"/>.
        /// </summary>
        /// <param name="edge">The <see cref="Edge"/> to add.</param>
        /// <returns>The <see cref="GraphValidationResult"/> of this command.</returns>
        public GraphValidationResult AddEdge(Edge edge)
        {
            return Add<Edge, EdgeValidator>(edge, () => m_Edges.Add(edge));
        }

        /// <summary>
        /// Remove a valid <see cref="Edge"/> from the <see cref="ImporterGraph"/>.
        /// </summary>
        /// <param name="edge">The <see cref="Edge"/> to remove.</param>
        /// <returns>The <see cref="GraphValidationResult"/> of this command.</returns>
        public GraphValidationResult RemoveEdge(Edge edge)
        {
            return Remove<Edge, EdgeValidator>(edge, () => m_Edges.Remove(edge));
        }

        /// <summary>
        /// Adds a <see cref="ResultEdge"/> to the <see cref="ImporterGraph"/>.
        /// </summary>
        /// <param name="edge">The <see cref="ResultEdge"/> to add.</param>
        /// <returns>The <see cref="GraphValidationResult"/> of this command.</returns>
        public GraphValidationResult AddResultEdge(ResultEdge edge)
        {
            return Add<ResultEdge, ResultEdgeValidator>(edge, () => m_ResultEdges.Add(edge));
        }

        /// <summary>
        /// Remove a <see cref="ResultEdge"/> from the <see cref="ImporterGraph"/>.
        /// </summary>
        /// <param name="edge">The <see cref="ResultEdge"/> to remove.</param>
        /// <returns>The <see cref="GraphValidationResult"/> of this command.</returns>
        public GraphValidationResult RemoveResultEdge(ResultEdge edge)
        {
            return Remove<ResultEdge, ResultEdgeValidator>(edge, () => m_ResultEdges.Remove(edge));
        }

        /// <summary>
        /// Set an existing <see cref="ResultEdge"/> in the graph as the main result of the <see cref="ImporterGraph"/>.
        /// </summary>
        /// <remarks>
        /// This method effectively set <paramref name="edge"/> as the first <see cref="ResultEdge"/> of the <see cref="ResultEdges"/>.
        /// </remarks>
        /// <param name="edge">The <see cref="ResultEdge"/> to set as main result.</param>
        /// <returns>The <see cref="GraphValidationResult"/> of this command.</returns>
        public GraphValidationResult SetMainResultEdge(ResultEdge edge)
        {
            return Add<ResultEdge, MainResultValidator>(edge, () =>
            {
                var index = m_ResultEdges.IndexOf(edge);
                m_ResultEdges[index] = m_ResultEdges[0];
                m_ResultEdges[0] = edge;
            });
        }

        /// <summary>
        /// Adds a <see cref="SettingEdge"/> to the <see cref="ImporterGraph"/>.
        /// </summary>
        /// <param name="edge">The <see cref="SettingEdge"/> to add.</param>
        /// <returns>The <see cref="GraphValidationResult"/> of this command.</returns>
        public GraphValidationResult AddSettingEdge(SettingEdge edge)
        {
            return Add<SettingEdge, SettingEdgeValidator>(edge, () => m_SettingEdges.Add(edge));
        }

        /// <summary>
        /// Remove a <see cref="SettingEdge"/> from the <see cref="ImporterGraph"/>.
        /// </summary>
        /// <param name="edge">The <see cref="SettingEdge"/> to remove.</param>
        /// <returns>The <see cref="GraphValidationResult"/> of this command.</returns>
        public GraphValidationResult RemoveSettingEdge(SettingEdge edge)
        {
            return Remove<SettingEdge, SettingEdgeValidator>(edge, () => m_SettingEdges.Remove(edge));
        }

        /// <summary>
        /// Add a new <see cref="ImportSetting{T}"/> in the <see cref="ImporterGraph"/>.
        /// </summary>
        /// <remarks>
        /// While the method is asking for an <see cref="IGraphValue"/>, we've pre-implemented a base class for serialization purpose
        /// the makes this API easier to deal with. See <see cref="ImportSetting{T}"/>.
        /// </remarks>
        /// <param name="setting">The new setting to add to the <see cref="ImporterGraph"/>.</param>
        /// <returns>The <see cref="GraphValidationResult"/> of this command.</returns>
        public GraphValidationResult AddImportSetting(IGraphValue setting)
        {
            return Add<IGraphValue, ImportSettingValidator>(setting, () =>
            {
                m_ImportSettings.Add(setting);
            });
        }

        /// <summary>
        /// Removes an <see cref="ImportSetting{T}"/> from the <see cref="ImporterGraph"/>.
        /// </summary>
        /// <remarks>
        /// It's possible to gather all existing settings in the current <see cref="ImporterGraph"/> through <see cref="ImportSettings"/>.
        /// </remarks>
        /// <param name="setting">The existing <see cref="ImportSetting{T}"/> to remove.</param>
        /// <returns>The <see cref="GraphValidationResult"/> of this command.</returns>
        public GraphValidationResult RemoveImportSetting(IGraphValue setting)
        {
            return Remove<IGraphValue, ImportSettingValidator>(setting, () =>
            {
                m_ImportSettings.Remove(setting);
                RemoveImportSettingsDependencies(setting);
            });
        }

        private void RemoveImportSettingsDependencies(IGraphValue setting)
        {
            for (var i = m_SettingEdges.Count - 1; i >= 0; i--)
            {
                if (m_SettingEdges[i].Id == setting.Id)
                {
                    RemoveSettingEdge(m_SettingEdges[i]);
                }
            }
        }

        /// <summary>
        /// Adds a <see cref="ImportConstant{T}"/> to the <see cref="ImporterGraph"/>.
        /// </summary>
        /// <param name="constant">The <see cref="ImportConstant{T}"/> to add.</param>
        /// <returns>The <see cref="GraphValidationResult"/> of this command.</returns>
        public GraphValidationResult AddImportConstant(IConstantValue constant)
        {
            return Add<IConstantValue, ImportConstantValidator>(constant, () => m_ImportConstants.Add(constant));
        }

        /// <summary>
        /// Removes a <see cref="ImportConstant{T}"/> from the <see cref="ImporterGraph"/>.
        /// </summary>
        /// <param name="constant">The <see cref="ImportConstant{T}"/> to remove.</param>
        /// <returns>The <see cref="GraphValidationResult"/> of this command.</returns>
        public GraphValidationResult RemoveImportConstant(IConstantValue constant)
        {
            return Remove<IConstantValue, ImportConstantValidator>(constant, () => m_ImportConstants.Remove(constant));
        }

        private GraphValidationResult Add<TComponent, TValidator>(TComponent component, Action onAdd)
            where TValidator : BaseGraphComponentValidator<TComponent>, new()
        {
            var validator = new TValidator();
            var validationResult = validator.ValidateAddition(component, this);
            if (validationResult.IsValid)
            {
                onAdd();
            }
            else
            {
                validationResult.DisplayErrors();
            }
            return validationResult;
        }

        private GraphValidationResult Remove<TComponent, TValidator>(TComponent component, Action onRemove)
            where TValidator : BaseGraphComponentValidator<TComponent>, new()
        {
            var validator = new TValidator();
            var validationResult = validator.ValidateRemoval(component, this);
            if (validationResult.IsValid)
            {
                onRemove();
            }
            else
            {
                validationResult.DisplayErrors();
            }
            return validationResult;
        }
    }
}
