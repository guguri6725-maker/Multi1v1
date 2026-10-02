using Unity.Mathematics;
using UnityEngine;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Describes a texture UV transform.
    /// </summary>
    public struct TextureUVTransform
    {
        /// <summary>
        /// Scale transform applied to the uvs of the sampled texture.
        /// </summary>
        public Vector2 scale;
        /// <summary>
        /// translation transform applied to the uvs of the sampled texture.
        /// </summary>
        public Vector2 translation;
        /// <summary>
        /// Rotation transform applied to the uvs of the sampled texture.
        /// </summary>
        public float rotation;
    }

    /// <summary>
    /// Describes the parameters of a texture sampler.
    /// </summary>
    public struct TextureSamplerSettings
    {
        /// <summary>
        /// Identifier of the texture sampler.
        /// </summary>
        public string samplerId;
        /// <summary>
        /// The name of texture coordinate.
        /// </summary>
        public string UVName;
        /// <summary>
        /// file path of the sampled texture.
        /// </summary>
        public string texturePath;
        /// <summary>
        /// Color channel of the sampled texture.
        /// </summary>
        public string textureChannel;

        /// <summary>
        /// UV transform applied to the sampled texture.
        /// </summary>
        public TextureUVTransform UVTransform;
        /// <summary>
        /// Scale to be applied to all components of the texture.
        /// </summary>
        public Vector4 textureScale;
        /// <summary>
        /// Bias to be applied to all components of the texture.
        /// </summary>
        public Vector4 textureBias;
        /// <summary>
        /// Horizontal wrap mode of the texture.
        /// </summary>
        public TextureWrapMode wrapS;
        /// <summary>
        /// Vertical wrap mode of the texture.
        /// </summary>
        public TextureWrapMode wrapT;

        public override int GetHashCode()
        {
            return samplerId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is TextureSamplerSettings other && (samplerId == other.samplerId);
        }
    }
    /// <summary>
    /// Describes the parameters of a specific material.
    /// </summary>
    public struct UsdMaterialDescription
    {
        /// <summary>
        /// Name of the material.
        /// </summary>
        public string name;

        /// <summary>
        /// Roughness of the material.
        /// </summary>
        public float roughness;

        /// <summary>
        /// Metallic factor of the material.
        /// </summary>
        public float metallic;

        /// <summary>
        /// Opacity of the material.
        /// </summary>
        public float opacity;

        /// <summary>
        /// Clear-coat mask of the material.
        /// </summary>
        public float clearcoat;

        /// <summary>
        /// Clear-coat roughness of the material.
        /// </summary>
        public float clearcoatRoughness;

        /// <summary>
        /// Opacity threshold of the material used for alpha clipping, a value of 0 indicates that no alpha clipping is required
        /// </summary>
        public float opacityThreshold;

        /// <summary>
        /// 1 if material uses specular workflow, 0 if material uses metallic workflow
        /// </summary>
        public int useSpecularWorkflow;

        /// <summary>
        /// Diffuse Color of the material.
        /// </summary>
        public Color diffuseColor;

        /// <summary>
        /// Emissive Color of the material.
        /// </summary>
        public Color emissiveColor;

        /// <summary>
        /// Specular Color of the material.
        /// </summary>
        public Color specularColor;

        /// <summary>
        /// The primvar name of diffuse color. Null if no primvar is connected to diffuse color.
        /// </summary>
        public string diffusePrimvarName;

        /// <summary>
        /// The primvar name of emissive color. Null if no primvar is connected to emissive color.
        /// </summary>
        public string emissivePrimvarName;

        /// <summary>
        /// The primvar name of specular color. Null if no primvar is connected to specular color.
        /// </summary>
        public string specularPrimvarName;

        /// <summary>
        /// The texture sampler data for the diffuse texture.
        /// </summary>
        public TextureSamplerSettings diffuseTextureSampler;
        /// <summary>
        /// The texture sampler data for the normal map texture.
        /// </summary>
        public TextureSamplerSettings normalTextureSampler;
        /// <summary>
        /// The texture sampler data for the opacity texture.
        /// </summary>
        public TextureSamplerSettings opacityTextureSampler;
        /// <summary>
        /// The texture sampler data for the occlusion texture.
        /// </summary>
        public TextureSamplerSettings occlusionTextureSampler;
        /// <summary>
        /// The texture sampler data for the specular color texture.
        /// </summary>
        public TextureSamplerSettings specularColorTextureSampler;
        /// <summary>
        /// The texture sampler data for the emissive color texture.
        /// </summary>
        public TextureSamplerSettings emissiveColorTextureSampler;
        /// <summary>
        /// The texture sampler data for the roughness texture.
        /// </summary>
        public TextureSamplerSettings roughnessTextureSampler;
        /// <summary>
        /// The texture sampler data for the metallic texture.
        /// </summary>
        public TextureSamplerSettings metallicTextureSampler;
        /// <summary>
        /// The texture sampler data for the clearcoat mask texture.
        /// </summary>
        public TextureSamplerSettings clearCoatMaskTextureSampler;
        /// <summary>
        /// The texture sampler data for the clearcoat roughness texture.
        /// </summary>
        public TextureSamplerSettings clearCoatRoughnessTextureSampler;
        /// <summary>
        /// The texture sampler data for the displacement texture.
        /// </summary>
        public TextureSamplerSettings displacementTextureSampler;
    }
}
