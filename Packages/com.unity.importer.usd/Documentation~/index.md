# Introduction

OpenUSD (Universal Scene Description) is a format developed by Pixar, intended for the interchange of 3D computer graphics data, collaborative editing, and designed to meet the needs of large-scale film and visual effects production. 

You can [read more about the USD format on Pixar’s website.](https://openusd.org/release/intro.html)

## USD Version
The version of the USD format that this package supports is determined by the version of the **USD Core package** (com.unity.usd.core) that it depends on. To find which USD version you are using, check the USD Core package's description in the Package Manager window. Newer versions of the USD format are currently available, and Unity's USD packages will be periodically updated to benefit from the latest features.

## Supported features

This package provides support for USD import in Unity. USD is a relatively new and rapidly evolving standard. As such, not all features are supported. This first release of the package provides support for most commonly used features, which includes:

- **Transforms**
- **Meshes**
- **Animation**<br/>Including support for the following types of animation:
  - Transform
  - Skeletal
  - Blend shapes
- **Hair**<br/>(See separate [Hair Package instructions](Hair.md))
- **Cameras**<br/>Including camera animation for the following properties:
  - Orthographic size
  - Focal length
  - Focus distance
  - Lens shift
  - Near and far clipping plane distance
- **Lights**<br/>There is limited support for USD lights, due to the discrepancies between lights in USD (which are primarily designed for non-realtime purposes) and Unity lights, which are designed for real-time performance. The supported USD light types are:
  - DistantLight
  - SphereLight
  - DiskLight
  - RectLight
- **Materials** (USD preview surface material only)
- **Textures**

## Understanding the Unity USD Packages

There are multiple USD packages available for Unity which each have different purposes. It's important to understand the differences so that you can use the correct one for your purposes.

- **Unity USD Importer** ([com.unity.importer.usd](https://docs.unity3d.com/Packages/com.unity.importer.usd@latest)) (**_This Package_**)<br/>A new, officially supported package which provides import functionality from USD files into the Unity Editor. Based on the USD Core package. This package is currently considered to be [pre-release](https://docs.unity3d.com/2023.3/Documentation/Manual/pack-preview.html).

- **USD Exporter** ([com.unity.exporter.usd](https://docs.unity3d.com/Packages/com.unity.exporter.usd@latest))<br/>An experimental package which provides export functionality from the Unity Editor to USD files. Based on the USD Core package. This package is in an earlier stage of development than the other packages in this bundle and is currently considered to be [experimental](https://docs.unity3d.com/2023.3/Documentation/Manual/pack-exp.html).

- **USD Core** ([com.unity.usd.core](https://docs.unity3d.com/Packages/com.unity.usd.core@latest))<br/>(Imported automatically when you import the Unity USD Importer or Exporter package). This package provides Unity’s officially supported C# bindings for the underlying C++ USD API, and determines which version of the USD format that Unity supports. To see which version of the USD format is supported, see the USD Core package's description in the Unity Editor's **Package Manager** window. This package is currently considered to be [pre-release](https://docs.unity3d.com/2023.3/Documentation/Manual/pack-preview.html).

- **Legacy USD Experimental Package** ([com.unity.formats.usd](https://docs.unity3d.com/Packages/com.unity.formats.usd@3.0/manual/index.html))<br/>An older experimental package which provided import and export functionality, but has only legacy support, and will soon be deprecated.
Unless you are already using the old experimental package in your project, you should use this new package (com.unity.importer.usd) to import USD assets into your project.


