using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will add Camera component to the GameObjects of the hierarchy.
    /// </summary>
    [NodeMetadata("CreateCameraNode", 0)]
    public class CreateCameraNode : Node<CreateCameraNode.InputPort, CreateCameraNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="CreateCameraNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// A PrimPath-CameraDescription mapping, read from the usd file.
            /// </summary>
            public Dictionary<string, CameraDescription> cameras;

            /// <summary>
            /// A PrimPath-GameObject mapping containing all the GameObjects that this node may turn into cameras.
            /// </summary>
            public Dictionary<string, GameObject> gameObjects;

            /// <summary>
            /// USD stage metadata.
            /// </summary>
            public UsdStageMetadata usdMetadata;
        }

        /// <summary>
        /// Output ports of the <see cref="CreateCameraNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-GameObjects mapping, resulting from the conversion.
            /// This mapping will be used when reconstructing the hierarchy.
            /// </summary>
            public Dictionary<string, GameObject> gameObjects;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.gameObjects = Input.gameObjects;
            var rotationOffset = Input.usdMetadata.isStageZup ? Quaternion.Euler(90, 0, 0) : Quaternion.identity;
            foreach (var kvp in Input.gameObjects)
            {
                if (Input.cameras.TryGetValue(kvp.Key, out var cameraData))
                {
                    var rotationOffsetGO = new GameObject();
                    rotationOffsetGO.name = "camera";
                    rotationOffsetGO.transform.rotation = rotationOffset;
                    var camera = rotationOffsetGO.AddComponent<Camera>();
                    ApplyCameraData(camera, cameraData);
                    rotationOffsetGO.transform.SetParent(kvp.Value.transform, false);
                }
                else
                {
                    Input.GraphLogger.LogImportWarning($"Could not find a matching CameraDescription for the GameObject at '{kvp.Key}'", null, NodeWarnings.CouldNotFindMatchingCameraDescription);
                }
            }
        }

        private static void ApplyCameraData(Camera camera, CameraDescription data)
        {
            if (data.projection == CameraDescription.ProjectionType.Orthographic)
            {
                camera.orthographic = true;
                camera.orthographicSize = data.sensorSize.y;
                camera.aspect = data.sensorSize.x / data.sensorSize.y;
            }
            else
            {
                camera.usePhysicalProperties = true;
                camera.gateFit = Camera.GateFitMode.Vertical;
                camera.focalLength = data.focalLength;
                camera.sensorSize = data.sensorSize;
                camera.lensShift = data.lensShift;
                camera.focusDistance = data.focusDistance;
                camera.shutterSpeed = data.shutterSpeed;
                camera.aperture = data.aperture;
            }

            camera.nearClipPlane = data.clipingRange.x;
            camera.farClipPlane = data.clipingRange.y;
        }
    }
}
