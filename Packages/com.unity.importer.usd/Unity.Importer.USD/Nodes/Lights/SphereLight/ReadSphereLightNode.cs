using System;
using System.Collections.Generic;
using pxr;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will read SphereLightDescription info from a list of UsdLuxSphereLight prims.
    /// </summary>
    [NodeMetadata("ReadSphereLightNode", 0)]
    public class ReadSphereLightNode : Node<ReadSphereLightNode.InputPort, ReadSphereLightNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadSphereLightNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// List of UsdLuxSphereLight prims to convert.
            /// </summary>
            public List<UsdPrim> usdSphereLights;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadSphereLightNode"/>.
        /// </summary>
        [Serializable]
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-SphereLightDescription mapping containing all the lights created by this node.
            /// </summary>
            public Dictionary<string, SphereLightDescription> sphereLights = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            ComputeSphereLightData(Input.usdSphereLights);
        }

        private void ComputeSphereLightData(List<UsdPrim> lightPrims)
        {
            foreach (var prim in lightPrims)
            {
                Output.sphereLights[prim.GetPath()] = GetSphereLightData(prim);
            }
        }

        private SphereLightDescription GetSphereLightData(UsdPrim usdPrim)
        {
            var usdLight = new UsdLuxLightAPI(usdPrim);
            var usdShadow = new UsdLuxShadowAPI(usdPrim);
            var usdSphereLight = new UsdLuxSphereLight(usdPrim);
            var color = Vt.VtValueToGfVec3f(usdLight.GetColorAttr().Get(UsdTimeCode.EarliestTime()));

            var light = new SphereLightDescription()
            {
                brightness =
                {
                    intensity = usdLight.GetIntensityAttr().Get(UsdTimeCode.EarliestTime()),
                    exposure = usdLight.GetExposureAttr().Get(UsdTimeCode.EarliestTime()),
                },
                color =
                {
                    color = new Color(color[0], color[1], color[2]),
                    enableColorTemperature = usdLight.GetEnableColorTemperatureAttr().Get(UsdTimeCode.EarliestTime()),
                    colorTemperature = usdLight.GetColorTemperatureAttr().Get(UsdTimeCode.EarliestTime()),
                },
                radius = usdSphereLight.GetTreatAsPointAttr().Get() ? 0 : usdSphereLight.GetRadiusAttr().Get(UsdTimeCode.EarliestTime()),
                treatAsPoint = usdSphereLight.GetTreatAsPointAttr().Get(UsdTimeCode.EarliestTime()),
                shadowEnabled = usdShadow.GetShadowEnableAttr().Get(UsdTimeCode.EarliestTime()),
            };

            if (usdPrim.HasAPI(TfType.FindByName("UsdLuxShapingAPI")))
            {
                var shapeLight = new UsdLuxShapingAPI(usdPrim);
                light.shape = new LightShapeDescription()
                {
                    isEnabled = true,
                    coneAngle = shapeLight.GetShapingConeAngleAttr().Get(UsdTimeCode.EarliestTime()),
                    coneSoftness = shapeLight.GetShapingConeSoftnessAttr().Get(UsdTimeCode.EarliestTime()),
                    focus = shapeLight.GetShapingFocusAttr().Get(UsdTimeCode.EarliestTime())
                };
            }
            return light;
        }
    }
}
