# OpenUSD Importer
This package provides support for the import of OpenUSD assets into the Unity Editor. The importer is built on the ScriptedImporter API and imports assets stored in USD's data format (with extensions .usd, .usda, .usdc & .usdz) to Unity native representations. The importer uses a deterministic nodegraph to define the steps followed during the import process.

The Importer supports both packaged USDZ files and multi-layer USD scenes composed from multiple files on disk. Imported assets are stored as artifacts in Unity's Asset Database and any changes to the source asset files are tracked and reflected within the Editor.

OpenUSD is a complex and extensible framework for scene description and our OpenUSD Importer package will continue to evolve in tandem with OpenUSD itself.

Currently supported USD version: v23.02.

# More on OpenUSD
OpenUSD (Universal Scene Description) is a format developed by Pixar, intended for the interchange of 3D computer graphics data, collaborative editing, and designed to meet the needs of large-scale film and visual effects production.

You can [read more about the USD format on Pixar’s website.](https://openusd.org/release/intro.html)
