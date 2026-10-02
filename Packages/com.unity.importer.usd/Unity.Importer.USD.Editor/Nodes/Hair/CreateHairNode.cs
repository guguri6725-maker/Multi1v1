using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Importer;
using UnityEngine;
using Unity.Importer.USD;
using Unity.Jobs;
#if HAIR_0_OR_HIGHER
using Unity.DemoTeam.Hair;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// Generates hair assets from input curve data.
    /// </summary>
    [NodeMetadata("CreateHairNode", 2, description = "Generates hair assets from input curve data", displayName = "CreateHairNode")]
    public class CreateHairNode : Node<CreateHairNode.InputPort, CreateHairNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="CreateHairNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// A PrimPath-CurvesDescription mapping describing the curve points.
            /// </summary>
            public Dictionary<string, CurvesDescription> curveData;

            /// <summary>
            /// The targeted amount of particle for hair re-sampling.
            /// </summary>
            public SamplesPerStrandSetting samplesPerStrand;
        }

        /// <summary>
        /// Output ports of the <see cref="CreateHairNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-HairAsset mapping describing the curve points.
            /// </summary>
            public Dictionary<string, HairAsset> hairAssets;

            /// <summary>
            /// A PrimPath-HairAsset mapping describing the curve points.
            /// </summary>
            public Dictionary<string, Mesh> hairMeshAssetRoots;

            /// <summary>
            /// A PrimPath-HairAsset mapping describing the curve points.
            /// </summary>
            public Dictionary<string, Mesh> hairMeshAssetLines;

            /// <summary>
            /// A PrimPath-HairAsset mapping describing the curve points.
            /// </summary>
            public Dictionary<string, Mesh> hairMeshAssetStrips;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.hairAssets = new Dictionary<string, HairAsset>();

            Output.hairMeshAssetRoots = new Dictionary<string, Mesh>();
            Output.hairMeshAssetLines = new Dictionary<string, Mesh>();
            Output.hairMeshAssetStrips = new Dictionary<string, Mesh>();

            ValidateHairResampleParticleCount(ref Input.samplesPerStrand.value);

            foreach (var kvp in Input.curveData)
            {
                HairAsset hairAsset = GenerateHairAssetFromPoints(kvp.Value);

                //Hair asset generation has errored out, so skip
                if (hairAsset == null)
                    continue;

                hairAsset.name = kvp.Key;

                Output.hairAssets.Add(kvp.Key, hairAsset);

                //Each hair asset references three meshes
                Output.hairMeshAssetRoots.Add(kvp.Key, hairAsset.strandGroups[0].meshAssetRoots);
                Output.hairMeshAssetLines.Add(kvp.Key, hairAsset.strandGroups[0].meshAssetLines);
                Output.hairMeshAssetStrips.Add(kvp.Key, hairAsset.strandGroups[0].meshAssetStrips);
            }
        }

        private void ValidateHairResampleParticleCount(ref int samplesPerStrand)
        {
            var min = SamplesPerStrandSetting.MIN_STRAND_PARTICLE_COUNT;
            var max = SamplesPerStrandSetting.MAX_STRAND_PARTICLE_COUNT;
            if (samplesPerStrand < min)
            {
                Input.GraphLogger.LogImportWarning($"Hair Resample Particle Count of {samplesPerStrand} is out of range {min}-{max}. Setting it to {min}.", null, NodeWarnings.HairResampleParticleCountOutOfRange);
                samplesPerStrand = min;
            }
            else if (samplesPerStrand > max)
            {
                Input.GraphLogger.LogImportWarning($"Hair Resample Particle Count of {samplesPerStrand} is out of range {min}-{max}. Setting it to {max}.", null, NodeWarnings.HairResampleParticleCountOutOfRange);
                samplesPerStrand = max;
            }
        }

        private HairAsset GenerateHairAssetFromPoints(CurvesDescription curves)
        {
            if (curves.curveData.Length == 0 || curves.curveDataHeads.Length == 0)
            {
                Input.GraphLogger.LogImportError("Error in HairFitterNode: Invalid point array! No points are in the input data structure.");
                return null;
            }

            var curveLengths = new NativeArray<int>(curves.curveDataHeads.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            using (curves.curveDataHeads)
            {
                new ConvertCurveHeadToLength(curves.curveDataHeads, curveLengths).Schedule(curves.curveDataHeads.Length - 1, 1).Complete();
                curveLengths[^ 1] = curves.curveData.Length - curves.curveDataHeads[^ 1];
            }

            HairAsset hair = ScriptableObject.CreateInstance<HairAsset>();

            //Creating custom setting for our hair assets. They will use an HairAssetCustomData to which we fed our imported curves.
            HairAsset.SettingsCustom settingsCustom = HairAsset.SettingsCustom.defaults;
            var dataProvider = ScriptableObject.CreateInstance<UsdHairAssetCustomData>();
            dataProvider.curveData = curves.curveData;
            dataProvider.curveLengths = curveLengths;
            settingsCustom.dataProvider = dataProvider;

            hair.settingsBasic = HairAsset.SettingsBasic.defaults;
            hair.settingsCustom = settingsCustom;
            hair.settingsCustom.settingsResolve.resampleParticleCount = Input.samplesPerStrand.value;

            HairAssetBuilder.BuildHairAssetCustom(hair);
            hair.checksum = "strandchecksum";

            dataProvider.Dispose();
            return hair;
        }

        [BurstCompile]
        private struct ConvertCurveHeadToLength : IJobParallelFor
        {
            //Defines where in the curve buffer the i-th curve starts
            [ReadOnly]
            private NativeArray<int> m_CurveBufferHead;

            //Defines the length of a curve in the curve buffer
            private NativeArray<int> m_CurveLengthBuffer;

            public ConvertCurveHeadToLength(NativeArray<int> curveBufferHead, NativeArray<int> curveLengthBuffer)
            {
                m_CurveBufferHead = curveBufferHead;
                m_CurveLengthBuffer = curveLengthBuffer;
            }

            public void Execute(int currentCurve)
            {
                m_CurveLengthBuffer[currentCurve] = m_CurveBufferHead[currentCurve + 1] - m_CurveBufferHead[currentCurve];
            }
        }
    }
}
#endif
