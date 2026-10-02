using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using pxr;
using Unity.Importer.USD;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Importer;
using USD.NET;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// This node finds all the skeletons in the stage and populates structures that can be used later in the graph for
    /// animating joints and blend shape weights.
    ///
    /// It does so by using the UsdSkelCache to discover skeletal bindings first, then going deeper in the stage to
    /// analyse the bindings that might not be resolved by the cache. This allows post-import re-binding and
    /// re-targeting of skeletal rigs and animation in Unity.
    ///
    /// The Output structures are optimized to avoid duplication of the animation data. Skeletons can share the
    /// same animation, and so we only want to move one copy of the animation data through the graph
    ///
    /// We only output bindings when there is animation.
    ///
    /// The Outputted skinning query data are the ones that are under an animated UsdSkelRoot prim (the one that have
    /// skeletons that are animated)
    ///
    /// The skinless skeletons are skeletons for which the cache was not able to resolve bindings, but for which we want
    /// to create game objects and animation for post-import re-binding and re-targeting
    /// </summary>
    [NodeMetadata("ResolveSkelBindingsNode", 0)]
    public class ResolveSkelBindingsNode : Node<ResolveSkelBindingsNode.InputPort, ResolveSkelBindingsNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ResolveSkelBindingsNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// The list of UsdSkelRoot prims to use the cache on
            /// </summary>
            public List<UsdPrim> skelRoots;

            /// <summary>
            /// The list of prims having the UsdSkelBindingAPI applied for deeper discovery of skeletal bindings
            /// </summary>
            public List<UsdPrim> primsWithSkelBindingAPI;
        }

        /// <summary>
        /// Output ports of the <see cref="ResolveSkelBindingsNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// The animated bindings
            /// </summary>
            public Dictionary<UsdSkelAnimation, List<UsdSkelSkeleton>> skelAnimToSkelSkeletons = new();

            /// <summary>
            /// The existing skinning queries that relate to the animated bindings
            /// </summary>
            public Dictionary<UsdSkelAnimation, List<SkinningQueryData>> skelAnimToSkinningQueryData = new();

            /// <summary>
            /// The skinless skeletons
            /// </summary>
            public List<UsdSkelSkeleton> skinlessSkeletons = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            // One skelAnim data can animate multiple skelSkeleton prims
            var animatedBindings = new Dictionary<
                string, // UsdSkelAnimation path
                KeyValuePair<UsdSkelAnimation, List<UsdSkelSkeleton>>>();

            var animatedSkinningQueryDatas = new Dictionary<
                string, // UsdSkelAnimation path
                KeyValuePair<UsdSkelAnimation, List<SkinningQueryData>>>();

            // We start by resolving the skeleton/anim/skinning bindings by using the recommended cache. This catches
            // all the occurrences of skeletons that are actually bound to a mesh in the stage
            var skeletonPathsProcessedFromCache = PopulateSkelBindingsFromCache(animatedBindings, animatedSkinningQueryDatas);

            // Some DCC workflows export skeletal data that do not get bound to a mesh once the stage is computed.
            // For example, when per-vertex animation is baked on the skinned mesh object, DCCs will disconnect the
            // skeleton joints from the mesh in order to avoid a double mesh transformation (once by the enveloped
            // skeleton and once by the baked animated vertices). In these scenarios, the source skeletal animation is
            // still part of the stage, but it might be under a different prim type (not a SkelRoot) or still under a
            // SkelRoot but not bound to a mesh.
            //
            // In Unity, we want to import these skeletons and their animation for supporting post-import
            // re-binding / re-targeting workflows
            var skinlessSkeletons = PopulateSkelBindingsFromUnskinnedSkeletons(animatedBindings, skeletonPathsProcessedFromCache);

            foreach (var pair in animatedBindings.Values)
                Output.skelAnimToSkelSkeletons[pair.Key] = pair.Value;
            foreach (var pair in animatedSkinningQueryDatas.Values)
                Output.skelAnimToSkinningQueryData[pair.Key] = pair.Value;

            Output.skinlessSkeletons = skinlessSkeletons;
        }

        HashSet<string> PopulateSkelBindingsFromCache(
            Dictionary<string, KeyValuePair<UsdSkelAnimation, List<UsdSkelSkeleton>>> animatedBindings,
            Dictionary<string, KeyValuePair<UsdSkelAnimation, List<SkinningQueryData>>> animatedSkinningQueryDatas)
        {
            var processedSkeletonPaths = new HashSet<string>();
            var skelRoots = Input.skelRoots;
            if (skelRoots == null || skelRoots.Count == 0)
                return processedSkeletonPaths;

            var cache = new UsdSkelCache();
            foreach (var skelRoot in skelRoots)
            {
                var skelBindings = SkelUtils.GetSkelBindingsFromCache(skelRoot, cache, Input.GraphLogger);
                foreach (var skelBinding in skelBindings)
                {
                    // We build a new instance. Using the one returned by the skel cache would lead to
                    // undetermined behavior once the skel cache has been terminated
                    var skelSkeleton = new UsdSkelSkeleton(skelBinding.GetSkeleton());
                    processedSkeletonPaths.Add(skelSkeleton.GetPath().GetString());

                    var skelQuery = cache.GetSkelQuery(skelSkeleton);
                    if (!skelQuery.IsValid())
                        continue;

                    var skelAnimQuery = skelQuery.GetAnimQuery();
                    if (!skelAnimQuery.IsValid())
                        continue;

                    var skelAnim = new UsdSkelAnimation(skelAnimQuery.GetPrim());
                    var skelAnimPath = skelAnim.GetPath().ToString();
                    if (!animatedBindings.ContainsKey(skelAnimPath))
                    {
                        animatedBindings[skelAnimPath] = new KeyValuePair<UsdSkelAnimation, List<UsdSkelSkeleton>>
                            (
                            skelAnim,
                            new List<UsdSkelSkeleton>()
                            );
                    }
                    animatedBindings[skelAnimPath].Value.Add(skelSkeleton);

                    if (!animatedSkinningQueryDatas.ContainsKey(skelAnimPath))
                    {
                        animatedSkinningQueryDatas[skelAnimPath] = new KeyValuePair<UsdSkelAnimation, List<SkinningQueryData>>
                            (
                            skelAnim,
                            new List<SkinningQueryData>()
                            );
                    }
                    foreach (var skinningQuery in skelBinding.GetSkinningTargetsAsVector())
                    {
                        var skinningQueryData = PopulateSkinningQueryData(skinningQuery, Input.GraphLogger);
                        if (skinningQueryData != null)
                            animatedSkinningQueryDatas[skelAnimPath].Value.Add(skinningQueryData);
                    }
                }
            }

            return processedSkeletonPaths;
        }

        List<UsdSkelSkeleton> PopulateSkelBindingsFromUnskinnedSkeletons(
            Dictionary<string, KeyValuePair<UsdSkelAnimation, List<UsdSkelSkeleton>>> animatedBindings,
            HashSet<string> alreadyProcessedSkeletonPaths)
        {
            var processedSkeletons = new List<UsdSkelSkeleton>();
            var primsWithSkelBindingAPI = Input.primsWithSkelBindingAPI;
            if (primsWithSkelBindingAPI == null || primsWithSkelBindingAPI.Count == 0)
                return processedSkeletons;

            foreach (var primWithSkelBindingAPI in primsWithSkelBindingAPI)
            {
                var skelBindingAPI = new UsdSkelBindingAPI(primWithSkelBindingAPI);
                var skelSkeleton = skelBindingAPI.GetInheritedSkeleton();
                if (!skelSkeleton)
                {
                    // Sometimes the prim having the binding applied is the skeleton prim
                    skelSkeleton = new UsdSkelSkeleton(primWithSkelBindingAPI);
                    if (!skelSkeleton)
                        continue;
                }

                if (alreadyProcessedSkeletonPaths.Contains(skelSkeleton.GetPath().GetString()))
                    continue;

                processedSkeletons.Add(skelSkeleton);
                var skelAnim = new UsdSkelAnimation(skelBindingAPI.GetInheritedAnimationSource());
                if (!skelAnim)
                    continue;

                var skelAnimPath = skelAnim.GetPath().ToString();
                if (!animatedBindings.ContainsKey(skelAnimPath))
                {
                    animatedBindings[skelAnimPath] = new KeyValuePair<UsdSkelAnimation, List<UsdSkelSkeleton>>
                        (
                        skelAnim,
                        new List<UsdSkelSkeleton>()
                        );
                }
                animatedBindings[skelAnimPath].Value.Add(skelSkeleton);
            }

            return processedSkeletons;
        }

        static SkinningQueryData PopulateSkinningQueryData(UsdSkelSkinningQuery skinningQuery, GraphLogger GraphLogger)
        {
            VtTokenArray blendShapesTokens = new VtTokenArray();
            skinningQuery.GetBlendShapeOrder(blendShapesTokens);

            var targetRels = skinningQuery.GetBlendShapeTargetsRel();
            if (!targetRels.IsValid())
                return null;

            var meshPath = skinningQuery.GetPrim().GetPath();
            var targetPaths = targetRels.GetTargets();
            if (blendShapesTokens.size() != targetPaths.Count)
            {
                GraphLogger.LogImportWarning("The number of blend shape tokens does not match the number of blend shape targets. " +
                    $"Blend shape weight animation will not be imported on {meshPath}", null, NodeWarnings.BlendShapeTokenNumberDoNotMatchTargetNumber);
                return null;
            }

            var data = new SkinningQueryData
            {
                meshPath = meshPath,
                blendShapeTokenToTarget = new Dictionary<string, string>()
            };
            for (int i = 0; i < blendShapesTokens.size(); ++i)
            {
                data.blendShapeTokenToTarget[blendShapesTokens[i]] = targetPaths[i];
            }

            return data;
        }
    }
}
