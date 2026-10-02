using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will assign the provided Unity Materials to their corresponding MeshFilter.
    /// </summary>
    [NodeMetadata("MapMaterialToMeshNode", 2)]
    public class MapMaterialToMeshNode : Node<MapMaterialToMeshNode.InputPort, MapMaterialToMeshNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="MapMaterialToMeshNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// A MaterialPrimPath-Material mapping containing the material that need to be assigned to mesh filters.
            /// </summary>
            public Dictionary<string, Material> materials;

            /// <summary>
            /// A MeshPrimPath-GameObject mapping containing the GameObjects with the mesh filter components.
            /// </summary>
            public Dictionary<string, GameObject> meshGameobjects;

            /// <summary>
            /// A MeshPrimPath-MaterialPrimPaths mapping to be used to map materials to their respective meshes.
            /// </summary>
            public Dictionary<string, List<string>> meshMaterialsPaths;

            /// <summary>
            /// An optional default material to be set when one cannot be found for a mesh.
            /// </summary>
            public Material defaultMaterial = null;
        }

        /// <summary>
        /// Output ports of the <see cref="MapMaterialToMeshNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// The resulting MeshPrimPath-GameObject mapping (Mesh filters with materials assigned).
            /// </summary>
            public Dictionary<string, GameObject> meshGameObjects;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.meshGameObjects = new Dictionary<string, GameObject>(Input.meshGameobjects.Count);
            foreach (var inputMeshGameObject in Input.meshGameobjects)
            {
                AssignMaterials(inputMeshGameObject.Key, inputMeshGameObject.Value);
                Output.meshGameObjects.Add(inputMeshGameObject.Key, inputMeshGameObject.Value);
            }
        }

        private void AssignMaterials(string meshPrimPath, GameObject meshGO)
        {
            var meshRenderer = meshGO.GetComponent<Renderer>();
            if (meshRenderer == null)
            {
                Input.GraphLogger.LogImportWarning(
                    $"GameObject '{meshGO.name}' does not have a {nameof(Renderer)}. Material won't be assigned.", null,
                    NodeWarnings.GameObjectDoesNotHaveRenderer);
            }

            var meshFilter = meshGO.GetComponent<MeshFilter>();
            Mesh mesh;

            if (meshRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                mesh = skinnedMeshRenderer.sharedMesh;
            }
            else if (meshFilter != null)
            {
                mesh = meshFilter.sharedMesh;
            }
            else
            {
                Input.GraphLogger.LogImportWarning(
                    $"GameObject '{meshGO.name}' does not have a {nameof(SkinnedMeshRenderer)} or a {nameof(MeshFilter)}. Material won't be assigned.",
                    null, NodeWarnings.GameObjectDoesNotHaveSkinnedMeshRenderer);
                return;
            }

            meshRenderer.sharedMaterials = BuildMaterialArray(meshPrimPath, mesh);
        }

        private Material[] BuildMaterialArray(string meshPrimPath, Mesh mesh)
        {
            var materials = new Material[mesh.subMeshCount];

            if (!Input.meshMaterialsPaths.TryGetValue(meshPrimPath, out var meshMaterialPaths))
            {
                meshMaterialPaths = new List<string>(0);
            }

            for (var index = 0; index < materials.Length; index++)
            {
                if (index >= meshMaterialPaths.Count)
                {
                    materials[index] = Input.defaultMaterial;
                }
                else if (Input.materials.TryGetValue(meshMaterialPaths[index], out var mat))
                {
                    materials[index] = mat;
                }
                else
                {
                    materials[index] = Input.defaultMaterial;
                }
            }

            return materials;
        }
    }
}
