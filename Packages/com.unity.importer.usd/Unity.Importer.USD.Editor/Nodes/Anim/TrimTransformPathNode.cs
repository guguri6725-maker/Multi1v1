using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// This node trims the editor curve binding paths. When there is only one default prim and preserve scene root is disabled,
    /// no additional root is created, and the game object resulting from the root will be renamed by the prefab system,
    /// breaking the animation. Trimming the root's name from the paths keeps the animation valid.
    /// </summary>
    [NodeMetadata("TrimTransformPathNode", 0)]
    public class TrimTransformPathNode : Node<TrimTransformPathNode.InputPort, TrimTransformPathNode.OutputPort>
    {
        public class InputPort : InputPorts
        {
            /// <summary>
            /// The root of the Unity GameObjects hierarchy.
            /// </summary>
            public GameObject root;

            /// <summary>
            /// The animated properties which editor curve binding paths need to be trimmed.
            /// </summary>
            public Dictionary<EditorCurveBinding, Keyframe[]> animatedProperties;
        }

        /// <summary>
        /// Output ports for the <see cref="CreateAnimationClipNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// The animated properties which editor curve binding paths have been trimmed.
            /// </summary>
            public Dictionary<EditorCurveBinding, Keyframe[]> animatedProperties;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.animatedProperties = new Dictionary<EditorCurveBinding, Keyframe[]>(Input.animatedProperties.Count);

            foreach (var kvp in Input.animatedProperties)
            {
                var binding = kvp.Key;
                var firstSeparatorIndex = binding.path.IndexOf('/');
                var pathRoot = firstSeparatorIndex > 0
                    ? binding.path.Substring(0, firstSeparatorIndex)
                    : binding.path;
                if (Input.root.name == pathRoot) // no additional root added, root will be renamed by prefab system, remove root from path
                {
                    binding.path = firstSeparatorIndex > 0
                        ? binding.path.Substring(firstSeparatorIndex + 1)
                        : String.Empty;
                }

                Output.animatedProperties.Add(binding, kvp.Value);
            }
        }
    }
}
