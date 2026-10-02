using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will aggregate its inputs into a single output.
    /// It handles inputs that implements IDictionary.
    /// </summary>
    /// <typeparam name="T">The type of IDictionary of the inputs and the output.</typeparam>
    [NodeMetadata("IDictionaryAggregatorNode", 0)]
    public class IDictionaryAggregatorNode<T> : Node<IDictionaryAggregatorNode<T>.InputPort, IDictionaryAggregatorNode<T>.OutputPort> where T : IDictionary, new()
    {
        private const string baseFieldName = "inputs_";

        /// <summary>
        /// Input ports of the <see cref="IDictionaryAggregatorNode{T}"/>.
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

            /// <summary>
            /// Input values to aggregate.
            /// </summary>
            public T inputs_7;

            /// <summary>
            /// Input values to aggregate.
            /// </summary>
            public T inputs_8;
        }

        /// <summary>
        /// Output ports of the <see cref="IDictionaryAggregatorNode{T}"/>.
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
                MergeDictionary((T)field.GetValue(Input), Output.output);
                i++;
                field = type.GetField($"{baseFieldName}{i}", BindingFlags.Instance | BindingFlags.Public);
            }
        }

        private static void MergeDictionary(T toMerge, T result)
        {
            if (toMerge == null)
                return;

            try
            {
                foreach (DictionaryEntry kvp in toMerge)
                {
                    if (!result.Contains(kvp.Key))
                    {
                        result.Add(kvp.Key, kvp.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error while merging dictionary : {e}");
                throw;
            }
        }
    }
}
