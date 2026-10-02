# Installation

This guide describes how to install the USD Importer package for your Unity Project.

## Editor version requirements

The USD Importer package requires the latest version of either Unity 2023.1, 2023.2 or 2023.3. It is not compatible with earlier versions of Unity.

## Installing the package

To install the package, you must use the "Install By Name" option in the Package Manager window:

1. Go to **Window** > **Package Manager** to open the Package Manager window.
2. Click the **Add (+)** dropdown menu, and select "Install package by name..."
3. Enter `com.unity.importer.usd` into the text field that is displayed.
4. Click the **Install** button.

## Dependencies

The USD importer package depends on the **USD Core** package, which provides Unity’s officially supported C# bindings for the underlying USD C++ library. The USD Core package is installed automatically when you install the USD Importer package. The USD Core package determines which version of USD the Unity Editor supports. The USD Importer leverages these bindings to query and interact with the underlying USD data.


## Hair, Fur and Grass

Support for Hair, Fur or Grass (BasisCurves) is supported by an additional package. See [Hair](Hair.md) for more details.