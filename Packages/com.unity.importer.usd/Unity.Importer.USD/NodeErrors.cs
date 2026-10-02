using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Unity.Importer.USD
{
    internal static class NodeErrors
    {
        internal const int Exception = 0;
        internal const int AnalyticsDataCastingError = 1;
        internal const int ReadMeshError = 2;
        internal const int ReadSkinnedMeshError = 3;
        internal const int InvalidJointIndices = 4;
        internal const int InvalidJointWeights = 5;
        internal const int ReferencedLayerNotInUnityProject = 6;
        internal const int CouldNotPopulateSkeletonCache = 7;
        internal const int CouldNotComputeBindingForSkelRoot = 8;
        internal const int UnsupportedJointInfluenceNumber = 9;

        internal static ReadOnlyDictionary<int, string> errorIdToDescriptions = new(new Dictionary<int, string>
        {
            { Exception, "Exception occured during import" },
            { AnalyticsDataCastingError, "Analytics data not of expected type" },
            { ReadMeshError, "Mesh Reading Error" },
            { ReadSkinnedMeshError, "Skinned Mesh Reading Error" },
            { InvalidJointIndices, "Joints indices data are invalid or empty for skinned mesh and SkelRoot" },
            { InvalidJointWeights, "Joints weights data are invalid or empty for skinned mesh and SkelRoot" },
            { ReferencedLayerNotInUnityProject, "Referenced layer is not in Unity project" },
            { CouldNotPopulateSkeletonCache, "Failed to populate skeleton cache with SkelRoot" },
            { CouldNotComputeBindingForSkelRoot, "Could not compute binding for SkelRoot" },
            { UnsupportedJointInfluenceNumber, "Unsupported joint influence number" },
        });
    }
}
