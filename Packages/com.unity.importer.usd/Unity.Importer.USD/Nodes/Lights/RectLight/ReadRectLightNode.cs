using System;
using System.Collections.Generic;
using pxr;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will read RectLightDescription info from a list of UsdLuxRectLight prims.
    /// </summary>
    [NodeMetadata("ReadRectLightNode", 0)]
    public class ReadRectLightNode : Node<ReadRectLightNode.InputPort, ReadRectLightNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadRectLightNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// List of UsdLuxRectLight prims to convert.
            /// </summary>
            public List<UsdPrim> usdRectLights;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadRectLightNode"/>.
        /// </summary>
        [Serializable]
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-RectLightDescription mapping containing all the lights created by this node.
            /// </summary>
            public Dictionary<string, RectLightDescription> rectLights = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            ComputeRectLightData(Input.usdRectLights);
        }

        private void ComputeRectLightData(List<UsdPrim> lightPrims)
        {
            foreach (var prim in lightPrims)
            {
                Output.rectLights[prim.GetPath()] = GetRectLightData(prim);
            }
        }

        private RectLightDescription GetRectLightData(UsdPrim usdPrim)
        {
            var usdLight = new UsdLuxLightAPI(usdPrim);
            var usdShadow = new UsdLuxShadowAPI(usdPrim);
            var usdRectLight = new UsdLuxRectLight(usdPrim);
            var color = Vt.VtValueToGfVec3f(usdLight.GetColorAttr().Get(UsdTimeCode.EarliestTime()));

            var light = new RectLightDescription()
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
                width = usdRectLight.GetWidthAttr().Get(UsdTimeCode.EarliestTime()),
                height = usdRectLight.GetHeightAttr().Get(UsdTimeCode.EarliestTime()),
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
