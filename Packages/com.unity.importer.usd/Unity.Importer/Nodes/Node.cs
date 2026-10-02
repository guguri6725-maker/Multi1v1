using System;

namespace UnityEngine.Importer
{
    /// <summary>
    /// Base class used to implement an import step in an <see cref="ImporterGraph" />.
    /// </summary>
    /// <remarks>
    /// Inherit from it to implement your own import process step and add them to a graph to drive your import.
    /// For example, this node is getting a GameObject as an input, adds a Camera component to it and output the modified GameObject.
    /// <seealso cref="INSERT-DOCUMENTATION-LINK"/>
    /// </remarks>
    /// A valid node must abide the following rules :
    /// - Node inherit from <see cref="Node{TInput,TOutput}"/> or implement <see cref="INode{TInput,TOutput}"/>
    /// - Node is decorated with a <see cref="NodeMetadataAttribute"/>> that has a unique combination of nodeId and version
    /// - is not generic (generic nodes can be declared but have to be abstract)
    /// <example>
    /// <code>
    /// using UnityEngine;
    ///
    /// [NodeMetadata("CameraNode", 0)]
    /// public class AddCameraNode : Node&lt;AddCameraNode.MyInputPort, AddCameraNode.MyOutputPort&gt;
    /// {
    ///     public class MyInputPort : InputPorts
    ///     {
    ///         public GameObject root;
    ///     }
    ///     public class MyOutputPort : OutputPorts
    ///     {
    ///         public GameObject root;
    ///     }
    ///
    ///     public override void Run()
    ///     {
    ///         GameObject root = Input.root;
    ///         root.AddComponent&lt;Camera&gt;();
    ///         Output.root = root;
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="TInput">The type containing all inputs for this import step.</typeparam>
    /// <typeparam name="TOutput">The type containing all outputs for this import step.</typeparam>
    public abstract class Node<TInput, TOutput> : INode<TInput, TOutput>
        where TInput : InputPorts
        where TOutput : OutputPorts
    {
        //TODO remove this field as soon as https://jira.unity3d.com/browse/UUM-15059 is fixed
        [HideInInspector]
        [SerializeField] private byte temporaryField;


        private TInput m_Input;
        private TOutput m_Output;

        /// <summary>
        /// The instance of the class containing all the Inputs required to process the node.
        /// </summary>
        /// <remarks>
        /// The content of this class will be populated by the graph execution
        /// and should be used in the <see cref="Run"/> method to generate the <see cref="Output"/> content.
        /// </remarks>
        public TInput Input
        {
            get => m_Input;
            set => m_Input = value;
        }

        /// <summary>
        /// The instance of the class containing all the Outputs that the node is creating.
        /// </summary>
        /// <remarks>
        /// The content of this class need to be populated during the <see cref="Run"/> method
        /// based on the content received in the <see cref="Input"/>.
        /// </remarks>
        public TOutput Output
        {
            get => m_Output;
            set => m_Output = value;
        }

        /// <summary>
        /// This method has to be implemented for each <see cref="Node{TInput,TOutput}"/>.
        /// </summary>
        /// <remarks>
        /// During the <see cref="ImporterGraph"/> execution,
        /// this method is responsible for reading content from its <see cref="Input"/>
        /// and populate its <see cref="Output"/> with its result.
        /// </remarks>
        public abstract void Run();

        InputPorts INode<TInput, TOutput>.Input
        {
            get => m_Input;
            set => m_Input = (TInput)value;
        }

        OutputPorts INode<TInput, TOutput>.Output
        {
            get => m_Output;
            set => m_Output = (TOutput)value;
        }

#if UNITY_INCLUDE_TESTS
        //Used by Mock during the test : NodeValidationTests.ValidateNodeAdded_IsOfInvalidType_ReturnErrors
        internal new virtual Type GetType() => base.GetType();
#endif
    }

    /// <summary>
    /// Base class for any <see cref="Node{TInput,TOutput}"/> Inputs port.
    /// </summary>
    /// <remarks>
    /// Inherit from this class to declare all required Inputs for a given node.
    /// </remarks>
    /// <example>
    /// <code>
    /// using UnityEngine;
    ///
    /// public class AddCameraNode : Node&lt;AddCameraNode.MyInputPort, AddCameraNode.MyOutputPort&gt;
    /// {
    ///     public class MyInputPort : InputPorts
    ///     {
    ///         public GameObject root; // The GameObject on which we want to add the camera component
    ///         public Color backgroundColor; // The backgroundColor set on the Camera
    ///     }
    ///
    ///     public class MyOutputPort : OutputPorts
    ///     {
    ///         public GameObject root;
    ///     }
    ///
    ///     ...
    /// }
    /// </code>
    /// </example>
    public abstract class InputPorts
    {
        /// <summary>
        /// An asset import context provided during the execution of a graph.
        /// </summary>
        public AssetLoading AssetLoading;

        /// <summary>
        /// A context to store and access logs during the execution of a graph.
        /// </summary>
        public GraphLogger GraphLogger;
    }

    /// <summary>
    /// Base class for any <see cref="Node{TInput,TOutput}"/> Outputs port.
    /// </summary>
    /// <remarks>
    /// Inherit from this class to declare all expected Outputs for a given node.
    /// </remarks>
    /// <example>
    /// <code>
    /// using UnityEngine;
    ///
    /// public class AddCameraNode : Node&lt;AddCameraNode.MyInputPort, AddCameraNode.MyOutputPort&gt;
    /// {
    ///     public class MyInputPort : InputPorts
    ///     {
    ///         public GameObject root; // The GameObject on which we want to add the camera component
    ///         public Color backgroundColor; // The backgroundColor set on the Camera
    ///     }
    ///
    ///     public class MyOutputPort : OutputPorts
    ///     {
    ///         public GameObject root;
    ///     }
    ///
    ///     ...
    /// }
    /// </code>
    /// </example>
    public abstract class OutputPorts
    {
    }

    /// <summary>
    /// This base interface is used by the graph runner API in order to execute an ImporterGraph.
    /// <seealso cref="Node{TInput,TOutput}"/>
    /// </summary>
    /// <remarks>
    /// It is recommended to inherits from the <see cref="Node{TInput,TOutput}"/> API in order to use this in the ImporterGraph system.
    /// </remarks>
    /// <typeparam name="TInput">The type containing all inputs for this import step.</typeparam>
    /// <typeparam name="TOutput">The type containing all outputs for this import step.</typeparam>
    public interface INode<out TInput, out TOutput> : INodeSerialization
        where TInput : InputPorts
        where TOutput : OutputPorts
    {
        /// <summary>
        /// This method is called by the graph runner to execute an import step during the import process.
        /// See more details on the <see cref="Node{TInput,TOutput}"/> API.
        /// </summary>
        void Run();
        /// <summary>
        /// The Inputs required by the node that are populated automatically by the graph runner.
        /// See more details on the <see cref="Node{TInput,TOutput}"/> API.
        /// </summary>
        InputPorts Input { get; set; }
        /// <summary>
        /// The Outputs expected from the node.
        /// See more details on the <see cref="Node{TInput,TOutput}"/> API.
        /// </summary>
        OutputPorts Output { get; set; }

        internal Type GetInputType => typeof(TInput);
        internal Type GetOutputType => typeof(TOutput);
    }

    /// <summary>
    /// Interface used as a workaround for serialization until Generics are supported with [SerializeReference]
    /// </summary>
    public interface INodeSerialization
    {
    }
}
