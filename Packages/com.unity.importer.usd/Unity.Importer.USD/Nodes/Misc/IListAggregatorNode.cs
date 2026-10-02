using System.Collections;
using System.Reflection;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will aggregate its inputs into a single output.
    /// It handles inputs that implements IList.
    /// </summary>
    /// <typeparam name="T">The type of IList of the inputs and the output.</typeparam>
    [NodeMetadata("IListAggregatorNode", 0)]
    public class IListAggregatorNode<T> : Node<IListAggregatorNode<T>.InputPort, IListAggregatorNode<T>.OutputPort> where T : IList, new()
    {
        private const string baseFieldName = "inputs_";

        /// <summary>
        /// Input ports of the <see cref="IListAggregatorNode{T}"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// Input values to aggregate.
            /// </summary>
            public T inputs_0;

            /// <summary>
            /// Input values to aggregate.
            /// </summary>
            public T inputs_1;

            /// <summary>
            /// Input values to aggregate.
            /// </summary>
            public T inputs_2;

            /// <summary>
            /// Input values to aggregate.
            /// </summary>
            public T inputs_3;

            /// <summary>
            /// Input values to aggregate.
            /// </summary>
            public T inputs_4;

            /// <summary>
            /// Input values to aggregate.
            /// </summary>
            public T inputs_5;

            /// <summary>
            /// Input values to aggregate.
            /// </summary>
            public T inputs_6;
        }

        /// <summary>
        /// Output ports of the <see cref="IListAggregatorNode{T}"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// Result of the aggregation of all input values.
            /// </summary>
            public T output;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.output = new T();
            var type = Input.GetType();
            var field = type.GetField($"{baseFieldName}0", BindingFlags.Instance | BindingFlags.Public);
            var i = 0;
            while (field != null)
            {
                MergeList((T)field.GetValue(Input), Output.output);
                i++;
                field = type.GetField($"{baseFieldName}{i}", BindingFlags.Instance | BindingFlags.Public);
            }
        }

        private static void MergeList(T toMerge, T result)
        {
            if (toMerge == null)
                return;

            foreach (var entry in toMerge)
            {
                result.Add(entry);
            }
        }
    }
}
