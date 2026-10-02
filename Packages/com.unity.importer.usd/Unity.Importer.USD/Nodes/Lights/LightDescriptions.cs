using UnityEngine;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Represents a Sphere light.
    /// </summary>
    public struct SphereLightDescription
    {
        /// <summary>
        /// Describes the intensity of the light through physical properties.
        /// </summary>
        public LightBrightnessDescription brightness;
        /// <summary>
        /// Describes the light's color properties.
        /// </summary>
        public LightColorDescription color;
        /// <summary>
        /// Describes the light's cone shape properties.
        /// </summary>
        public LightShapeDescription shape;
        /// <summary>
        /// Whether this light casts shadows or not.
        /// </summary>
        public bool shadowEnabled;
        /// <summary>
        /// Radius of the light.
        /// </summary>
        public float radius;
        /// <summary>
        /// Whether the light is treated as a point or a volume.
        /// </summary>
        public bool treatAsPoint;
    }

    /// <summary>
    /// Represents a Rectangle light.
    /// </summary>
    public struct RectLightDescription
    {
        /// <summary>
        /// Describes the intensity of the light through physical properties.
        /// </summary>
        public LightBrightnessDescription brightness;
        /// <summary>
        /// Describes the light's color properties.
        /// </summary>
        public LightColorDescription color;
        /// <summary>
        /// Describes the light's cone shape properties.
        /// </summary>
        public LightShapeDescription shape;
        /// <summary>
        /// Whether this light casts shadows or not.
        /// </summary>
        public bool shadowEnabled;
        /// <summary>
        /// width of the light.
        /// </summary>
        public float width;
        /// <summary>
        /// height of the light.
        /// </summary>
        public float height;
    }

    /// <summary>
    /// Represents a Disk light.
    /// </summary>
    public struct DiskLightDescription
    {
        /// <summary>
        /// Describes the brightness of the light through physical properties.
        /// </summary>
        public LightBrightnessDescription brightness;
        /// <summary>
        /// Describes the light's cone shape properties.
        /// </summary>
        public LightShapeDescription shape;
        /// <summary>
        /// Describes the light's color properties.
        /// </summary>
        public LightColorDescription color;
        /// <summary>
        /// Whether this light casts shadows or not.
        /// </summary>
        public bool shadowEnabled;
        /// <summary>
        /// radius of the light shape.
        /// </summary>
        public float radius;
    }

    /// <summary>
    /// Describes a Distant light.
    /// </summary>
    public struct DistantLightDescription
    {
        /// <summary>
        /// Describes the intensity of the light through physical properties.
        /// </summary>
        public LightBrightnessDescription brightness;
        /// <summary>
        /// Describes the light's color properties.
        /// </summary>
        public LightColorDescription color;
        /// <summary>
        /// Whether this light casts shadows or not.
        /// </summary>
        public bool shadowEnabled;
    }

    /// <summary>
    /// Describes a Spot light cone.
    /// </summary>
    public struct LightShapeDescription
    {
        /// <summary>
        /// The focus distance
        /// </summary>
        public float focus;

        /// <summary>
        /// The cone angle
        /// </summary>
        public float coneAngle;

        /// <summary>
        /// The cone softness
        /// </summary>
        public float coneSoftness;

        /// <summary>
        /// Whether the cone light is enabled
        /// </summary>
        public bool isEnabled;

        /// <summary>
        /// Applies the described properties the given light
        /// </summary>
        public void ApplyTolight(Light light)
        {
            if (isEnabled)
            {
                light.type = LightType.Spot;
                light.spotAngle = coneAngle * 2.0f;
                light.innerSpotAngle = (1.0f - coneSoftness) * light.spotAngle;
            }
        }
    }

    /// <summary>
    /// Describes the brightness of a light.
    /// </summary>
    public struct LightBrightnessDescription
    {
        /// <summary>
        /// Intensity of the light.
        /// </summary>
        public float intensity;
        /// <summary>
        /// Exposure of the light.
        /// </summary>
        public float exposure;

        /// <summary>
        /// Applies the described properties the given light
        /// </summary>
        public void ApplyTolight(Light light, float lightIntensityMultiplier)
        {
            light.intensity = intensity * lightIntensityMultiplier * Mathf.Pow(2, exposure);
        }
    }

    /// <summary>
    /// Describes the color of a light.
    /// </summary>
    public struct LightColorDescription
    {
        /// <summary>
        /// The color temperature
        /// </summary>
        public float colorTemperature;

        /// <summary>
        /// Whether the light uses color temperature or not.
        /// </summary>
        public bool enableColorTemperature;

        /// <summary>
        /// Color of the light.
        /// </summary>
        public Color color;

        /// <summary>
        /// Applies the described properties the given light
        /// </summary>
        public void ApplyTolight(Light light)
        {
            light.color = color;
            light.useColorTemperature = enableColorTemperature;
            light.colorTemperature = colorTemperature;
        }
    }
}
