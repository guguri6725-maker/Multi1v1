using System;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Importer;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// This <see cref="Node{TInput,TOutput}"/> combines separate <see cref="Inputs"/> into an actual <see cref="TextureGenerationSettings"/> instance.
    /// </summary>
    /// <remarks>
    /// It would be possible to directly expose a <see cref="TextureGenerationSettings"/> in the <see cref="ImportSetting{T}"/> of an <see cref="ImporterGraph"/>,
    /// but using separate settings makes it possible to override them separately on an asset basis.
    ///
    /// This node output is usually used by a <see cref="CreateTextureNode"/> to generate <see cref="Texture">Textures</see>
    /// using the <see cref="TextureGenerator"/> class during an import.
    /// </remarks>
    /// <seealso cref="TextureImporterSettings"/>
    [NodeMetadata(nameof(CreateTextureGenerationSettingsNode), 0)]
    public class CreateTextureGenerationSettingsNode :
        Node<CreateTextureGenerationSettingsNode.Inputs, CreateTextureGenerationSettingsNode.Outputs>
    {
        /// <summary>
        /// The list of <see cref="TextureGenerationSettings"/> that can be driven from this graph node.
        /// </summary>
        public class Inputs : InputPorts
        {
            /// <summary>
            /// The <see cref="TextureImporterType"/> of the texture.
            /// </summary>
            /// <remarks>
            /// The default value is <see cref="TextureImporterType.Default"/>.
            /// It can be set to <see cref="TextureImporterType.NormalMap"/> to tell the <see cref="TextureGenerator"/> to create a Normal Map Texture.
            /// </remarks>
            /// <seealso cref="TextureImporterSettings.textureType"/>
            public TextureImporterType textureType = TextureImporterType.Default;
            /// <summary>
            /// Tells whether the texture contain color data or other type of data.
            /// </summary>
            /// <remarks>
            /// Textures like normal map, metallic map or roughness map usually don't contain color data and should set this setting to false.
            /// </remarks>
            /// <seealso cref="TextureImporterSettings.sRGBTexture"/>
            public bool sRGB = true;
            /// <summary>
            /// Select how the alpha of the imported texture is generated.
            /// </summary>
            /// <seealso cref="TextureImporterSettings.alphaSource"/>
            public TextureImporterAlphaSource alphaSource = TextureImporterAlphaSource.None;
            /// <summary>
            /// Indicates whether to dilate the color channels.
            /// </summary>
            /// <remarks>
            /// This helps to avoid filtering artifacts on the edges of the alpha channel if the alpha channel represents transparency.
            /// </remarks>
            /// <seealso cref="TextureImporterSettings.alphaIsTransparency"/>
            public bool alphaIsTransparency = false;
            /// <summary>
            /// Specifies how Unity scales the dimension size if the texture source file has a non-power of two (NPOT) dimension size.
            /// </summary>
            /// <seealso cref="TextureImporterSettings.npotScale"/>
            public TextureImporterNPOTScale npotScale;
            /// <summary>
            /// Indicates whether it is possible to access the texture data from scripts using <see cref="Texture2D.SetPixels"/>, <see cref="Texture2D.GetPixels"/> and other <see cref="Texture2D"/> methods.
            /// </summary>
            /// <seealso cref="TextureImporterSettings.readable"/>
            public bool readable = false;
            /// <summary>
            /// Specifies if and how mipmaps have to be generated for the textures.
            /// </summary>
            /// <seealso cref="MipMapSettings"/>
            public MipMapSettings mipMaps;
            /// <summary>
            /// Specifies how the texture behaves when it tiles.
            /// </summary>
            /// <seealso cref="TextureImporterSettings.wrapMode"/>
            public TextureWrapModeSettings wrapMode = new() { all = TextureWrapMode.Repeat };
            /// <summary>
            /// Specifies how Unity filters the texture when the texture stretches during 3D transformations.
            /// </summary>
            /// <seealso cref="TextureImporterSettings.filterMode"/>
            public FilterMode filterMode = FilterMode.Bilinear;
            /// <summary>
            /// Controls the texture quality when you view the texture at a steep angle.
            /// </summary>
            /// <seealso cref="TextureImporterSettings.aniso"/>
            public int anisoLevel = 1;
            /// <summary>
            /// Set the maximum imported Texture dimensions in pixels.
            /// </summary>
            /// <seealso cref="TextureImporterPlatformSettings.maxTextureSize"/>
            public int maxSize = 4096;
            /// <summary>
            /// Choose an algorithm for downscaling the Texture when the Texture dimensions are larger than the specified <see cref="maxSize"/>.
            /// </summary>
            /// <seealso cref="TextureImporterPlatformSettings.resizeAlgorithm"/>
            public TextureResizeAlgorithm resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
            /// <summary>
            /// Select the internal representation format of the texture.
            /// </summary>
            /// <seealso cref="TextureImporterPlatformSettings.format"/>
            public TextureImporterFormat format = TextureImporterFormat.Automatic;
            /// <summary>
            /// Choose the compression type for the Texture.
            /// </summary>
            /// <seealso cref="TextureImporterPlatformSettings.textureCompression"/>
            public TextureImporterCompression compression = TextureImporterCompression.Compressed;
            /// <summary>
            /// Use crunch compression, if applicable.
            /// </summary>
            /// <seealso cref="CrunchedCompression"/>
            public CrunchedCompression crunchedCompression = new()
            {
                useCrunchedCompression = false,
                compressionQuality = TextureCompressionQuality.Normal
            };
            /// <summary>
            /// Allows alpha splitting for this Texture on these platforms: tvOS, iOS, Lumin, and Android.
            /// </summary>
            /// <remarks>
            /// This setting is not exposed in our default graph and is always set to false from an <see cref="ImportConstant{T}"/>.
            /// </remarks>
            /// <seealso cref="TextureImporterPlatformSettings.allowsAlphaSplitting"/>
            public bool allowsAlphaSplitting = false;
            /// <summary>
            /// ETC2 texture decompression fallback override on Android devices that don’t support ETC2.
            /// </summary>
            /// <remarks>
            /// This setting is not exposed in our default graph and is always set to <see cref="AndroidETC2FallbackOverride.Quality32Bit"/> from an <see cref="ImportConstant{T}"/>.
            /// </remarks>
            /// <seealso cref="TextureImporterPlatformSettings.androidETC2FallbackOverride"/>
            public AndroidETC2FallbackOverride androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality32Bit;
            /// <summary>
            /// The mipmap bias of the Texture.
            /// </summary>
            /// <seealso cref="TextureImporterSettings.mipmapBias"/>
            /// <seealso cref="Texture.mipMapBias"/>
            public float mipmapBias;
            /// <summary>
            /// Ignore the Gamma attribute in PNG files. This property does not effect other file formats.
            /// </summary>
            /// <seealso cref="TextureImporterSettings.ignorePngGamma"/>
            public bool ignorePNGGamma = false;
            /// <summary>
            /// Indicates whether to create the normal map from a grayscale heightmap.
            /// </summary>
            /// <seealso cref="NormalMapFromGrayScale"/>
            public NormalMapFromGrayScale createFromGrayScale = new()
            {
                createFromGrayScale = false,
                heightmapScale = 0.25f,
                normalMapFilter = TextureImporterNormalFilter.Standard,
            };
            /// <summary>
            /// Indicates whether to invert the green (Y) channel values of a normal map.
            /// </summary>
            /// <seealso cref="TextureImporterSettings.flipGreenChannel"/>
            public bool flipGreenChannel = false;
        }

        /// <summary>
        /// The output of the node.
        /// </summary>
        public class Outputs : OutputPorts
        {
            /// <summary>
            /// This output setting can be consumed by a <see cref="CreateTextureNode"/> to generate <see cref="Texture"/> during an import.
            /// </summary>
            public TextureGenerationSettings settings;
        }

        /// <inheritdoc cref="Node{TInput,TOutput}.Run"/>
        public override void Run()
        {
            Output.settings = new TextureGenerationSettings(Input.textureType)
            {
                platformSettings =
                {
                    maxTextureSize = Input.maxSize,
                    resizeAlgorithm = Input.resizeAlgorithm,
                    format = Input.format,
                    textureCompression = Input.compression,
                    crunchedCompression = Input.crunchedCompression.useCrunchedCompression,
                    compressionQuality = (int)Input.crunchedCompression.compressionQuality,
                    allowsAlphaSplitting = Input.allowsAlphaSplitting,
                    androidETC2FallbackOverride = Input.androidETC2FallbackOverride
                },
                textureImporterSettings =
                {
                    textureShape = TextureImporterShape.Texture2D,
                    sRGBTexture = Input.sRGB,

                    alphaSource = Input.alphaSource,
                    alphaIsTransparency = Input.alphaIsTransparency,
                    npotScale = Input.npotScale,
                    readable = Input.readable,

                    mipmapEnabled = Input.mipMaps.enabled,
                    mipmapFilter = Input.mipMaps.mode,
                    fadeOut = Input.mipMaps.fadeOut,
                    borderMipmap = Input.mipMaps.border,
                    mipMapsPreserveCoverage = Input.mipMaps.preserveCoverage,
                    mipmapFadeDistanceStart = Input.mipMaps.fadeDistanceStart,
                    mipmapFadeDistanceEnd = Input.mipMaps.fadeDistanceEnd,
                    streamingMipmaps = Input.mipMaps.enableStreaming,
                    streamingMipmapsPriority = Input.mipMaps.streamingPriority,
                    ignoreMipmapLimit = Input.mipMaps.ignoreMipMapLimit,
                    alphaTestReferenceValue = Input.mipMaps.alphaCutoff,

                    wrapModeU = Input.wrapMode.u,
                    wrapModeV = Input.wrapMode.v,
                    wrapModeW = Input.wrapMode.w,
                    filterMode = Input.filterMode,
                    aniso = Input.anisoLevel,
                    mipmapBias = Input.mipmapBias,
                    ignorePngGamma = Input.ignorePNGGamma,

                    convertToNormalMap = Input.createFromGrayScale.createFromGrayScale,
                    heightmapScale = Input.createFromGrayScale.heightmapScale,
                    normalMapFilter = Input.createFromGrayScale.normalMapFilter,
                    flipGreenChannel = Input.flipGreenChannel,
                },
            };
        }
    }

    /// <summary>
    /// Specifies how the texture behaves when it tiles.
    /// </summary>
    /// <seealso cref="TextureImporterSettings.wrapMode"/>
    [Serializable]
    public struct TextureWrapModeSettings
    {
        /// <summary>
        /// Texture U coordinate wrapping mode.
        /// </summary>
        public TextureWrapMode u;
        /// <summary>
        /// Texture V coordinate wrapping mode.
        /// </summary>
        public TextureWrapMode v;
        /// <summary>
        /// Texture W coordinate wrapping mode.
        /// </summary>
        public TextureWrapMode w;

        /// <summary>
        /// Selects U, V, and W coordinate wrapping mode.
        /// </summary>
        public TextureWrapMode all
        {
            get => u;
            set => u = v = w = value;
        }
    }

    /// <summary>
    /// Indicates whether to create the normal map from a grayscale heightmap.
    /// </summary>
    [Serializable]
    public struct NormalMapFromGrayScale
    {
        /// <summary>
        /// True if normal map should be created from a grayscale heightmap.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.convertToNormalMap"/>
        public bool createFromGrayScale;
        /// <summary>
        /// Controls the amount of bumpiness.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.heightmapScale"/>
        public float heightmapScale;
        /// <summary>
        /// Specifies how to calculate the bumpiness.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.normalMapFilter"/>
        public TextureImporterNormalFilter normalMapFilter;
    }

    /// <summary>
    /// Specifies if and how mipmaps have to be generated for the textures.
    /// </summary>
    [Serializable]
    public struct MipMapSettings
    {
        /// <summary>
        /// Indicates whether to generate mipmaps for this texture.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.mipmapEnabled"/>
        public bool enabled;
        /// <summary>
        /// Specifies the method Unity uses to filter mipmaps and optimize image quality.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.filterMode"/>
        public TextureImporterMipFilter mode;
        /// <summary>
        /// Indicates whether mipmaps should fade to gray as the mip levels progress.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.fadeOut"/>
        public bool fadeOut;
        /// <summary>
        /// Indicates whether to stop colors bleeding out to the edge of the lower MIP levels.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.borderMipmap"/>
        public bool border;
        /// <summary>
        /// Indicates whether the alpha channel in generated mipmaps preserves coverage during the alpha text.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.mipMapsPreserveCoverage"/>
        public bool preserveCoverage;
        /// <summary>
        /// The reference value that controls the mipmap coverage during the alpha test.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.alphaTestReferenceValue"/>
        public float alphaCutoff;
        /// <summary>
        /// Mip level where texture begins to fade out to gray.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.mipmapFadeDistanceStart"/>
        public int fadeDistanceStart;
        /// <summary>
        /// Mip level where texture is faded out to gray completely.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.mipmapFadeDistanceEnd"/>
        public int fadeDistanceEnd;
        /// <summary>
        /// Indicates whether to use Mipmap Streaming for this texture.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.streamingMipmaps"/>
        public bool enableStreaming;
        /// <summary>
        /// The priority of the textures in the Mipmap Streaming system.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.streamingMipmapsPriority"/>
        public int streamingPriority;
        /// <summary>
        /// Enable this option to use all mips, regardless of the Mipmap Limit settings in the Quality menu.
        /// </summary>
        /// <seealso cref="TextureImporterSettings.ignoreMipmapLimit"/>
        public bool ignoreMipMapLimit;
    }

    /// <summary>
    /// Use crunch compression, if applicable.
    /// </summary>
    [Serializable]
    public struct CrunchedCompression
    {
        /// <summary>
        /// True if texture should use crunched compression.
        /// </summary>
        /// <seealso cref="TextureImporterPlatformSettings.crunchedCompression"/>
        public bool useCrunchedCompression;
        /// <summary>
        /// Quality of the crunched compression.
        /// </summary>
        /// <seealso cref="TextureImporterPlatformSettings.compressionQuality"/>
        public TextureCompressionQuality compressionQuality;
    }
}
