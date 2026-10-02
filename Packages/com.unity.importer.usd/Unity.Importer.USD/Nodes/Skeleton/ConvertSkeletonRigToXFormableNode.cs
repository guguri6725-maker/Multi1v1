using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will read skeleton <see cref="RigDescription"/> to convert them to <see cref="XFormableDescription"/>.
    /// The result can be used to create skeleton Transform hierarchies.
    /// </summary>
    [NodeMetadata("ConvertSkeletonRigToXFormableNode", 1)]
    public class ConvertSkeletonRigToXFormableNode : BaseMatrixConversionNode<ConvertSkeletonRigToXFormableNode.InputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ConvertSkeletonRigToXFormableNode"/>.
        /// </summary>
        public class InputPort : BaseInputPort
        {
            /// <summary>
            /// A mapping of SkelRoot prim paths to <see cref="RigDescription"/> describing the hierarchy of a skeleton.
            /// </summary>
            public Dictionary<string, RigDescription> rigDescriptions;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            var matrixCount = 0;
            foreach (var rigDescription in Input.rigDescriptions)
            {
                matrixCount += rigDescription.Value.jointTransformMatrix.Count;
            }
            var matrices = new NativeArray<float4x4>(matrixCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var i = 0;
            foreach (var rigDescription in Input.rigDescriptions)
            {
                foreach (var joint in rigDescription.Value.jointTransformMatrix)
                {
                    matrices[i] = joint.Value;
                    i++;
                }
            }

            using var TRSs = new NativeArray<TransformData>(matrixCount, Allocator.TempJob);

            ConvertMatrixToTRS(matrices, TRSs, Input.usdMetadata.isStageZup, Input.usdMetadata.metersPerUnit);

            TRSToXFormableDescriptions(GetRigPaths(TRSs.Length), TRSs);
            matrices.Dispose();
        }

        private string[] GetRigPaths(int boneCount)
        {
            var paths = new string[boneCount];
            var i = 0;
            foreach (var rigDescription in Input.rigDescriptions)
            {
                foreach (var joint in rigDescription.Value.jointTransformMatrix)
                {
                    paths[i] = joint.Key;
                    i++;
                }
            }

            return paths;
        }
    }
}
