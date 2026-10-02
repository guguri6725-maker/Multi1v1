# Inspector Settings

This page explains the inspector settings available using the default USD importer graph.

To view the inspector settings for a USD asset, select the USD asset in the Project window. 

Some types of USD assets (`.usd`, `.usda` & `.usdc`) are not imported by default. When these types are present in your project, they display a single **Import** checkbox in the inspector, which allows you to mark them for import. `.usdz` assets are imported by default, because they are self-contained and not part of other multi-file assets, so they do not display the **Import** checkbox.


## For a non-imported USD asset

If a `.usd`, `.usda` or `.usdc` asset is not currently marked for import, its inspector shows only one setting:

| Name | Description |
|---|---|
| Import | Controls whether this asset is imported. Enable this setting if it is a stand-alone asset, or the root of a multi-file asset. Leave this setting disabled if it is imported as part of a multi-file USD asset, where a different file is the root of the composition. | 

## For an imported USD asset

| Name | Description |
|---|---|
| **Import** | Controls whether this asset is imported. (not displayed for `.usdz` assets) |
| **Graph** | Specifies the import graph used to import this asset. The default is **usdImporter (Importer Graph)**. You should change this using the object picker to **usdImporterGroom (Importer Graph)** if your asset requires strand-based hair rendering. |
| **Preserve scene root** | By default, the importer strips away empty root nodes from the hierarchy contained in the USD file. Enable this setting to preserve empty root nodes in the scene. This is useful if you need the hierarchy exactly preserved including empty nodes, for example when mapping skeletal animation from one file onto a hierarchy contained in another file. |
| **Light intensity multiplier** | Applies a multiplier to all lights in the file, allowing you to brighten or dim them during the import process. |
| **Recalculate mesh normals** | Recalculate the normals of meshes from the triangles and vertices. Enable this if the normals in your model are displayed incorrectly. This is useful for debugging, for example if your USD file does not contain normal data. If your files contain normals, enabling this causes the importer to ignore the normals in your file and calculate new ones based only on the position and orientation of the triangles and vertices in your mesh. |
| **Recalculate mesh tangents** | Similar to Recalculate mesh normals. If your USD file does not contain tangent data, you can enable this to automatically calculate tangent data based on the position and orientation of the triangles and vertices in your mesh. Useful for debugging files that do not contain this data. |
| **Loop animations** | Controls whether animation clips imported by the USD importer are set to loop indefinitely or play once. |
| **Textures Alpha Source** | Controls how the alpha channel of the texture is generated. This is None by default. |   |
| | **None** : The imported texture does not have an alpha channel, whether or not the input texture has one. |
| | **From Input** : The alpha channel from the input texture is used, if a texture is provided. |
| | **From Gray Scale** : The alpha channel is generated from the mean (average) of the input texture’s RGB values. |
| **Textures Alpha is transparency** | Allows you to specify that the source texture’s alpha channel is intended to be used as transparency. When enabled, the color channel values are dilated (spread) from the edges of the visible pixels through the fully transparent pixels to avoid filtering artifacts around the edges. |   |
| **Textures NPOT scale** | Controls how textures with a non-power-of-two dimension size should be scaled during import. |   |
|   | **None** : Texture dimensions stay the same. |
|   | **To Nearest** : Scales the texture to the nearest power-of-two dimension size at import time. For example, a 257x511 px texture is scaled to 256x512 px. Note that PVRTC formats require textures to be square (that is width equal to height), so the final dimension size is upscaled to 512x512 px. |
|   | **To Larger** : Scales the texture to the power-of-two dimension size of the largest dimension size value at import time. For example, a 257x511 px texture is scaled to 512x512 px. |
|   | **To Smaller** : Scale the texture to the power-of-two dimension size of the smallest dimension size value at import time. For example, a 257x511 px texture is scaled to 256x256 px. |
| **Textures are Readable** | Controls whether Unity stores an additional copy of the texture pixel data in CPU-addressable memory. Enable this if you want to read, write, and manupulate pixel data on the CPU. For more information, see [Texture.isReadable](https://docs.unity3d.com/ScriptReference/Texture-isReadable.html).|
| **Textures MipMap** | |
|   | **Enabled** : Enabled mip map generation for textures. Mip maps are smaller versions of the texture that are used when the texture is smaller than actual size on screen. See documentation on Importing Textures for more information on mip maps. |
|   | **Mode** |   |
|   | **Box Filter** : The simplest way to fade out mipmaps. The MIP levels become smoother as they go down in dimension size. |
|   | **Kaiser Filter** : A sharpening algorithm runs on the mipmaps as they go down in dimension size. Try this option if your textures are too blurry in the distance. (The algorithm is of the Kaiser Window type; see Wikipedia for further information.) |
|   | **Fade Out** : Enable this property to make the mipmaps fade to gray as the MIP levels progress. This is useful for detail maps, so detail disappears when far away. |
|   | **Border** : Enable this property to avoid colors bleeding out to the edge of the lower MIP levels. Used for light cookies (see below). This property is disabled by default. |
|   | **Preserve Coverage** : Enable this property if you want the alpha channel of generated mipmaps to preserve coverage during the alpha test. This helps transparent textures remain the same average opacity at lower MIP levels. See TextureImporterSettings.mipMapsPreserveCoverage for more information. |
|   | **Alpha Cutoff** : Sets the reference value for controlling the Mip Map coverage during the alpha test. |
|   | **Fade Distance Start** : When mip maps are set to fade out, this setting controls at which MIP level the fade starts. |
|   | **Fade Distance End** : When mip maps are set to fade out, this setting controls at which MIP level the fade ends. |
|   | **Enable Streaming** : Enable this property to use Mip Map Streaming on this texture. This setting is valid for any texture in the 3D environment that Unity displays with a Mesh Renderer. Diffuse textures, normal maps and light maps are all valid for mip map Streaming. |
|   | **Streaming Priority** : Use this to set the priority of the mipmap. Unity uses this to determine which mipmaps to prioritize when assigning resources. Higher values represent a higher priority (for example, 3 is a higher priority than 1). This setting is only available when Streaming Mip Maps is enabled.<br/><br/>The Mip Map Priority number is also a mipmap offset for the Memory Budget (set in the Quality Settings when Texture Streaming is enabled). For example, with a priority of 2, the Mip Map Streaming system tries to use a mipmap two mip levels higher than texture with a priority of 0. One mip level higher is 2x in each axis and two levels higher is 4x in each axis, so two mip levels higher results in a texture 16x larger. If it can’t do this, it uses a lower mip level to fit the Memory Budget. Negative values are also valid. See Mip Map Streaming API for more details. |
|   | **Ignore Mip Map Limit** : Controls whether the importer ignores all texture mipmap limit settings.<br/><br/>Enable this to always import textures at full resolution, disregarding the active quality setting's global texture mipmap limit. |
|   | **Repeat** : The texture repeats, creating a tiled effect. |
|   | **Clamp** : The mapping values are clamped to the range 0-1, giving the effect that the pixels from the nearest edge are repeated. |
|   | **Mirror** : The texture is mirrored back and forth at every integer value, creating a tiled effect where the texture appears backwards then forwards in turn. |
|   | **Mirror Once** : A combination of Mirror and Clamp. The texture is mirrored once, but any mapping values beyond that are clamped, giving the effect that the pixels from the nearest edge are repeated. |
| **Textures Filter mode** | Controls how the pixel colors are sampled from the source texture, when drawn larger on the screen than its original size. The default option is Bilinear. |
|   | **Point** : No filtering is applied. Texture samples are taken from a single point. This results in textures appearing blocky when viewed larger than their original size on screen. |
|   | **Bilinear** : Texture samples are bilinearly interpolated from the nearest four pixel colors, giving the effect of appearing blurred when viewed larger than original size. |
|   | **Trilinear** : In addition to bilinear filtering, this setting provides filtering between the mipmap levels, which hides the transition from one mipmap level to another. Without this, sharp edges between mipmap levels can be visible on planes that recede away from the camera such as long walls or floors.<br/><br/>For textures without mipmaps, this setting is the same as Bilinear.|
| **Textures Aniso level** | Anisotropic filtering increases texture quality when viewing the texture at a steep angle receding away from the camera such as long walls or floors. Increasing this level increases the sharpness at greater distances from the camera, but comes at a performance cost. |
| **Textures max size** | Sets the maximum imported texture dimensions in pixels. Artists often prefer to work with very large source images which need to be scaled down for use in your game or app. This setting allows you to scale down the texture to a suitable dimensions during the import process.|
| **Textures Resize algorithm** | The algorithm for downscaling the texture when its dimensions are larger than the specified max size. |   |
|   | **Mitchell** : Textures are resized using the Mitchell (Bicubic) algorithm. This is the default, and in most cases results in the best quality. |
|   | **Bilinear** : Textures are resized using simpler bilinear sampling, which produces a blurrier but smoother result than Mitchell. |
| **Textures Format** | Allows you to bypass the automatic system and specify the internal representation to use for textures. The list of available formats depends on the platform and texture type. For more information, see Texture formats for platform-specific overrides.<br/><br/>Note: Even when you don’t override a platform, this option shows the format chosen by the automatic system. This property is only available when overriding for a specific platform, and not as a default setting.|
| **Textures Compression** | The compression type for textures. This helps Unity choose the right compression format for a texture. Depending on the platform and the availability of compression formats, different settings might end up with the same internal format. For example, Low Quality Compression affects mobile platforms, but not desktop platforms. |
|   | **Uncompressed** : Textures are not compressed. |
|   | **Compressed** : Textures are compressed in the standard compression format. |
|   | **CompressedHQ** : Textures are compressed in a high-quality format. This might use more memory than Compressed. |
|   | **CompressedLQ** : Textures are compressed in a low-quality format. This might use less memory than Compressed. |
| **Textures Crunched Compression** | Crunch is a lossy compression format on top of DXT or ETC texture compression. Unity decompresses textures to DXT or ETC on the CPU and then uploads them to the GPU at runtime. Crunch compression helps the texture use the lowest possible amount of space on disk and for downloads. Crunch textures can take a long time to compress, but decompression at runtime is very fast. |
|   | **Use Crunched Compression** : Enables the Crunched Compression on textures. |
|   | **Compression Quality** : When using Crunch texture compression, this setting adjusts the quality. A higher compression quality means larger textures and longer compression times.<br/><br/>Note: For Android platforms, the Compresion Quality values provide slightly different options. For more information, see Texture formats for platform-specific overrides. |
| **Textures Mip Bias** | Allows you to adjust the threshold that Unity uses to select a lower or higher mup when sampling. See [Mipmaps intro](https://docs.unity3d.com/Manual/texture-mipmaps-introduction.html) for more information. |
| **Textures (PNG) Ignore Gamma** | Enable this property to ignore the Gamma attribute in PNG files. This property does not effect other file formats. |
| **Normal maps Create From Grayscale** | Settings relating to importing greyscale textures as normal maps. |
|   | **Create From Gray Scale** : Controls whether textures assigned as a normal map in UsdPreviewSurface materials are imported as-is, or converted from a grayscale image to normalmap data. Enable this setting if the normal maps in your USD are greyscale images. Leave this setting disabled if your textures already contain normal map data. |
|   | **Heightmap Scale** : This value is applied as a multiplier to the greyscale amount, which allows you to increase or reduce the effect of the greyscale values when output as normal map data.  |
|   | **Normal Map Filter** : Controls which filtering method is applied when converting greyscale images to normal map data.  |
| **Normal maps Flip Green Channel** | Enable this setting to correct issues where the normal data appears inverted. |
