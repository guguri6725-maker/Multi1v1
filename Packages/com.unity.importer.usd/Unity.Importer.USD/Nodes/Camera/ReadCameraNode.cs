using System;
using System.Collections.Generic;
using pxr;
using Unity.Mathematics;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will read CameraDescriptions info from a list of UsdGeomCamera prim.
    /// </summary>
    [NodeMetadata("ReadCameraNode", 0)]
    public class ReadCameraNode : Node<ReadCameraNode.InputPort, ReadCameraNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadCameraNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// List of USDGeomCamera prims to convert.
            /// </summary>
            public List<UsdPrim> usdCameras;

            /// <summary>
            /// USD stage metadata.
            /// </summary>
            public UsdStageMetadata usdMetadata;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadCameraNode"/>.
        /// </summary>
        [Serializable]
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-CameraDescription mapping containing all the cameras created by this node.
            /// </summary>
            public Dictionary<string, CameraDescription> cameras = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            ComputeCameraData(Input.usdCameras);
        }

        private void ComputeCameraData(List<UsdPrim> cameraPrims)
        {
            foreach (var prim in cameraPrims)
            {
                Output.cameras[prim.GetPath()] = GetCameraData(prim, Input.usdMetadata);
            }
        }

        private CameraDescription GetCameraData(UsdPrim usdPrim, UsdStageMetadata usdMetadata)
        {
            var millimitersPerUnit = usdMetadata.metersPerUnit * 10.0f;
            var usdCamera = new UsdGeomCamera(usdPrim);

            var clippingRange = Vt.VtValueToGfVec2f(usdCamera.GetClippingRangeAttr().Get(UsdTimeCode.EarliestTime()));
            var clippingRangeScaled = new float2(clippingRange[0], clippingRange[1]) * usdMetadata.metersPerUnit;
            var aperture = new float2(usdCamera.GetHorizontalApertureAttr().Get(UsdTimeCode.EarliestTime()), usdCamera.GetVerticalApertureAttr().Get(UsdTimeCode.EarliestTime()));

            var projectionAttr = usdCamera.GetProjectionAttr();
            if (Vt.VtValueToTfToken(projectionAttr.Get(UsdTimeCode.EarliestTime())) == UsdGeomTokens.orthographic)
            {
                var camera = new CameraDescription()
                {
                    projection = CameraDescription.ProjectionType.Orthographic,
                    focalLength = .0f,
                    clipingRange = clippingRangeScaled,
                    sensorSize = aperture * usdMetadata.metersPerUnit * .05f, // Produces correct result with Houdini and Maya exports but does not seem to respect USD specs.
                    lensShift = new float2(.0f, .0f)
                };
                return camera;
            }
            else
            {
                var fstops = Vt.VtValueTofloat(usdCamera.GetFStopAttr().Get(UsdTimeCode.EarliestTime()));
                var shutterSpeed = (float)((usdCamera.GetShutterCloseAttr().Get(UsdTimeCode.EarliestTime()) - usdCamera.GetShutterOpenAttr().Get(UsdTimeCode.EarliestTime())) * usdMetadata.timeCodesPerSecond.GetValue());
                shutterSpeed = math.max(shutterSpeed, 0.05f);
                var apertureOffset = new float2(usdCamera.GetHorizontalApertureOffsetAttr().Get(UsdTimeCode.EarliestTime()), usdCamera.GetVerticalApertureOffsetAttr().Get(UsdTimeCode.EarliestTime()));
                var camera = new CameraDescription()
                {
                    projection = CameraDescription.ProjectionType.Perspective,
                    focalLength = usdCamera.GetFocalLengthAttr().Get() * millimitersPerUnit,
                    clipingRange = clippingRangeScaled,
                    sensorSize = aperture * millimitersPerUnit,
                    lensShift = new float2(
                        apertureOffset.x / aperture.x,
                        apertureOffset.y / aperture.y
                        ),
                    focusDistance = usdCamera.GetFocusDistanceAttr().Get(UsdTimeCode.EarliestTime()) * usdMetadata.metersPerUnit,
                    shutterSpeed = shutterSpeed,
                    aperture = fstops
                };
                return camera;
            }
        }
    }
}
