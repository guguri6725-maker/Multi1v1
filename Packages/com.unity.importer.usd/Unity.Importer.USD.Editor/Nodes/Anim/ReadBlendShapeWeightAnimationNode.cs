using System.Collections.Generic;
using pxr;
using Unity.Importer.USD;
using UnityEngine;
using UnityEngine.Importer;
using USD.NET;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// This node will read skeletal blend shape weight animation
    /// The node has a single output: all the found animation for animated blend shape weights
    /// </summary>
    [NodeMetadata("ReadBlendShapeWeightAnimationNode", 0)]
    public class ReadBlendShapeWeightAnimationNode : Node<ReadBlendShapeWeightAnimationNode.InputPort, ReadBlendShapeWeightAnimationNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadBlendShapeWeightAnimationNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// The animated bindings
            /// </summary>
            public Dictionary<UsdSkelAnimation, List<UsdSkelSkeleton>> skelAnimToSkelSkeletons;

            /// <summary>
            /// The skinning query data relating to the animated bindings (the ones in skelAnimToSkelSkeletons)
            /// </summary>
            public Dictionary<UsdSkelAnimation, List<SkinningQueryData>> skelAnimToSkinningQueryData;

            /// <summary>
            /// USD stage metadata.
            /// </summary>
            public UsdStageMetadata usdMetadata;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadBlendShapeWeightAnimationNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// All the animated blend shape weight properties
            /// </summary>
            public Dictionary<EditorCurveBinding, Keyframe[]> animatedProperties = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            var skelAnimToSkelSkeletons = Input.skelAnimToSkelSkeletons;
            if (skelAnimToSkelSkeletons == null || skelAnimToSkelSkeletons.Count == 0)
                return;

            var skelAnimToSkininningQueryData = Input.skelAnimToSkinningQueryData;
            if (skelAnimToSkininningQueryData == null || skelAnimToSkininningQueryData.Count == 0)
                return;

            var animatedProperties = ReadBlendShapeWeightAnim(skelAnimToSkelSkeletons, skelAnimToSkininningQueryData);
            if (animatedProperties.Count > 0)
                Output.animatedProperties = animatedProperties;
        }

        Dictionary<EditorCurveBinding, Keyframe[]> ReadBlendShapeWeightAnim(
            Dictionary<UsdSkelAnimation, List<UsdSkelSkeleton>> skelAnimToSkelSkeletons,
            Dictionary<UsdSkelAnimation, List<SkinningQueryData>> skelAnimToSkininningQueryData)
        {
            var animatedProperties = new Dictionary<EditorCurveBinding, Keyframe[]>();
            var timeCodesPerSecond = (float)Input.usdMetadata.timeCodesPerSecond.GetValue();
            foreach (var kvp in skelAnimToSkelSkeletons)
            {
                var skelAnim = kvp.Key;
                var blendShapeNames = IntrinsicTypeConverter.FromVtArray((VtTokenArray)skelAnim.GetBlendShapesAttr().Get());
                var nbBlendShapes = blendShapeNames.Length;
                var blendShapeWeightsAttr = skelAnim.GetBlendShapeWeightsAttr();
                var timeSamples = blendShapeWeightsAttr.GetTimeSamples();
                var nbKeyframes = timeSamples.Count;
                for (int b = 0; b < nbBlendShapes; ++b)
                {
                    if (!skelAnimToSkininningQueryData.ContainsKey(skelAnim))
                        continue;

                    var keyframes = new Keyframe[nbKeyframes];
                    for (var t = 0; t < nbKeyframes; ++t)
                    {
                        var time = timeSamples[t];
                        var weights = (VtFloatArray)blendShapeWeightsAttr.Get(time);
                        keyframes[t] = new Keyframe
                        {
                            time = (float)time / timeCodesPerSecond,
                            value = weights[b]
                        };
                    }

                    foreach (var skinningData in skelAnimToSkininningQueryData[skelAnim])
                    {
                        var meshPath = skinningData.meshPath.TrimStart('/');
                        var mappings = skinningData.blendShapeTokenToTarget;
                        if (mappings.ContainsKey(blendShapeNames[b]))
                        {
                            // The property path in Unity is "blendshape.<shapeName>"
                            // Our mappings contain the "full path to the blend shape property", i.e. "/prim_path/blendshape_name"
                            var propertyName = "blendShape." + mappings[blendShapeNames[b]].Substring(meshPath.Length + 2);
                            var binding = new EditorCurveBinding
                            {
                                path = meshPath,
                                type = typeof(SkinnedMeshRenderer),
                                propertyName = propertyName
                            };

                            animatedProperties[binding] = keyframes;
                        }
                    }
                }
            }

            return animatedProperties;
        }
    }
}
