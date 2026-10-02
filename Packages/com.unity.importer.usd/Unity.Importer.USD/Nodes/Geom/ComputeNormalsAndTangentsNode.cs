using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;
using UnityEngine.Rendering;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Recompute normals and tangents for mesh that are missing any of them.
    /// </summary>
    [NodeMetadata("ComputeNormalsAndTangentsNode", 0)]
    public class ComputeNormalsAndTangentsNode : Node<ComputeNormalsAndTangentsNode.InputPort, ComputeNormalsAndTangentsNode.OutputPort>
    {
        private static readonly MeshUpdateFlags meshUpdateFlags = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontResetBoneBounds;

        /// <summary>
        /// Input ports of the <see cref="ComputeNormalsAndTangentsNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// A primPath-Mesh mapping containing the mesh of the usd file.
            /// </summary>
            public Dictionary<string, Mesh> pathToUnityMesh;

            /// <summary>
            /// A flag telling if mesh normals should be recalculated if missing.
            /// </summary>
            public bool recalculateNormals;

            /// <summary>
            /// A flag telling if mesh tangents should be recalculated if missing or if normals have been recalculated.
            /// </summary>
            public bool recalculateTangents;
        }

        /// <summary>
        /// Output ports of the <see cref="ComputeNormalsAndTangentsNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A primPath-Mesh mapping containing the mesh of the usd file (same as the input).
            /// </summary>
            public Dictionary<string, Mesh> pathToUnityMesh;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.pathToUnityMesh = Input.pathToUnityMesh;

            if (!Input.recalculateNormals && !Input.recalculateTangents)
                return;

            foreach (var entry in Input.pathToUnityMesh)
            {
                var mesh = entry.Value;
                if (Input.recalculateNormals)
                {
                    mesh.RecalculateNormals(meshUpdateFlags);
                    if (Input.recalculateTangents)
                    {
                        mesh.RecalculateTangents(meshUpdateFlags);
                    }
                }
                else if (Input.recalculateTangents && mesh.HasVertexAttribute(VertexAttribute.Normal))
                {
                    mesh.RecalculateTangents(meshUpdateFlags);
                }
            }
        }
    }
}
