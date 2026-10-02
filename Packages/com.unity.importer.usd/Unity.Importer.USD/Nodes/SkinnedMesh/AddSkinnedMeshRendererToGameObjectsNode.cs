using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will add SkinnedMeshRenderer to the GameObjects of the hierarchy.
    /// This supports re-using meshes if USD reference are used in this asset.
    /// </summary>
    [NodeMetadata("AddSkinnedMeshRendererToGameObjectsNode", 2)]
    public class AddSkinnedMeshRendererToGameObjectsNode : Node<AddSkinnedMeshRendererToGameObjectsNode.InputPort, AddSkinnedMeshRendererToGameObjectsNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="AddSkinnedMeshRendererToGameObjectsNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// A mapping of mesh prim path to its Unity mesh.
            /// Used to find the mesh associated to a SkinnedMeshRenderer.
            /// </summary>
            public Dictionary<string, Mesh> pathToUnityMesh;

            /// <summary>
            /// A mapping of mesh prim path to their Master prim path, if any.
            /// Used to handle mesh references.
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
            public Dictionary<string, GameObject> meshGameObjects;

            /// <summary>
            /// A mapping of xform prim path to the skeletons bone GameObjects in the hierarchy.
            /// Used to access GameObjects representing bones or skeleton roots.
            /// </summary>
            public Dictionary<string, GameObject> skeletonGameObjects;

            /// <summary>
            /// A mapping of skeleton root prim path to its skeleton RigDescription.
            /// Used to find the GameObjects composing a skeleton rig and assign them to the SkinnedMeshRenderer.
            /// </summary>
            public Dictionary<string, RigDescription> rigDescriptions;

            /// <summary>
            /// A mapping of mesh prim path to the corresponding UsdSkinnedMeshData.
            /// Used to link a skinned mesh to its skeleton.
            /// </summary>
            public Dictionary<string, UsdSkinnedMeshData> skinnedMeshData;
        }

        /// <summary>
        /// Output ports of the <see cref="AddSkinnedMeshRendererToGameObjectsNode"/>.
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
            Output.gameObjects = new Dictionary<string, GameObject>(Input.meshGameObjects.Count);
            foreach (var kvp in Input.meshGameObjects)
            {
                if (Input.objsToRefs.TryGetValue(kvp.Key, out var meshPrimPath))
                {
                    if (Input.pathToUnityMesh.TryGetValue(meshPrimPath, out var mesh))
                    {
                        var renderer = kvp.Value.AddComponent<SkinnedMeshRenderer>();
                        renderer.sharedMesh = mesh;

                        if (Input.skinnedMeshData.TryGetValue(meshPrimPath, out var data))
                        {
                            BindBones(data, renderer);
                        }

                        if (Input.invisiblePaths.Contains(kvp.Key))
                        {
                            kvp.Value.GetComponent<SkinnedMeshRenderer>().enabled = false;
                        }
                    }
                }
                Output.gameObjects.Add(kvp.Key, kvp.Value);
            }
        }

        private void BindBones(UsdSkinnedMeshData data, SkinnedMeshRenderer renderer)
        {
            var skeletonRootPath = data.skeletonRootPath;
            var bones = ComputeBones(skeletonRootPath);
            renderer.rootBone = Input.skeletonGameObjects[skeletonRootPath].transform;
            renderer.bones = bones.ToArray();
        }

        private List<Transform> ComputeBones(string skeletonRootPath)
        {
            if (!Input.rigDescriptions.ContainsKey(skeletonRootPath))
            {
                Input.GraphLogger.LogImportWarning(
                    $"Could not find bones for skeleton root path '{skeletonRootPath}'.", null,
                    NodeWarnings.CouldNotFindBones);
                return new List<Transform>();
            }

            var rigDescription = Input.rigDescriptions[skeletonRootPath];
            var result = new List<Transform>(rigDescription.jointTransformMatrix.Count);
            foreach (var joint in rigDescription.jointTransformMatrix)
            {
                if (!Input.skeletonGameObjects.ContainsKey(joint.Key))
                {
                    Input.GraphLogger.LogImportWarning(
                        $"Could not find GameObject for bone '{joint.Key}' and skeleton root path '{skeletonRootPath}'.",
                        null, NodeWarnings.CouldNotFindGameObjectForBone);
                    continue;
                }
                result.Add(Input.skeletonGameObjects[joint.Key].transform);
            }

            return result;
        }
    }
}
