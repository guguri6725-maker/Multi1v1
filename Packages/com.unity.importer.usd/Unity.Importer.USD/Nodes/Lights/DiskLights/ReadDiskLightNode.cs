using System;
using System.Collections.Generic;
using pxr;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will read DiskLightDescription info from a list of UsdLuxDiskLight prims.
    /// </summary>
    [NodeMetadata("ReadDiskLightNode", 0)]
    public class ReadDiskLightNode : Node<ReadDiskLightNode.InputPort, ReadDiskLightNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="ReadDiskLightNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// List of UsdLuxDiskLight prims to convert.
            /// </summary>
            public List<UsdPrim> usdDiskLights;
        }

        /// <summary>
        /// Output ports of the <see cref="ReadDiskLightNode"/>.
        /// </summary>
        [Serializable]
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-DiskLightDescription mapping containing all the lights created by this node.
            /// </summary>
            public Dictionary<string, DiskLightDescription> diskLights = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            ComputeDiskLightData(Input.usdDiskLights);
        }

        private void ComputeDiskLightData(List<UsdPrim> lightPrims)
        {
            foreach (var prim in lightPrims)
            {
                Output.diskLights[prim.GetPath()] = GetDiskLightData(prim);
            }
        }

        private DiskLightDescription GetDiskLightData(UsdPrim usdPrim)
        {
            var usdLight = new UsdLuxLightAPI(usdPrim);
            var usdShadow = new UsdLuxShadowAPI(usdPrim);
            var usdDiskLight = new UsdLuxDiskLight(usdPrim);
            var color = Vt.VtValueToGfVec3f(usdLight.GetColorAttr().Get(UsdTimeCode.EarliestTime()));

            var light = new DiskLightDescription()
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
                radius = usdDiskLight.GetRadiusAttr().Get(),
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
