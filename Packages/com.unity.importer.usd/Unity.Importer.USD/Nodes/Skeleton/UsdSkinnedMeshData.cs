using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Represents a USD skeleton data.
    /// Used to properly set Weights on <see cref="Mesh"/>> and <see cref="SkinnedMeshRenderer"/> during Mesh conversion.
    /// </summary>
    public struct UsdSkinnedMeshData
    {
        /// <summary>
        /// Is the weight interpolation constant.
        /// </summary>
        public bool isWeightInterpolationConstant;

        /// <summary>
        /// This skeleton joint indices.
        /// </summary>
        public int[] jointIndices;

        /// <summary>
        /// This skeleton joint weights.
        /// </summary>
        public float[] weights;

        /// <summary>
        /// The array size of the original joint weight array in USD.
        /// </summary>
        public int weightsElementSize;

        /// <summary>
        /// The joint binding matrices for this skeleton.
        /// </summary>
        public float4x4[] jointsBindingMatrices;

        /// <summary>
        /// The PrimPath of the skeleton root.
        /// </summary>
        public string skeletonRootPath;

        /// <summary>
        /// A USD mesh may possess a joint remapping that has to be used to make them correspond to the skeleton joint order.
        /// </summary>
        /// <remarks>
        /// - key is the index of the joint as described by the mesh in 'uniform token[] skel:joints'
        /// - value is the corresponding index in the skeleton as described by its 'uniform token[] joints'
        ///
        /// meshToSkeletonJointIndices can be null if no mapping exist or it must at least provide a mapping for each joint used by this mesh.
        /// </remarks>
        public Dictionary<int, int> meshToSkeletonJointIndices;
    }
}
