# Notes, Limitations and Known Issues

### Unsupported Features

The USD importer does not currently support the following features:

- Per-vertex animation (Geocache / Alembic like vertex streaming)
- Usd Geom intrinsics (UsdGeomCapsule/Cone/Cube/Cylinder/Sphere)
- Draw Modes (origin/bounds/cards/default/inherited)
- Subdivisions (subdivisionScheme)
- UDIM UV mapping
- MaterialX (ASF)
- Volumes (UsdVol)
- PointInstancer 

### No runtime import

The USD importer does not provide functionality to import USD files at runtime, it is an Editor-only feature. This means you cannot use the USD importer package to create apps that themselves import USD files.

### Animation clips are not editable

The current version of the USD importer does not provide settings for editing animation clips (eg. splitting, trimming, etc). The only animation setting available is to control whether the clips loop indefinitely or play once.

### Transform rotation animations do not always have correct interpolation

This is known and will be addressed in a later release.


### All files must be in the Unity project's Assets folder

All USD files to import must be in the Unity project's Assets folder. This includes all referenced files, for multi-file assets.

- USD files with paths pointing anywhere outside of the project's Assets folder will not import and give a warning. Paths should always be relative and be contained in the project. This will remain a restriction but will be handled more gracefully in future releases.

- Files zipped using **usdzip** from the USD Toolset will import normally with only a warning even when paths are invalid or outside the project. This is allowed since the USDZ is meant to be self-contained.

### Handedness is converted from USD (right) to Unity (left) by default

Handedness is converted from right to left, with the same behavior as the Unity FBX importer "Bake Axis" Conversion option. This means that assets that would face the default camera in USDview (Z+) will face away in the Editor (Z-).

### Subdivisions are not supported

When exporting mesh data from a DCC as USD, the subdivisionScheme must be set to "none". Subdivisions are not supported and using [catmullClark/loop/bilinear](https://openusd.org/dev/api/class_usd_geom_mesh.html#a01c7ff0dc2e9b6be9f09db6cfafb7c0a) will not generate normals. In some DCCs e.g. Maya using "none" generates a polygonal mesh with normals. (Note: your DCC may also require additional steps to compute normals for export).

### Only the UsdPreviewSurface Material type is imported

Other material descriptions are not imported.

### Meshes with concave faces may require triangulation in DCC 

This is not specific to the USD importer, but important to be aware of.

### Textures using UDIM UV tiling are not supported

The texture importer doesn’t take UDIM into account.

### Not all lights types are supported

Some lights parameters cannot be mapped to Unity lights. Light intensity is forced to 1 for now. Lux/Lumen values are ignored. 

