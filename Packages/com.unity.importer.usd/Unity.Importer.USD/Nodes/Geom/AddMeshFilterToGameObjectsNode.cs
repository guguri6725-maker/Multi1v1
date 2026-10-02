using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will add MeshFilter and MeshRenderer to the GameObjects of the hierarchy.
    /// This supports re-using meshes if USD reference are used in this asset.
    /// </summary>
    [NodeMetadata("AddMeshFilterToGameObjectsNode", 2)]
    public class AddMeshFilterToGameObjectsNode : Node<AddMeshFilterToGameObjectsNode.InputPort, AddMeshFilterToGameObjectsNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="AddMeshFilterToGameObjectsNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// A mapping of mesh prim path to its Unity mesh.
            /// Used to find the mesh associated to a SkinnedMeshRenderer.
            /// </summary>
            public Dictionary<string, Mesh> pathToUnityMesh;

            /// <summary>
            /// A mapping of prim path to their Master prim path, if any.
            /// </summary>
            public Dictionary<string, string> objsToRefs;

            /// <summary>
            /// A collection of invisible prims' paths
            /// </summary>
            public HashSet<string> invisiblePaths;

            /// <summary>
            /// A mapping of xform prim path to their GameObjects in the hierarchy.
            /// Used to add SkinnedMeshRenderers to the right GameObject.
            /// </summary>
            public Dictionary<string, GameObject> gameObjects;
        }

        /// <summary>
        /// Output ports of the <see cref="AddMeshFilterToGameObjectsNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-GameObjects mapping, commonly used when reconstructing the hierarchy.
            /// </summary>
            public Dictionary<string, GameObject> gameObjects;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.gameObjects = new Dictionary<string, GameObject>(Input.gameObjects.Count);
            foreach (var kvp in Input.gameObjects)
            {
                if (Input.objsToRefs.TryGetValue(kvp.Key, out var meshName))
                {
                    if (Input.pathToUnityMesh.TryGetValue(meshName, out var mesh))
                    {
                        kvp.Value.AddComponent<MeshFilter>().sharedMesh = mesh;
                        kvp.Value.AddComponent<MeshRenderer>();

                        if (Input.invisiblePaths.Contains(kvp.Key))
                        {
                            kvp.Value.GetComponent<MeshRenderer>().enabled = false;
                        }
                    }
                }
                Output.gameObjects.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
