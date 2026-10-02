using System;
using System.Collections.Generic;
using pxr;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will read DistantLightDescriptions info from a list of UsdLuxDistantLight prims.
    /// </summary>
    [NodeMetadata("ReadDistantLightNode", 0)]
    public class ReadDistantLightNode : Node<ReadDistantLightNode.InputPort, ReadDistantLightNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadDistantLightNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// List of UsdLuxDistantLight prims to convert.
            /// </summary>
            public List<UsdPrim> usdDistantLights;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadDistantLightNode"/>.
        /// </summary>
        [Serializable]
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-DistantLightDescription mapping containing all the lights created by this node.
            /// </summary>
            public Dictionary<string, DistantLightDescription> distantLights = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            ComputeDistantLightData(Input.usdDistantLights);
        }

        private void ComputeDistantLightData(List<UsdPrim> lightPrims)
        {
            foreach (var prim in lightPrims)
            {
                Output.distantLights[prim.GetPath()] = GetDistantLightData(prim);
            }
        }

        private DistantLightDescription GetDistantLightData(UsdPrim usdPrim)
        {
            var usdLight = new UsdLuxLightAPI(usdPrim);
            var usdShadow = new UsdLuxShadowAPI(usdPrim);

            var color = Vt.VtValueToGfVec3f(usdLight.GetColorAttr().Get());

            var light = new DistantLightDescription()
            {
                brightness =
                {
                    // intensity = usdLight.GetIntensityAttr().Get(),
                    // BUG DCC-3882 - Distant Light is getting 50 000 from UsdLuxLightAPI when not set in file but BuildInRP and URP cannot use that value - they want 1. And any value in Lux/Lumen will blast the scene beyond recognition.
                    // HDRP can use this value but it is overiding the default Intensity value in the Light component to 100 000 (It's an overide only visible in Debug inspector - so revert the overide and use the real intensity from GetIntensityAttr if you need the original Lux/Lumen value in HDRP)
                    // Setting to 1 + ignoring related falling tests for now. The meaning of "intensity" varies so much between among applications, there's no good solution for this.
                    intensity = 1f,
                    exposure = usdLight.GetExposureAttr().Get(),
                },
                color =
                {
                    color = new Color(color[0], color[1], color[2]),
                    enableColorTemperature = usdLight.GetEnableColorTemperatureAttr().Get(),
                    colorTemperature = usdLight.GetColorTemperatureAttr().Get(),
                },

                shadowEnabled = usdShadow.GetShadowEnableAttr().Get(),
            };
            return light;
        }
    }
}
