using System;
using System.Collections.Generic;
using pxr;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will read Unity Material info from a list of UsdShadeMaterial prim.
    /// </summary>
    [NodeMetadata("ReadMaterialNode", 6)]
    public class ReadMaterialNode : Node<ReadMaterialNode.InputPort, ReadMaterialNode.OutputPort>
    {
        private static readonly Color UsdPreviewSurfaceDefaultDiffuseColor = new(0.18f, 0.18f, 0.18f, 1.0f);
        private static readonly TfType UsdShadeNodeGraph = TfType.FindByName("UsdShadeNodeGraph");

        /// <summary>
        /// Input ports of the <see cref="ReadMaterialNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// A list of UsdShadeMaterial prims, to be read and converted to <see cref="UsdMaterialDescription"/>.
            /// </summary>
            public List<UsdPrim> usdMaterials;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadMaterialNode"/>.
        /// </summary>
        [Serializable]
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A MaterialPrimPath-<see cref="UsdMaterialDescription"/> mapping resulting from the reading.
            /// Commonly used to create Unity Material from the description.
            /// </summary>
            public Dictionary<string, UsdMaterialDescription> materialDescriptions = new();

            /// <summary>
            /// A diffuse color texture sampler ids hashset resulting from the reading.
            /// Commonly used to extract and import textures used by the materials.
            /// </summary>
            public Dictionary<string, TextureSamplerSettings> diffuseColorTextureSamplers = new();

            /// <summary>
            /// A normal texture sampler ids hashset resulting from the reading.
            /// Commonly used to extract and import textures used by the materials.
            /// </summary>
            public Dictionary<string, TextureSamplerSettings> normalTextureSamplers = new();

            /// <summary>
            /// A roughness texture sampler ids hashset resulting from the reading.
            /// Commonly used to extract and import textures used by the materials.
            /// </summary>
            public Dictionary<string, TextureSamplerSettings> roughnessTextureSamplers = new();

            /// <summary>
            /// A metallic texture sampler ids hashset resulting from the reading.
            /// Commonly used to extract and import textures used by the materials.
            /// </summary>
            public Dictionary<string, TextureSamplerSettings> metallicTextureSamplers = new();

            /// <summary>
            /// An emissive color texture sampler ids hashset resulting from the reading.
            /// Commonly used to extract and import textures used by the materials.
            /// </summary>
            public Dictionary<string, TextureSamplerSettings> emissiveColorTextureSamplers = new();

            /// <summary>
            /// An opacity texture sampler ids hashset resulting from the reading.
            /// Commonly used to extract and import textures used by the materials.
            /// </summary>
            public Dictionary<string, TextureSamplerSettings> opacityTextureSamplers = new();

            /// <summary>
            /// A displacement texture sampler ids hashset resulting from the reading.
            /// Commonly used to extract and import textures used by the materials.
            /// </summary>
            public Dictionary<string, TextureSamplerSettings> displacementTextureSamplers = new();

            /// <summary>
            /// An specular color texture sampler ids hashset resulting from the reading.
            /// Commonly used to extract and import textures used by the materials.
            /// </summary>
            public Dictionary<string, TextureSamplerSettings> specularColorTextureSamplers = new();

            /// <summary>
            /// A occlusion texture sampler ids hashset resulting from the reading.
            /// Commonly used to extract and import textures used by the materials.
            /// </summary>
            public Dictionary<string, TextureSamplerSettings> occlusionTextureSamplers = new();

            /// <summary>
            /// A clear-coat texture sampler ids hashset resulting from the reading.
            /// Commonly used to extract and import textures used by the materials.
            /// </summary>
            public Dictionary<string, TextureSamplerSettings> clearCoatTextureSamplers = new();

            /// <summary>
            /// A clear-coat roughness texture sampler ids hashset resulting from the reading.
            /// Commonly used to extract and import textures used by the materials.
            /// </summary>
            public Dictionary<string, TextureSamplerSettings> clearCoatRoughnessTextureSamplers = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            ComputeMaterialDescription(Input.usdMaterials);
        }

        private void ComputeMaterialDescription(List<UsdPrim> materialPrims)
        {
            foreach (var prim in materialPrims)
            {
                //consider only the master prim path of each material to prevent duplicates
                var masterPrimPath = prim.IsInstanceProxy() ? prim.GetPrimInPrototype().GetPath() : prim.GetPath();
                if (Output.materialDescriptions.ContainsKey(masterPrimPath))
                    continue;

                // retrieve shader connected to surface output
                var material = new UsdShadeMaterial(prim);

                SdfPathVector connections = new SdfPathVector();
                material.GetSurfaceAttr().GetConnections(connections);
                if (connections.Count == 0)
                    continue;

                var shaderPrim = prim.GetPrimAtPath(connections[0].GetPrimPath());
                UsdShadeShader shader = new UsdShadeShader(shaderPrim);

                // Assuming UsdPreviewSurface here.
                if (shaderPrim.IsValid())
                {
                    if (Vt.VtValueToTfToken(shader.GetIdAttr().Get()) == UsdPreviewSurfaceTokens.UsdPreviewSurface)
                    {
                        var description = GetMaterialDescription(shader, prim);
                        Output.materialDescriptions.Add(masterPrimPath, description);
                    }
                }
            }
        }

        private UsdMaterialDescription GetMaterialDescription(UsdShadeShader shader, UsdPrim usdPrim)
        {
            var diffuseInputShader = GetShaderConnectedToInput(shader, UsdPreviewSurfaceTokens.DiffuseColor, out _);
            var emissiveInputShader = GetShaderConnectedToInput(shader, UsdPreviewSurfaceTokens.EmissiveColor, out _);
            var specularInputShader = GetShaderConnectedToInput(shader, UsdPreviewSurfaceTokens.SpecularColor, out _);

            return new UsdMaterialDescription
            {
                name = usdPrim.GetName(),
                roughness = GetFloatInputValue(shader, UsdPreviewSurfaceTokens.Roughness, 0.5f),
                useSpecularWorkflow = GetIntInputValue(shader, UsdPreviewSurfaceTokens.UseSpecularWorkflow, 0),
                metallic = GetFloatInputValue(shader, UsdPreviewSurfaceTokens.Metallic, 0f),
                opacity = GetFloatInputValue(shader, UsdPreviewSurfaceTokens.Opacity, 1f),
                opacityThreshold = GetFloatInputValue(shader, UsdPreviewSurfaceTokens.OpacityThreshold, 0f),
                diffuseColor = GetColorInputValue(shader, UsdPreviewSurfaceTokens.DiffuseColor, UsdPreviewSurfaceDefaultDiffuseColor),
                emissiveColor = GetColorInputValue(shader, UsdPreviewSurfaceTokens.EmissiveColor, Color.black),
                specularColor = GetColorInputValue(shader, UsdPreviewSurfaceTokens.SpecularColor, Color.black),
                clearcoat = GetFloatInputValue(shader, UsdPreviewSurfaceTokens.ClearCoat, 0f),
                clearcoatRoughness = GetFloatInputValue(shader, UsdPreviewSurfaceTokens.ClearCoatRoughness, 0.01f),
                diffuseTextureSampler = GetTextureSamplerSettings(shader, UsdPreviewSurfaceTokens.DiffuseColor, Output.diffuseColorTextureSamplers),
                opacityTextureSampler = GetTextureSamplerSettings(shader, UsdPreviewSurfaceTokens.Opacity, Output.opacityTextureSamplers),
                normalTextureSampler = GetTextureSamplerSettings(shader, UsdPreviewSurfaceTokens.Normal, Output.normalTextureSamplers),
                occlusionTextureSampler = GetTextureSamplerSettings(shader, UsdPreviewSurfaceTokens.Occlusion, Output.occlusionTextureSamplers),
                metallicTextureSampler = GetTextureSamplerSettings(shader, UsdPreviewSurfaceTokens.Metallic, Output.metallicTextureSamplers),
                roughnessTextureSampler = GetTextureSamplerSettings(shader, UsdPreviewSurfaceTokens.Roughness, Output.roughnessTextureSamplers),
                specularColorTextureSampler = GetTextureSamplerSettings(shader, UsdPreviewSurfaceTokens.SpecularColor, Output.specularColorTextureSamplers),
                emissiveColorTextureSampler = GetTextureSamplerSettings(shader, UsdPreviewSurfaceTokens.EmissiveColor, Output.emissiveColorTextureSamplers),
                clearCoatMaskTextureSampler = GetTextureSamplerSettings(shader, UsdPreviewSurfaceTokens.ClearCoat, Output.clearCoatTextureSamplers),
                clearCoatRoughnessTextureSampler = GetTextureSamplerSettings(shader, UsdPreviewSurfaceTokens.ClearCoatRoughness, Output.clearCoatRoughnessTextureSamplers),
                displacementTextureSampler = GetTextureSamplerSettings(shader, UsdPreviewSurfaceTokens.Displacement, Output.displacementTextureSamplers),
                diffusePrimvarName = GetPrimvarName(diffuseInputShader),
                emissivePrimvarName = GetPrimvarName(emissiveInputShader),
                specularPrimvarName = GetPrimvarName(specularInputShader),
            };
        }

        private TextureSamplerSettings GetTextureSamplerSettings(UsdShadeShader materialShader, TfToken textureSlotName, Dictionary<string, TextureSamplerSettings> texturePathsOutput)
        {
            TextureSamplerSettings sampler = new TextureSamplerSettings();
            sampler.samplerId = GetSamplerId(materialShader, textureSlotName);
            var inputShader = GetShaderConnectedToInput(materialShader, textureSlotName, out var textureChannel);

            if (inputShader == null)
                return sampler;

            var inputShaderToken = GetShaderIdToken(inputShader);
            if (ShaderIsTextureReader(inputShaderToken))
            {
                GetTransform2dAndUVPrimvarReaderShaders(inputShader, out var uvPrimvarShader, out var transform2DShader);
                sampler.UVTransform = GetTextureTransform(transform2DShader);
                sampler.texturePath = GetTexturePathFromShader(inputShader);
                sampler.UVName = GetUVName(uvPrimvarShader);
                var vtValue = new VtValue();
                var file = inputShader.GetInput(UsdPreviewSurfaceTokens.file);
                if (file.Get(vtValue))
                {
                    var path = (SdfAssetPath)(vtValue);

                    sampler.texturePath = path.GetResolvedPath();

                    sampler.wrapS = GetWrapModeInputValue(inputShader, UsdPreviewSurfaceTokens.wrapS);
                    sampler.wrapT = GetWrapModeInputValue(inputShader, UsdPreviewSurfaceTokens.wrapT);

                    sampler.textureBias = GetVector4Property(inputShader, UsdPreviewSurfaceTokens.bias, Vector4.zero);
                    sampler.textureScale = GetVector4Property(inputShader, UsdPreviewSurfaceTokens.scale, Vector4.one);
                }

                sampler.textureChannel = textureChannel;
                if (sampler.texturePath != null)
                    texturePathsOutput.Add(sampler.samplerId, sampler);
            }

            return sampler;
        }

        public static string GetSamplerId(UsdShadeShader materialShader, TfToken textureSlotName)
        {
            var usdProp = materialShader.GetInput(textureSlotName);
            if (usdProp == null || !usdProp.IsDefined() || !usdProp.HasConnectedSource())
                return null;

            SdfPathVector inputConnections = new SdfPathVector();
            usdProp.GetRawConnectedSourcePaths(inputConnections);
            if (inputConnections.Count > 0)
            {
                return inputConnections[0].GetAsString();
            }
            return null;
        }

        private static UsdShadeShader GetShaderConnectedToInput(UsdShadeShader shader, TfToken inputName, out string shaderOutputName)
        {
            var usdProp = shader.GetInput(inputName);
            if (usdProp != null && usdProp.IsDefined())
            {
                if (usdProp.HasConnectedSource())
                {
                    SdfPathVector inputConnections = new SdfPathVector();
                    usdProp.GetRawConnectedSourcePaths(inputConnections);
                    if (inputConnections.Count > 0)
                    {
                        var nodePrim = shader.GetPrim().GetPrimAtPath(inputConnections[0].GetPrimPath());
                        var nodeOutputName = SdfPath.StripNamespace(inputConnections[0].GetNameToken());

                        if (nodePrim.IsA(UsdShadeNodeGraph))
                        {
                            var nodeGraph = new UsdShadeNodeGraph(nodePrim);
                            var sourceName = new TfToken();
                            var outputShader = nodeGraph.ComputeOutputSource(new TfToken(nodeOutputName), sourceName, out var _);

                            shaderOutputName = sourceName;
                            return outputShader;
                        }

                        var connectedShader = new UsdShadeShader(nodePrim);
                        shaderOutputName = nodeOutputName;
                        return connectedShader;
                    }
                }
            }

            shaderOutputName = "r";
            return null;
        }

        private static string GetTexturePathFromShader(UsdShadeShader textureShader)
        {
            {
                var file = textureShader.GetInput(UsdPreviewSurfaceTokens.file);
                if (file != null)
                {
                    var vtValue = new VtValue();
                    if (file.Get(vtValue))
                    {
                        var path = (SdfAssetPath)(vtValue);
                        var texturePath = path.GetResolvedPath();
                        return string.IsNullOrEmpty(texturePath) ? null : texturePath;
                    }
                }
            }

            return null;
        }

        private static string GetUVName(UsdShadeShader uvPrimvarShader)
        {
            if (uvPrimvarShader != null)
            {
                return GetPrimvarName(uvPrimvarShader);
            }

            return null;
        }

        private static string GetPrimvarName(UsdShadeShader primvarReaderShader)
        {
            if (primvarReaderShader == null || !ShaderIsPrimvarReader(primvarReaderShader))
                return null;
            var varNameInput = primvarReaderShader.GetInput(UsdPreviewSurfaceTokens.varname);
            VtValue varNameVtValue = null;

            if (varNameInput.HasConnectedSource())
            {
                SdfPathVector inputConnections = new SdfPathVector();
                varNameInput.GetRawConnectedSourcePaths(inputConnections);
                if (inputConnections.Count > 0)
                {
                    var connectedAttribute = primvarReaderShader.GetPrim().GetAttributeAtPath(inputConnections[0]);
                    varNameVtValue = connectedAttribute.Get();
                }
            }
            else
            {
                varNameVtValue = varNameInput.GetAttr().Get();
            }

            if (varNameVtValue != null)
            {
                // This value type is a TfToken in USD versions < 21.11, and a string in 21.11+
                var typeName = varNameVtValue.GetTypeName();
                if (typeName == "string")
                {
                    return varNameVtValue;
                }
                return Vt.VtValueToTfToken(varNameVtValue).GetString();
            }

            return null;
        }

        private TextureUVTransform GetTextureTransform(UsdShadeShader usdTransform2d)
        {
            if (usdTransform2d != null)
            {
                var rotation = GetFloatInputValue(usdTransform2d, UsdPreviewSurfaceTokens.rotation, 0);
                var translation = GetVector2InputValue(usdTransform2d, UsdPreviewSurfaceTokens.translation, Vector2.zero);
                var scale = GetVector2InputValue(usdTransform2d, UsdPreviewSurfaceTokens.scale, Vector2.one);
                return new TextureUVTransform()
                {
                    rotation = -rotation,
                    translation = new Vector2(translation[0], translation[1]),
                    scale = new Vector2(scale[0], scale[1])
                };
            }
            return new TextureUVTransform()
            {
                translation = Vector2.zero,
                scale = Vector2.one,
                rotation = 0
            };
        }

        private static UsdShadeShader GetTransform2dInputShader(UsdShadeShader transform2dShader)
        {
            return GetShaderConnectedToInput(transform2dShader, UsdPreviewSurfaceTokens.inputIn, out _);
        }

        private static UsdShadeShader GetUVSupplierShader(UsdShadeShader textureShader)
        {
            return GetShaderConnectedToInput(textureShader, UsdPreviewSurfaceTokens.st, out _);
        }

        private static TfToken GetShaderIdToken(UsdShadeShader shader)
        {
            var shaderIdToken = new TfToken();
            shader.GetShaderId(shaderIdToken);

            return shaderIdToken;
        }

        private static bool ShaderIsPrimvarReader(TfToken shaderIdToken)
        {
            return shaderIdToken.GetString().StartsWith("UsdPrimvarReader");
        }

        private static bool ShaderIsPrimvarReader(UsdShadeShader shader)
        {
            var shaderIdToken = GetShaderIdToken(shader);

            return shaderIdToken.GetString().StartsWith("UsdPrimvarReader");
        }

        private static bool ShaderIsTextureReader(TfToken shaderIdToken)
        {
            return shaderIdToken == UsdPreviewSurfaceTokens.UsdUVTexture;
        }

        private static bool ShaderIsTransform2D(TfToken shaderIdToken)
        {
            return shaderIdToken == UsdPreviewSurfaceTokens.UsdTransform2d;
        }

        private static void GetTransform2dAndUVPrimvarReaderShaders(UsdShadeShader textureShader, out UsdShadeShader primvarReaderShader, out UsdShadeShader transform2dShader)
        {
            var uvSupplierShader = GetUVSupplierShader(textureShader);

            if (uvSupplierShader != null)
            {
                var uvSupplierShaderIdToken = GetShaderIdToken(uvSupplierShader);
                if (ShaderIsTransform2D(uvSupplierShaderIdToken))
                {
                    transform2dShader = uvSupplierShader;
                    var transform2dInputShader = GetTransform2dInputShader(transform2dShader);

                    primvarReaderShader = ShaderIsPrimvarReader(transform2dInputShader) ? transform2dInputShader : null;
                }
                else if (ShaderIsPrimvarReader(uvSupplierShaderIdToken))
                {
                    transform2dShader = null;
                    primvarReaderShader = uvSupplierShader;
                }
                else
                {
                    transform2dShader = null;
                    primvarReaderShader = null;
                }
            }
            else
            {
                transform2dShader = null;
                primvarReaderShader = null;
            }
        }

        private static float GetFloatInputValue(UsdShadeShader shader, TfToken usdPropertyName, float fallback)
        {
            var vtValue = GetVtValue(shader, usdPropertyName);
            return vtValue != null ? (float)vtValue : fallback;
        }

        private static int GetIntInputValue(UsdShadeShader shader, TfToken usdPropertyName, int fallback)
        {
            var vtValue = GetVtValue(shader, usdPropertyName);
            return vtValue != null ? (int)vtValue : fallback;
        }

        private static VtValue GetVtValue(UsdShadeShader shader, TfToken usdPropertyName)
        {
            var usdProp = shader.GetInput(usdPropertyName);
            if (usdProp != null)
            {
                var vtValue = new VtValue();
                if (usdProp.Get(vtValue))
                {
                    return vtValue;
                }
            }

            return null;
        }

        private static Vector2 GetVector2InputValue(UsdShadeShader shader, TfToken usdPropertyName, Vector2 fallback)
        {
            var usdProp = shader.GetInput(usdPropertyName);
            if (usdProp != null)
            {
                var vtValue = new VtValue();
                if (usdProp.Get(vtValue))
                {
                    var d = (GfVec2f)vtValue;
                    return new Vector2(d[0], d[1]);
                }
            }

            return fallback;
        }

        private static Vector4 GetVector4Property(UsdShadeShader shader, TfToken usdPropertyName, Vector4 fallback)
        {
            var usdProp = shader.GetInput(usdPropertyName);
            if (usdProp != null)
            {
                var vtValue = new VtValue();
                if (usdProp.Get(vtValue))
                {
                    var d = (GfVec4f)vtValue;
                    return new Vector4(d[0], d[1], d[2], d[3]);
                }
            }

            return fallback;
        }

        private static Color GetColorInputValue(UsdShadeShader shader, TfToken usdPropertyName, Color defaultColor)
        {
            var usdProp = shader.GetInput(usdPropertyName);
            if (usdProp != null)
            {
                var vtValue = new VtValue();
                if (usdProp.Get(vtValue))
                {
                    var d = (GfVec3f)vtValue;
                    return new Color(d[0], d[1], d[2]);
                }
            }

            return defaultColor;
        }

        private TextureWrapMode GetWrapModeInputValue(UsdShadeShader shader, TfToken usdPropertyName)
        {
            var usdProp = shader.GetInput(usdPropertyName);
            if (usdProp != null)
            {
                VtValue wrap = new VtValue();
                usdProp.Get(wrap);
                var token = Vt.VtValueToTfToken(wrap);

                if (token == UsdPreviewSurfaceTokens.repeat)
                    return TextureWrapMode.Repeat;
                if (token == UsdPreviewSurfaceTokens.mirror)
                    return TextureWrapMode.Mirror;
            }
            return TextureWrapMode.Clamp;
        }
    }
}
