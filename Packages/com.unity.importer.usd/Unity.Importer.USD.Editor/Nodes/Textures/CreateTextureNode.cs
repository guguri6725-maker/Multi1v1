using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using pxr;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Importer;
using UnityEditor.AssetImporters;
using Unity.Importer.USD;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// This node will create Unity Texture from the texture paths provided (usually extracted from material usage in the usd file).
    /// It currently supports extracting textures from pgn, jpg, and exr file formats.
    /// </summary>
    [NodeMetadata("CreateTextureNode", 2)]
    public class CreateTextureNode : Node<CreateTextureNode.InputPort, CreateTextureNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="CreateTextureNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// Settings to apply to textures.
            /// - <see cref="TextureGenerationSettings.textureImporterSettings.textureType"/> should be set to <see cref="TextureImporterType.Default"/> for most texture usage,
            /// except for normal maps that should use <see cref="TextureImporterType.NormalMap"/>.
            /// - <see cref="TextureGenerationSettings.textureImporterSettings.sRGBTexture"/> should be set to true for Textures used for Color map or Emissive Color map,
            /// and false for other Texture usages that aren't using the texture value for output colors but fot their data (like height maps, metallic our roughness textures).
            /// - Other settings can be set to limit texture size, generate mipmaps and choose specific compressions.
            /// </summary>
            public TextureGenerationSettings textureSettings = new TextureGenerationSettings(TextureImporterType.Default);

            /// <summary>
            /// textures samplers by ids used to extract and import textures referenced by the materials.
            /// </summary>
            public Dictionary<string, TextureSamplerSettings> textureSamplers;
        }

        /// <summary>
        /// Output ports of the <see cref="CreateTextureNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A TextureId-Texture mapping containing the created textures. Commonly used to assign textures to materials.
            /// </summary>
            public Dictionary<string, Texture> texturesBySamplerId = new();
            /// <summary>
            /// A Texture path-Texture mapping containing the created textures. Commonly used to output textures.
            /// </summary>
            public Dictionary<string, Texture> texturesByPath = new();
        }

        private const string k_ZippedTextureRegex = @"(.+)\[(.+)\]$";
        private const string k_UsdZExtension = ".usdz";

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            ExtractTextures();
        }

        /// <summary>
        /// Extract textures from the main file and any other ones in the usd composition.
        /// If a texture is in an usdz file, we will use the UsdZ api provided by the usd foundation team and try to unzip it.
        /// </summary>
        private void ExtractTextures()
        {
            var zipFileCache = new Dictionary<string, UsdZipFileCacheEntry>();
            var zipRegex = new Regex(k_ZippedTextureRegex);

            foreach (var kvp in Input.textureSamplers)
            {
                if (!Output.texturesByPath.TryGetValue(kvp.Value.texturePath, out var texture))
                {
                    var sampler = kvp.Value;

                    if (sampler.texturePath.EndsWith("]"))
                    {
                        var match = zipRegex.Match(sampler.texturePath);

                        // Texture is in a usdz file
                        if (match.Success)
                        {
                            var filePath = match.Groups[1].Value;
                            if (!zipFileCache.ContainsKey(filePath))
                            {
                                zipFileCache.Add(filePath, new UsdZipFileCacheEntry(filePath));
                            }

                            texture = LoadTextureFromUsdZ(zipFileCache[filePath], sampler, match.Groups[2].Value);
                        }
                    }
                    else
                    {
                        texture = LoadTextureFromFile(sampler);
                    }

                    Output.texturesByPath.Add(kvp.Value.texturePath, texture);
                }
                AddTextureToOutput(texture, kvp.Value);
            }

            foreach (var entry in zipFileCache)
            {
                entry.Value.Dispose();
            }
        }

        private void AddTextureToOutput(Texture texture, TextureSamplerSettings sampler)
        {
            if (texture != null)
            {
                Output.texturesBySamplerId.Add(sampler.samplerId, texture);
            }
            else
            {
                Input.GraphLogger.LogImportWarning($"Could not load texture at path : {sampler.texturePath}. Texture is invalid or not of a supported format (jpg, png, or exr).", null, NodeWarnings.CouldNotLoadTexture);
            }
        }

        /// <summary>
        ///  Will use the UsdZ api from the usd package to extract a byte[] of our texture from the UsdZ file and feed it to CreateTexture.
        /// </summary>
        private Texture LoadTextureFromUsdZ(UsdZipFileCacheEntry usdZipFile, TextureSamplerSettings sampler, string zipPath)
        {
            if (!usdZipFile.zippedFileNames.Contains(zipPath))
            {
                Input.GraphLogger.LogImportWarning($"Could not find zipped texture at path : {sampler.texturePath}", null, NodeWarnings.CouldNotFindZippedTexture);
                return null;
            }

            var filePtr = usdZipFile.zipFile.GetFile(zipPath, out var size);
            var managedArray = new byte[size];
            Marshal.Copy(filePtr, managedArray, 0, size);
            return CreateTexture(Path.GetFileNameWithoutExtension(zipPath), managedArray);
        }

        /// <summary>
        /// We're fetching the texture file, reading its content and feed it to CreateTexture.
        /// </summary>
        private Texture LoadTextureFromFile(TextureSamplerSettings sampler)
        {
            if (!File.Exists(sampler.texturePath))
            {
                Input.GraphLogger.LogImportWarning($"Could not find texture at path : {sampler.texturePath}", null, NodeWarnings.CouldNotFindTexture);
                return null;
            }

            var fileData = File.ReadAllBytes(sampler.texturePath);
            return CreateTexture(Path.GetFileNameWithoutExtension(sampler.texturePath), fileData);
        }

        /// <summary>
        /// Uses 'texture.LoadImage' to extract the Color32 array from the texture file and feed the array to 'TextureGenerator.GenerateTexture'
        /// </summary>
        private Texture CreateTexture(string textureName, byte[] textureRawData)
        {
            var texture = new Texture2D(1, 1);
            if (!texture.LoadImage(textureRawData))
                return null;

            //We are not using 'texture.GetRawTextureData<Color32>()' because the Color32 conversion can lead to swapped channels
            //(if texture is ARGB instead of RGBA like Color32)
            using var natColor32 = new NativeArray<Color32>(texture.GetPixels32(), Allocator.Temp);
            var importSetting = Input.textureSettings;
            importSetting.sourceTextureInformation = new SourceTextureInformation
            {
                width = texture.width,
                height = texture.height
            };

            // Do not remove this line. We do not want AssetPostprocessors to have side effects
            // on the USD importer or any importer that would use this Node.
            importSetting.enablePostProcessor = false;
            var textureGeneration = TextureGenerator.GenerateTexture(importSetting, natColor32);
            textureGeneration.output.name = textureName;
            return textureGeneration.output;
        }

        private class UsdZipFileCacheEntry : IDisposable
        {
            public UsdZipFile zipFile;
            public HashSet<string> zippedFileNames;

            public UsdZipFileCacheEntry(string assetPath)
            {
                zipFile = UsdZipFile.Open(assetPath);
                zippedFileNames = new HashSet<string>(zipFile.GetFileNames());
            }

            public void Dispose()
            {
                zipFile?.Dispose();
            }
        }
    }
}
