using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will add Light component to the GameObjects of the hierarchy.
    /// </summary>
    [NodeMetadata("CreateDiskLightNode", 0)]
    public class CreateDiskLightNode : Node<CreateDiskLightNode.InputPort, CreateDiskLightNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="CreateDiskLightNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// A PrimPath-DiskLightDescription mapping, read from the usd file.
            /// </summary>
            public Dictionary<string, DiskLightDescription> diskLights;

            /// <summary>
            /// A PrimPath-GameObject mapping containing all the GameObjects that this node may turn into Disk Light.
            /// </summary>
            public Dictionary<string, GameObject> gameObjects;

            /// <summary>
            /// USD stage metadata.
            /// </summary>
            public UsdStageMetadata usdMetadata;

            /// <summary>
            /// The light intensity multiplier to apply
            /// </summary>
            public float lightIntensityMultiplier;
        }

        /// <summary>
        /// Output ports of the <see cref="CreateDiskLightNode"/>.
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
                if (Input.diskLights.TryGetValue(kvp.Key, out var lightData))
                {
                    var rotationOffsetGO = new GameObject();
                    rotationOffsetGO.name = "light";
                    rotationOffsetGO.transform.rotation = rotationOffset;
                    var light = rotationOffsetGO.AddComponent<Light>();
                    ApplyDiskLightData(light, lightData, Input.usdMetadata.metersPerUnit, Input.lightIntensityMultiplier);
                    rotationOffsetGO.transform.SetParent(kvp.Value.transform, false);
                }
                else
                {
                    Input.GraphLogger.LogImportWarning($"Could not find a matching DiskLightDescription for the GameObject at '{kvp.Key}'", null, NodeWarnings.CouldNotFindDiskLightDescription);
                }
            }
        }

        private static void ApplyDiskLightData(Light light, DiskLightDescription data, float scaleFactor, float lightIntensityMultiplier)
        {
            light.type = LightType.Disc;
            data.shape.ApplyTolight(light);
            data.color.ApplyTolight(light);
            data.brightness.ApplyTolight(light, lightIntensityMultiplier);

            light.shadows = data.shadowEnabled ? LightShadows.Soft : LightShadows.None;
            light.range = scaleFactor * 100.0f;
        }
    }
}
