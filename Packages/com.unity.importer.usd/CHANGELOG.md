# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0-pre.2] - 2024-08-23

### Fixed
- Included platforms should match between core and importer  (#222)

## [1.0.0-pre.1] - 2023-12-11

### Changed
- Replaced usage of official USD logo as icons with approved in-house alternatives. (#215)
- Check if folder already exists in build post process to avoid conflict (#218)
- Various documentations updates and revisions (#216)(#217)(#219)(#220)

## [0.5.5] - 2023-12-01

### Changed
- Update analytics event version to v3 to address table partition issue (#213)
- Update README.md with package description (#211)

## [0.5.4] - 2023-11-28

### Added
 - Enabled ci on Ubuntu 22.04 and fixed issue causing Linux CI failures (#207)

### Changed
- Update core package dependency to v0.5.4 (#203)
- Update analytics event and upgrade Analytics API (#209)
- Update mac tests to macos-12 (#205)

### Fixed
- Fixed typo in MetallicUVTranslation (#200)

## [0.5.3] - 2023-11-03

### Changed
- Groom/Hair are now opt-in through second graph (#194)
- Modular importer package merge (#193)
- Update core package dependency to v0.5.2 (#197)
- Prevent Distant light from using Lux/Lumen values incorrectly (#195)

## [0.5.2] - 2023-10-18

### Added
- Added loop animations graph setting (#185)
- Added preserve scene root setting (#190)
- Allow to import assets with more than 4 bone influences (#184)

### Fixed
- Player build fixes (#176)
- Converting constant influence interpolation to vertex interpolation for skinned meshes (#183)
- Fixing the name of the setting edge in the usd importer graph factory (#188)

### Changed
- Optimization: Reduce Regex Scanning (#187)
- Update core package dependency to v0.5.2 (#189)

### Removed

## [0.5.1] - 2023-07-06

### Added
- Add a recalculate normals and tangents node to the default graph and import settings for it (#175)
- Added support for extracting textures from usdz file referenced by the root file (#181)

### Fixed
- Fixed blendshape normals import (#174)
- Fixed mesh bounding box calculation when Up axis is Z (#180)
- Used input orientation in animation keyframes (#178)
- Fixed import of skeleton with missing joint (#182)
- Fixed joint animation with constant transform (#182)
- Do not create skinned mesh for those with joint influence > 4 (#182)

### Changed
- Try to import Display colors by default, if a mesh has no UsdShadeMaterial (#177)

### Removed

## [0.5.0] - 2023-06-27

### Added
- Added a cross-SRP shader graph (#138)
- Added support for multiple uv sets for material (#144)
- Added support for UsdTransform2d texture transforms (#154)
- Use HDRP lit specular material for material with specular workflow mode (#149)
- Added support for blendshapes in-betweens (#152)
- Added nodes for displayColor and displayOpacity import (#157)
- Add a normals and tangents generation optional node (#158)
- Turned PrimVarMapping setting into constant (#153)
- Added support for vertex color in material (#155)
- Fallback to primvar:normals if normals are missing (#161)

### Fixed
- Added a reusable pattern to hold on to managed objects until native code does not need them anymore (#143)
- Fix Nurbs curve sampling and wrong axis conversion with Y Up axis (#139)
- Removed UsdStage.reload workaround and made sure that stage layers are disposed right after usage. (#116)
- Fixed multi-selection UI (#145)
- Support material using UsdShadeNodeGraph (#168)
- Fixed sub-mesh attributes missing and UV layout not rendering in preview (#169)
- Fix leak in MeshConversionUtils when reading meshes Color arrays (#172)

### Changed
- Merged HairResampleParticleCount and HairCurvesSampleDensity settings into HairStrandLengthSetting (#147)
- Do not set default material for USD mesh without material (#146)
- Updated package dependency com.unity.usd.core to 0.5.0

### Removed

## [0.4.0] - 2023-03-31

### Added
- Implement skeletal joint transform animation (#90)
- Implement blend shape weights animation (#96)
- Now importing unbounded skeletons and their animation (#113)
- Added camera Properties animation (#91)
- Handle orthographic cameras animations (#98)
- Implementation of USD Lights conversion. Handles SphereLight, RectLight and DiskLight (#86)
- Enabled textures for URP and HDRP (#106)
- Added missing texture settings (#102)
- Now use jobs for vertex welding (#104)
- Now supporting stages using float metersperunit metadata values (#132)
- Added warning when curves have a wrong VertexCount compared to the provided vertices (#115)
- Added warning when curves have no points (#115)

### Fixed
- Fix joints binding matrices scaling issue (#89)
- Fixed a crash when material connections were using malformed paths (#118)
- Fixed blendshape import crash when they are missing properties (points, normals) (#108)
- Fixing Native leaks in BlendShape and MeshDescription (#105)
- Fixed crashes when managed objects were GC'd while native code still needed their underlying object (#126)
- Fixed wrong placement of Lights (#125)
- Fixed warnings about missing targets in the animation editor when inspecting imported animation (#130)
- Now ignoring references to files containing illegal characters in their paths (#134)
- Now using correct streams for vertex data (#133)
- Fixed conversion of Uniform and Constant Primvars (#131)
- Fix orientation error in hair curves when USD stage Up axis is Y (#128)
- Fix all outstanding documentation warnings (#119)

### Changed
- Updated com.unity.importer minimum version to 0.3.2 (#100)
- Updated com.unity.usd.core to 0.4.0 to get a more recent USD dll and some import fixes (#114).
- Optimized the way animation curves are written to the generated animation clip (#83)
- UsdTimeCode used for import is now EarliestTime instead of Default (#101)
- Vertex welding now in dedicated node (#95)
- Use UsdShadeMaterialBindingAPI to retrieve Mesh subsets and avoid subsets overlaps (#109).
- Handle axis switch in curves (#111).
- Ignore malformed curve in a curve prim while importing the valid ones (#111).
- Disable mesh and skinned mesh renderer for invisible USD mesh (#117).
- Prevent having an hair resampling particle count out of range and cause a system crash (#124).

### Removed
- Removed support for splines and now importing all usd curves as hair (#97).
- Removed incorrectly exposed Create graph option from menu (#127).


## [0.3.0] - 2023-02-02
- Adding support for multi-files dependencies
- Updated minimum Unity version to 2023.1.0b4
- Removed local USD SDK bindings to use com.unity.usd.core instead
- Updated com.unity.importer minimum version to 0.3.1

## [0.2.1] - 2022-10-04

## [0.2.0] - 2022-09-28

## [0.1.0] - 2021-11-24

### This is the first release of *Unity Package com.unity.importer.USD*.
