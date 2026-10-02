using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Unity.Importer.USD
{
    internal static class NodeWarnings
    {
        internal const int BlendShapeTokenNumberDoNotMatchTargetNumber = 1;
        internal const int CouldNotFindHairAsset = 2;
        internal const int HairResampleParticleCountOutOfRange = 3;
        internal const int CouldNotLoadTexture = 4;
        internal const int CouldNotFindZippedTexture = 5;
        internal const int CouldNotFindTexture = 6;
        internal const int CouldNotFindMatchingCameraDescription = 7;
        internal const int InvalidBasisCurveType = 8;
        internal const int InvalidBasisCurveWrap = 9;
        internal const int InvalidBasisCurveWrapForCurveType = 10;
        internal const int InvalidVertexCountForBasisCurve = 11;
        internal const int EmptyOrderForNurbsCurve = 12;
        internal const int EmptyRangeForNurbsCurve = 13;
        internal const int OrderNotMatchingNurbsCurveCount = 14;
        internal const int RangeNotMatchingNurbsCurveCount = 15;
        internal const int EmptyKnotForNurbsCurve = 16;
        internal const int TooFewKnotForNurbsCurve = 17;
        internal const int DecreasingKnotForNurbsCurve = 18;
        internal const int InvalidOrderValueForNurbsCurve = 19;
        internal const int InvalidRangeInterpolationForNurbsCurve = 20;
        internal const int TooLowRangeForNurbsCurve = 21;
        internal const int TooHighRangeForNurbsCurve = 22;
        internal const int EmptyPointsForCurve = 23;
        internal const int InvalidCurve = 24;
        internal const int NonMatchingVertexCountForCurve = 25;
        internal const int MeshJointNotMappingToSkeleton = 26;
        internal const int CouldNotExtractBlendShapeInBetweenWeight = 27;
        internal const int OutOfBoundBlendShapeWeight = 28;
        internal const int DuplicateBlendShapeWeight = 29;
        internal const int CouldNotFindParentGameObject = 30;
        internal const int CouldNotFindParentPrim = 31;
        internal const int CouldNotFindDiskLightDescription = 32;
        internal const int CouldNotFindDistantLightDescription = 33;
        internal const int CouldNotFindRectLightDescription = 34;
        internal const int CouldNotFindSphereLightDescription = 35;
        internal const int GameObjectDoesNotHaveSkinnedMeshRenderer = 36;
        internal const int GameObjectDoesNotHaveRenderer = 37;
        internal const int CouldNotFindTextureForMaterial = 38;
        internal const int WrongInterpolationForDisplayColorOpacity = 39;
        internal const int CouldNotFindBlendShapePrim = 40;
        internal const int CouldNotFindJoint = 41;
        internal const int CouldNotFindBones = 42;
        internal const int CouldNotFindGameObjectForBone = 43;
        internal const int InvalidTimeCodesPerSecond = 44;

        internal static ReadOnlyDictionary<int, string> warningIdToDescriptions = new(new Dictionary<int, string>()
        {
            { BlendShapeTokenNumberDoNotMatchTargetNumber, "The number of blend shape tokens does not match the number of blend shape target" },
            { CouldNotFindHairAsset, "Could not find hair asset" },
            { HairResampleParticleCountOutOfRange, "Hair Resample Particle Count is out of range" },
            { CouldNotLoadTexture, "Could not load texture at path" },
            { CouldNotFindZippedTexture, "Could not find zipped texture at path" },
            { CouldNotFindTexture, "Could not find texture at path" },
            { CouldNotFindMatchingCameraDescription, "Could not find a matching CameraDescription for the GameObject" },
            { InvalidBasisCurveType, "UsdGeomBasisCurves type data is not Cubic or Linear" },
            { InvalidBasisCurveWrap, "UsdGeomBasisCurves wrap data is not one of NonPeriodic, Periodic or Pinned" },
            { InvalidBasisCurveWrapForCurveType, "Bezier or Linear basis curves cannot have a Pinned wrap" },
            { InvalidVertexCountForBasisCurve, "VertexCount is invalid for basis curve" },
            { EmptyOrderForNurbsCurve, "UsdGeomNurbsCurves has an empty or non-existent Order array" },
            { EmptyRangeForNurbsCurve, "UsdGeomNurbsCurves has an empty or non-existent Ranges array" },
            { OrderNotMatchingNurbsCurveCount, "UsdGeomNurbsCurves order data is not the same length as the number of curves" },
            { RangeNotMatchingNurbsCurveCount, "UsdGeomNurbsCurves ranges data is not the same length as the number of curves" },
            { EmptyKnotForNurbsCurve, "UsdGeomNurbsCurves has an empty or non-existent Knots array" },
            { TooFewKnotForNurbsCurve, "UsdGeomNurbsCurves has too few knots" },
            { DecreasingKnotForNurbsCurve, "UsdGeomNurbsCurves knots values must not decrease" },
            { InvalidOrderValueForNurbsCurve, "UsdGeomNurbsCurves has an invalid order value" },
            { InvalidRangeInterpolationForNurbsCurve, "UsdGeomNurbsCurves ranges data has an invalid interpolation" },
            { TooLowRangeForNurbsCurve, "UsdGeomNurbsCurves ranges data has a minimum that is too low" },
            { TooHighRangeForNurbsCurve, "UsdGeomNurbsCurves ranges data has a maximum that is too large" },
            { EmptyPointsForCurve, "UsdGeomCurves has an empty or non-existent Points array" },
            { InvalidCurve, "Invalid UsdGeomCurves" },
            { NonMatchingVertexCountForCurve, "UsdGeomCurves VertexCount sum is different than the count of provided vertices" },
            { MeshJointNotMappingToSkeleton, "Mesh joint does not have mapping to skeleton" },
            { CouldNotExtractBlendShapeInBetweenWeight, "Could not extract weight for blendshape in-between" },
            { OutOfBoundBlendShapeWeight, "Found out-of-bounds weight for blendshape in-between" },
            { DuplicateBlendShapeWeight, "Found duplicated weight for blendshape in-between" },
            { CouldNotFindParentGameObject, "Could not find parentPath when trying to parent the GameObject" },
            { CouldNotFindParentPrim, "Could not find the parent prim path in the usd file" },
            { CouldNotFindDiskLightDescription, "Could not find a matching DiskLightDescription for the GameObject" },
            { CouldNotFindDistantLightDescription, "Could not find a matching DistantLightDescription for the GameObject" },
            { CouldNotFindRectLightDescription, "Could not find a matching RectLightDescription for the GameObject" },
            { CouldNotFindSphereLightDescription, "Could not find a matching SphereLightDescription for the GameObject" },
            { GameObjectDoesNotHaveSkinnedMeshRenderer,  "GameObject does not have a SkinnedMeshRenderer or a MeshFilter" },
            { GameObjectDoesNotHaveRenderer, "GameObject does not have a Renderer" },
            { CouldNotFindTextureForMaterial, "Could not find texture to assign to material" },
            { WrongInterpolationForDisplayColorOpacity, "There was an interpolation error for displayColor and/or displayOpacity for prim" },
            { CouldNotFindBlendShapePrim, "Could not find blendshape prim at path" },
            { CouldNotFindJoint, "Could not find joint when remapping mesh joints" },
            { CouldNotFindBones, "Could not find bones for skeleton root path" },
            { CouldNotFindGameObjectForBone, "Could not find GameObject for bone and skeleton root path" },
            { InvalidTimeCodesPerSecond, "The USD stage timeCodesPerSecond value is invalid" }
        });
    }
}
