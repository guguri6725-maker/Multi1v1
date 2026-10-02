using System;
using System.Collections.Generic;

namespace UnityEngine.Importer
{
    /// <summary>
    /// Represents a Constant value in an <see cref="ImporterGraph"/>.
    /// See also: <seealso cref="ImportConstant{T}"/>.
    /// </summary>
    public interface IConstantValue : IDestinationEdge
    {
        /// <summary>
        /// The value of the constant.
        /// </summary>
        object Value { get; }
        /// <summary>
        /// The Type of the constant.
        /// </summary>
        Type Type { get; }
    }

    /// <summary>
    /// Represents a constant in the <see cref="ImporterGraph"/> that contains a <see cref="Value"/> linked to a <see cref="Node{TInput,TOutput}"/> <see cref="InputPorts"/>.
    /// </summary>
    /// <remarks>
    /// Constants can be added and removed from the <see cref="ImporterGraph"/> using <see cref="ImporterGraph.AddImportConstant"/> and <see cref="ImporterGraph.RemoveImportConstant"/>.
    ///
    /// <code>
    /// using UnityEngine.Importer;
    /// using UnityEngine;
    ///
    /// public class ExampleNode : Node&lt;ExampleNode.MyInputs, ExampleNode.MyOutputs&gt;
    /// {
    ///     public class MyInputs : NodeInputs
    ///     {
    ///         public int ExampleInt;
    ///     }
    ///     public class MyOutputs : NodeOutputs
    ///     {
    ///     }
    ///     public override void Run() {}
    /// }
    ///
    /// public static class ExampleClass
    /// {
    ///     public static void AddNewConstantToAGraph()
    ///     {
    ///         var graph = ScriptableObject.CreateInstance&lt;ImporterGraph&gt;();
    ///         var node = new ExampleNode();
    ///         graph.AddNode(node);
    ///         var newConstant = new ImportConstant&lt;int&gt;(5, node, nameof(ExampleNode.MyInputs.ExampleInt));
    ///         graph.AddConstant(newConstant);
    ///     }
    /// }
    /// </code>
    /// </remarks>
    /// <typeparam name="T">The setting's type.</typeparam>
    [Serializable]
    public class ImportConstant<T> : IConstantValue, IEquatable<ImportConstant<T>>
    {
        [SerializeField] private T value;
        [SerializeField] private NodePort destination;

        /// <summary>
        /// Creates a new instance of a <see cref="ImportConstant{T}"/>.
        /// </summary>
        /// <param name="value">The value of the constant.</param>
        /// <param name="destination">The <see cref="Edge"/> destination <see cref="INode{TInput,TOutput}"/>.</param>
        /// <param name="destinationFieldName">The <see cref="Edge"/> destination input field on the destination <see cref="INode{TInput,TOutput}"/>.</param>
        public ImportConstant(T value, INode<InputPorts, OutputPorts> destination, string destinationFieldName)
        {
            this.value = value;
            this.destination = new NodePort(destination, NodePort.NodePortTarget.Input, destinationFieldName);
        }

        /// <summary>
        /// The value of the constant.
        /// </summary>
        public T Value => value;

        /// <inheritdoc cref="IConstantValue.Value"/>
        object IConstantValue.Value => value;

        /// <summary>
        /// The Type of the constant.
        /// </summary>
        public Type Type => typeof(T);

        /// <inheritdoc cref="IDestinationEdge.Destination"/>
        public NodePort Destination => destination;

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"{value} => {destination}";
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public bool Equals(ImportConstant<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<T>.Default.Equals(value, other.value) && destination.Equals(other.destination);
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ImportConstant<T>)obj);
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            return HashCode.Combine(value, destination);
        }
    }
}
