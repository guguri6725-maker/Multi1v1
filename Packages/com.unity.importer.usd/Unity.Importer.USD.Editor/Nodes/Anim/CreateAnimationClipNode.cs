using System.Collections.Generic;
using System.IO;
using Unity.Importer.USD;
using UnityEngine;
using UnityEngine.Importer;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// This node will create an animation clip containing all the passed animation data.
    /// </summary>
    [NodeMetadata("CreateAnimationClipNode", 0)]
    public class CreateAnimationClipNode : Node<CreateAnimationClipNode.InputPort, CreateAnimationClipNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="CreateAnimationClipNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// The properties to animate
            /// </summary>
            public Dictionary<EditorCurveBinding, Keyframe[]> animatedProperties;

            /// <summary>
            /// A flag telling if the animations loop time should be enabled.
            /// </summary>
            public bool loopAnimations;

            /// <summary>
            /// USD stage metadata.
            /// </summary>
            public UsdStageMetadata usdMetadata;
        }

        /// <summary>
        /// Output ports for the <see cref="CreateAnimationClipNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// The animation clip that gets created
            /// </summary>
            public AnimationClip clip;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            if (Input.animatedProperties == null || Input.animatedProperties.Count == 0)
                return;

            Output.clip = new AnimationClip();
            Output.clip.legacy = false;
            Output.clip.frameRate = (float)Input.usdMetadata.timeCodesPerSecond.GetValue();
            Output.clip.name = Path.GetFileNameWithoutExtension(Input.AssetLoading.AssetPath);

            if (Input.loopAnimations)
            {
                Output.clip.wrapMode = WrapMode.Loop;
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(Output.clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(Output.clip, settings);
            }

            var nbCurves = Input.animatedProperties.Count;
            var bindings = new EditorCurveBinding[nbCurves];
            var animCurves = new AnimationCurve[nbCurves];

            int curveIndex = 0;
            foreach (var kvp in Input.animatedProperties)
            {
                bindings[curveIndex] = kvp.Key;
                var curve = new AnimationCurve(kvp.Value);
                SetTangentMode(curve);
                animCurves[curveIndex] = curve;

                curveIndex++;
            }

            AnimationUtility.SetEditorCurves(Output.clip, bindings, animCurves);
            Output.clip.EnsureQuaternionContinuity();
        }

        private static void SetTangentMode(AnimationCurve curve, AnimationUtility.TangentMode mode = AnimationUtility.TangentMode.Linear)
        {
            int nbKeys = curve.length;
            for (int k = 0; k < nbKeys; ++k)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, k, mode);
                AnimationUtility.SetKeyRightTangentMode(curve, k, mode);
            }
        }
    }
}
