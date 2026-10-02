using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will assign the provided Texture2D to their respective Materials.
    /// </summary>
    [NodeMetadata("MapTextureToMaterialNode", 3)]
    public class MapTextureToMaterialNode : Node<MapTextureToMaterialNode.InputPort, MapTextureToMaterialNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="MapTextureToMaterialNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// A MaterialPrimPath-Material mapping containing the material that need to have their texture assigned.
            /// </summary>
            public Dictionary<string, Material> materials;

            /// <summary>
            /// A MaterialPrimPath-<see cref="UsdMaterialDescription"/> mapping that will be used to map textures to their respective materials.
            /// </summary>
            public Dictionary<string, UsdMaterialDescription> materialDescriptions = new();

            /// <summary>
            /// A TextureId-Texture mapping containing the diffuse colortextures that will be assigned to the materials.
            /// </summary>
            public Dictionary<string, Texture> diffuseColorTextures;

            /// <summary>
            /// A TextureId-Texture mapping containing the normal textures that will be assigned to the materials.
            /// </summary>
            public Dictionary<string, Texture> normalTextures;

            /// <summary>
            /// A TextureId-Texture mapping containing the roughness textures that will be assigned to the materials.
            /// </summary>
            public Dictionary<string, Texture> roughnessTextures;

            /// <summary>
            /// A TextureId-Texture mapping containing the metallic textures that will be assigned to the materials.
            /// </summary>
            public Dictionary<string, Texture> metallicTextures;

            /// <summary>
            /// A TextureId-Texture mapping containing the emissive color textures that will be assigned to the materials.
            /// </summary>
            public Dictionary<string, Texture> emissiveColorTextures;

            /// <summary>
            /// A TextureId-Texture mapping containing the opacity textures that will be assigned to the materials.
            /// </summary>
            public Dictionary<string, Texture> opacityTextures;

            /// <summary>
            /// A TextureId-Texture mapping containing the displacement textures that will be assigned to the materials.
            /// </summary>
            public Dictionary<string, Texture> displacementTextures;

            /// <summary>
            /// A TextureId-Texture mapping containing the specular color textures that will be assigned to the materials.
            /// </summary>
            public Dictionary<string, Texture> specularColorTextures;

            /// <summary>
            /// A TextureId-Texture mapping containing the occlusion textures that will be assigned to the materials.
            /// </summary>
            public Dictionary<string, Texture> occlusionTextures;

            /// <summary>
            /// A TextureId-Texture mapping containing the clear-coat textures that will be assigned to the materials.
            /// </summary>
            public Dictionary<string, Texture> clearCoatTextures;

            /// <summary>
            /// A TextureId-Texture mapping containing the clear-coat roughness textures that will be assigned to the materials.
            /// </summary>
            public Dictionary<string, Texture> clearCoatRoughnessTextures;
        }

        /// <summary>
        /// Output ports of the <see cref="MapTextureToMaterialNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A MaterialPrimPath-Material mapping that will be used to map materials to their respective mesh.
            /// </summary>
            public Dictionary<string, Material> materials = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            var materialHashSet = new HashSet<Material>();
            foreach (var materialEntry in Input.materials)
            {
                if (materialHashSet.Contains(materialEntry.Value))
                    continue;

                var mat = materialEntry.Value;
                var description = Input.materialDescriptions[materialEntry.Key];
                MapTextureToMaterial(mat, description);
                materialHashSet.Add(mat);
            }

            Output.materials = Input.materials;
        }

        private void MapTextureToMaterial(Material material, UsdMaterialDescription description)
        {
            if (TryAssignTexture(description.normalTextureSampler.samplerId, material, Input.normalTextures, "_NormalMap"))
            {
                AssignUVTransforms(material, "_Normal", description.normalTextureSampler);
            }
            if (TryAssignTexture(description.opacityTextureSampler.samplerId, material, Input.opacityTextures, "_OpacityMap"))
            {
                AssignUVTransforms(material, "_Opacity", description.opacityTextureSampler);
                AssignTextureScaleAndBias(material, "_Opacity", description.opacityTextureSampler);
            }
            if (TryAssignTexture(description.occlusionTextureSampler.samplerId, material, Input.occlusionTextures, "_OcclusionMap"))
            {
                AssignUVTransforms(material, "_Occlusion", description.occlusionTextureSampler);
                AssignTextureScaleAndBias(material, "_Occlusion", description.occlusionTextureSampler);
            }

            if (TryAssignTexture(description.displacementTextureSampler.samplerId, material, Input.displacementTextures,
                "_DisplacementMap"))
            {
                material.SetFloat("_USEDISPLACEMENTMAP", 1.0f);
            }

            if (TryAssignTexture(description.diffuseTextureSampler.samplerId, material, Input.diffuseColorTextures, "_DiffuseColorMap"))
            {
                material.SetColor("_DiffuseColor", Color.white);
                AssignUVTransforms(material, "_Diffuse", description.diffuseTextureSampler);
                AssignTextureScaleAndBias(material, "_Diffuse", description.diffuseTextureSampler);
            }

            if (TryAssignTexture(description.specularColorTextureSampler.samplerId, material, Input.specularColorTextures,
                "_SpecularColorMap"))
            {
                material.SetColor("_SpecularColor", Color.white);
                AssignUVTransforms(material, "_Specular", description.specularColorTextureSampler);
                AssignTextureScaleAndBias(material, "_Specular", description.specularColorTextureSampler);
            }

            if (TryAssignTexture(description.emissiveColorTextureSampler.samplerId, material, Input.emissiveColorTextures, "_EmissiveColorMap"))
            {
                material.SetColor("_EmissiveColor", Color.white);
                AssignUVTransforms(material, "_Emissive", description.emissiveColorTextureSampler);
                AssignTextureScaleAndBias(material, "_Emissive", description.emissiveColorTextureSampler);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }

            if (TryAssignTexture(description.roughnessTextureSampler.samplerId, material, Input.roughnessTextures, "_RoughnessMap"))
            {
                material.SetFloat("_Roughness", 1.0f);
                AssignUVTransforms(material, "_Roughness", description.roughnessTextureSampler);
                AssignTextureScaleAndBias(material, "_Roughness", description.roughnessTextureSampler);
            }

            if (TryAssignTexture(description.metallicTextureSampler.samplerId, material, Input.metallicTextures, "_MetallicMap"))
            {
                material.SetFloat("_Metallic", 1.0f);
                AssignUVTransforms(material, "_Metallic", description.metallicTextureSampler);
                AssignTextureScaleAndBias(material, "_Metallic", description.metallicTextureSampler);
            }

            if (TryAssignTexture(description.clearCoatMaskTextureSampler.samplerId, material, Input.clearCoatTextures, "_ClearCoatMaskMap"))
            {
                material.SetFloat("_ClearCoatMask", 1.0f);
                AssignUVTransforms(material, "_ClearCoatMask", description.clearCoatMaskTextureSampler);
                AssignTextureScaleAndBias(material, "_ClearCoatMask", description.clearCoatMaskTextureSampler);
            }

            if (TryAssignTexture(description.clearCoatRoughnessTextureSampler.samplerId, material, Input.clearCoatRoughnessTextures, "_ClearCoatRoughnessMap"))
            {
                material.SetFloat("_ClearCoatRoughness", 1.0f);
                AssignUVTransforms(material, "_ClearCoatRoughness", description.clearCoatRoughnessTextureSampler);
                AssignTextureScaleAndBias(material, "_ClearCoatRoughness", description.clearCoatRoughnessTextureSampler);
            }
        }

        private bool TryAssignTexture(string textureId, Material material, Dictionary<string, Texture> textures, string materialTextureName)
        {
            if (string.IsNullOrEmpty(textureId))
                return false;

            if (!textures.ContainsKey(textureId))
            {
                Input.GraphLogger.LogImportWarning(
                    $"Could not find texture '{textureId}' to assign to material '{material.name}'", null,
                    NodeWarnings.CouldNotFindTextureForMaterial);
                return false;
            }

            material.SetTexture(materialTextureName, textures[textureId]);
            return true;
        }

        private static void AssignUVTransforms(Material material, string keywordPrefix, TextureSamplerSettings transform)
        {
            material.SetVector($"{keywordPrefix}UVTranslation", transform.UVTransform.translation);
            material.SetVector($"{keywordPrefix}UVScale", transform.UVTransform.scale);
            material.SetFloat($"{keywordPrefix}UVRotation", transform.UVTransform.rotation);
        }

        private static void AssignTextureScaleAndBias(Material material, string keywordPrefix, TextureSamplerSettings transform)
        {
            material.SetVector($"{keywordPrefix}TextureBias", transform.textureBias);
            material.SetVector($"{keywordPrefix}TextureScale", transform.textureScale);
        }
    }
}
