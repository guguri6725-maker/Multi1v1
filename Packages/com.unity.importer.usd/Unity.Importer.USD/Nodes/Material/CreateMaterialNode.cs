using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will create Unity Materials using the provided MaterialPrimPath-UsdMaterialDescription.
    /// By default, the shader used to create Materials is 'Autodesk Interactive'.
    /// </summary>
    [NodeMetadata("CreateMaterialNode", 4)]
    public class CreateMaterialNode : Node<CreateMaterialNode.InputPort, CreateMaterialNode.OutputPort>
    {
        // enum values from https://github.cds.internal.unity3d.com/unity/unity/blob/e3f9258dd469b627c8a9ca8acc23fc30f2f7fca2/Packages/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialExtension.cs#L48
        private const float HDRPLitSpecularMaterialId = 4f;
        private const float HDRPLitStandardMaterialId = 1f;

        // enum values from https://github.cds.internal.unity3d.com/unity/unity/blob/e3f9258dd469b627c8a9ca8acc23fc30f2f7fca2/Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Targets/UniversalTarget.cs#L48
        private const float URPSpecularWorkflowMode = 0;
        private const float URPMetallicWorkflowMode = 1f;

        /// <summary>
        /// Input ports of the <see cref="CreateMaterialNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// The shader used to create USDPreviewSurface material
            /// </summary>
            public Shader usdPreviewSurfaceShader;

            /// <summary>
            /// A MaterialPrimPath-<see cref="UsdMaterialDescription"/> mapping used to create the Unity Materials.
            /// </summary>
            public Dictionary<string, UsdMaterialDescription> materialDescriptions;

            /// <summary>
            /// This structure defines the mappings between Primvar names in UsdGeomMesh and the texture coordinate names
            /// </summary>
            public PrimvarToVertexAttributeMapping primVarMapping;
        }

        /// <summary>
        /// Output ports of the <see cref="CreateMaterialNode"/>.
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
            foreach (var description in Input.materialDescriptions)
            {
                Output.materials.Add(description.Key, CreateMaterial(description.Value, Input.usdPreviewSurfaceShader, Input.primVarMapping.VertexAttributeToPrimvarMap));
            }
        }

        private Material CreateMaterial(UsdMaterialDescription description, Shader shader, Dictionary<string, string> vertexAttributeToPrimvarMap)
        {
            var material = new Material(shader);

            material.name = description.name;
            material.SetColor("_DiffuseColor", description.diffuseColor);
            material.SetColor("_EmissiveColor", description.emissiveColor);
            material.SetColor("_SpecularColor", description.specularColor);
            material.SetFloat("_Metallic", description.metallic);
            material.SetFloat("_Roughness", description.roughness);
            material.SetFloat("_Opacity", description.opacity);
            material.SetFloat("_ClearCoatMask", description.clearcoat);
            material.SetFloat("_ClearCoatRoughness", description.clearcoatRoughness);
            material.SetFloat("_Opacity", description.opacity);
            material.SetFloat("_OpacityThreshold", description.opacityThreshold);

            SetMaterialChannelMask(description.metallicTextureSampler.textureChannel, "_MetallicChannelMask", material);
            SetMaterialChannelMask(description.roughnessTextureSampler.textureChannel, "_RoughnessChannelMask", material);
            SetMaterialChannelMask(description.opacityTextureSampler.textureChannel, "_OpacityChannelMask", material);
            SetMaterialChannelMask(description.occlusionTextureSampler.textureChannel, "_OcclusionChannelMask", material);
            SetMaterialChannelMask(description.clearCoatMaskTextureSampler.textureChannel, "_ClearCoatMaskChannelMask", material);
            SetMaterialChannelMask(description.clearCoatRoughnessTextureSampler.textureChannel, "_ClearCoatRoughnessChannelMask", material);

            if (description.diffuseTextureSampler.UVName != null)
            {
                SetUVChannelMask(description.diffuseTextureSampler.UVName, "_DiffuseUVMask", material, vertexAttributeToPrimvarMap);
            }

            if (description.normalTextureSampler.UVName != null)
            {
                SetUVChannelMask(description.normalTextureSampler.UVName, "_NormalUVMask", material, vertexAttributeToPrimvarMap);
            }
            if (description.emissiveColorTextureSampler.UVName != null)
            {
                SetUVChannelMask(description.emissiveColorTextureSampler.UVName, "_EmissiveUVMask", material, vertexAttributeToPrimvarMap);
            }
            if (description.specularColorTextureSampler.UVName != null)
            {
                SetUVChannelMask(description.specularColorTextureSampler.UVName, "_SpecularUVMask", material, vertexAttributeToPrimvarMap);
            }
            if (description.metallicTextureSampler.UVName != null)
            {
                SetUVChannelMask(description.metallicTextureSampler.UVName, "_MetallicUVMask", material, vertexAttributeToPrimvarMap);
            }
            if (description.displacementTextureSampler.UVName != null)
            {
                SetUVChannelMask(description.displacementTextureSampler.UVName, "_DisplacementUVMask", material, vertexAttributeToPrimvarMap);
            }
            if (description.opacityTextureSampler.UVName != null)
            {
                SetUVChannelMask(description.opacityTextureSampler.UVName, "_OpacityUVMask", material, vertexAttributeToPrimvarMap);
            }
            if (description.occlusionTextureSampler.UVName != null)
            {
                SetUVChannelMask(description.occlusionTextureSampler.UVName, "_OcclusionUVMask", material, vertexAttributeToPrimvarMap);
            }
            if (description.roughnessTextureSampler.UVName != null)
            {
                SetUVChannelMask(description.roughnessTextureSampler.UVName, "_RoughnessUVMask", material, vertexAttributeToPrimvarMap);
            }
            if (description.clearCoatMaskTextureSampler.UVName != null)
            {
                SetUVChannelMask(description.clearCoatMaskTextureSampler.UVName, "_ClearCoatMaskUVMask", material, vertexAttributeToPrimvarMap);
            }
            if (description.clearCoatRoughnessTextureSampler.UVName != null)
            {
                SetUVChannelMask(description.clearCoatRoughnessTextureSampler.UVName, "_ClearCoatRoughnessUVMask", material, vertexAttributeToPrimvarMap);
            }

            if (description.diffusePrimvarName != null)
            {
                SetUseVertexColor(description.diffusePrimvarName, "_DiffuseUseVertexColor", material,
                    vertexAttributeToPrimvarMap);
            }
            if (description.emissivePrimvarName != null)
            {
                SetUseVertexColor(description.emissivePrimvarName, "_EmissiveUseVertexColor", material,
                    vertexAttributeToPrimvarMap);
            }
            if (description.specularPrimvarName != null)
            {
                SetUseVertexColor(description.specularPrimvarName, "_SpecularUseVertexColor", material,
                    vertexAttributeToPrimvarMap);
            }

            if (description.useSpecularWorkflow == 1)
            {
                material.SetFloat("_WorkflowMode", URPSpecularWorkflowMode);
                material.SetFloat("_MaterialID", HDRPLitSpecularMaterialId);
            }
            else
            {
                material.SetFloat("_WorkflowMode", URPMetallicWorkflowMode);
                material.SetFloat("_MaterialID", HDRPLitStandardMaterialId);
            }

            if (description.opacityThreshold > 0)
            {
                material.SetFloat("_BUILTIN_AlphaClip", 1.0f);
                material.SetFloat("_AlphaClip", 1f);
            }
            else
            {
                material.SetFloat("_BUILTIN_AlphaClip", 0);
                material.SetFloat("_AlphaClip", 0);
            }

            if ((description.opacityTextureSampler.texturePath != null || description.opacity < 1f) && description.opacityThreshold == 0)
            {
                material.SetFloat("_BUILTIN_Surface", 1f);
                material.SetFloat("_Surface", 1f); // URP
                material.SetFloat("_SurfaceType", 1f); // HDRP
            }

            return material;
        }

        private static void SetMaterialChannelMask(string ChannelValue, string materialChannelMaskName, Material material)
        {
            switch (ChannelValue)
            {
                case "r":
                    material.SetVector(materialChannelMaskName, new Vector4(1f, 0, 0, 0));
                    break;
                case "g":
                    material.SetVector(materialChannelMaskName, new Vector4(0, 1f, 0, 0));
                    break;
                case "b":
                    material.SetVector(materialChannelMaskName, new Vector4(0, 0, 1f, 0));
                    break;
                case "a":
                    material.SetVector(materialChannelMaskName, new Vector4(0, 0, 0, 1f));
                    break;
            }
        }

        private static void SetUVChannelMask(string uvSetValue, string uvChannelMaskName, Material material, Dictionary<string, string> vertexAttributeToPrimvarMap)
        {
            if (vertexAttributeToPrimvarMap.TryGetValue(uvSetValue, out var uvCoord))
            {
                switch (uvCoord)
                {
                    case "TextCoord0":
                        material.SetVector(uvChannelMaskName, new Vector4(1f, 0, 0, 0));
                        break;
                    case "TextCoord1":
                        material.SetVector(uvChannelMaskName, new Vector4(0, 1f, 0, 0));
                        break;
                    case "TextCoord2":
                        material.SetVector(uvChannelMaskName, new Vector4(0, 0, 1f, 0));
                        break;
                    case "TextCoord3":
                        material.SetVector(uvChannelMaskName, new Vector4(0, 0, 0, 1f));
                        break;
                }
            }
        }

        private static void SetUseVertexColor(string primvarName, string usePrimvarName, Material material, Dictionary<string, string> vertexAttributeToPrimvarMap)
        {
            if (vertexAttributeToPrimvarMap.ContainsKey(primvarName))
            {
                material.SetFloat(usePrimvarName, 1f);
            }
        }
    }
}
