using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will create Unity Materials using the provided MaterialPrimPath-UsdMaterialDescription.
    /// By default, the shader used to create Materials is 'usdPreviewSurfaceShader'.
    /// </summary>
    [NodeMetadata("CreateMaterialFromMeshColorNode", 0, description = "Create Unity Materials using the provided MaterialPrimPath-UsdMaterialDescription", displayName = "CreateMaterialFromMeshColor")]
    public class CreateMaterialFromMeshColorNode : Node<CreateMaterialFromMeshColorNode.InputPort, CreateMaterialFromMeshColorNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="CreateMaterialFromMeshColorNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// The shader used to create USDPreviewSurface material
            /// </summary>
            public Shader usdPreviewSurfaceShader;

            /// <summary>
            /// A MaterialPrimPath-DisplayColorMaterialDescription mapping containing all the material data needed to create the materials.
            /// </summary>
            public Dictionary<string, DisplayColorMaterialDescription> displayColorMaterialDescriptions = new();
        }

        /// <summary>
        /// Output ports of the <see cref="CreateMaterialFromMeshColorNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A MaterialPrimPath-Material mapping containing all the materials created by this node.
            /// </summary>
            public Dictionary<string, Material> materials = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            foreach (var description in Input.displayColorMaterialDescriptions)
            {
                Output.materials.Add(description.Key, CreateMaterial(description.Value, Input.usdPreviewSurfaceShader));
            }
        }

        private Material CreateMaterial(DisplayColorMaterialDescription description, Shader shader)
        {
            var material = new Material(shader)
            {
                name = description.name
            };

            material.SetColor("_DiffuseColor", description.color);
            material.SetFloat("_Opacity", description.opacity);

            var transparencyFlag = description.opacity < 1f ? 1f : 0f;
            material.SetFloat("_BUILTIN_Surface", transparencyFlag);
            material.SetFloat("_Surface", transparencyFlag); // URP
            material.SetFloat("_SurfaceType", transparencyFlag); // HDRP

            return material;
        }
    }
}
