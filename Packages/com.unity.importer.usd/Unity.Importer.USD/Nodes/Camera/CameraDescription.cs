using Unity.Mathematics;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Represents a camera.
    /// </summary>
    public struct CameraDescription
    {
        /// <summary>
        /// Enumeration of camera's projection types.
        /// </summary>
        public enum ProjectionType
        {
            /// <summary>
            /// Perspective.
            /// </summary>
            Perspective,
            /// <summary>
            /// Orthographic.
            /// </summary>
            Orthographic
        }
        /// <summary>
        /// Projection type.
        /// </summary>
        public ProjectionType projection;
        /// <summary>
        /// sensor size in millimeters for perspective camera, vertical half-size in meters for orthographic camera.
        /// </summary>
        public float2 sensorSize;
        /// <summary>
        /// Clipping range in meters.
        /// </summary>
        public float2 clipingRange;
        /// <summary>
        /// Lens shift in aperture space. ie : a horizontal value of 1.0f offsets the lens by the width of the sensor.
        /// </summary>
        public float2 lensShift;
        /// <summary>
        /// Focus distance in meters.
        /// </summary>
        public float focusDistance;
        /// <summary>
        /// ShutterSpeed in seconds.
        /// </summary>
        public float shutterSpeed;
        /// <summary>
        /// Focal length in millimeters.
        /// </summary>
        public float focalLength;
        /// <summary>
        /// Aperture number in F-stops.
        /// </summary>
        public float aperture;
    }
}
