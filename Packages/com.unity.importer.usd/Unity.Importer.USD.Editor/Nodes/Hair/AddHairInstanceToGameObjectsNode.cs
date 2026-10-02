using System.Collections.Generic;
using Unity.Importer.USD;
using UnityEngine;
using UnityEngine.Importer;

#if HAIR_0_OR_HIGHER
using Unity.DemoTeam.Hair;

namespace UnityEditor.Importer.USD
{
    /// <summary>
    /// Add hair instances to targeted GameObjects and link an hair asset to them.
    /// </summary>
    [NodeMetadata("AddHairInstanceToGameObjectsNode", 0, description = "Add hair instances to targeted GameObjects and link an hair asset to them.", displayName = "AddHairInstanceToGameObjects")]
    public class AddHairInstanceToGameObjectsNode : Node<AddHairInstanceToGameObjectsNode.InputPort, AddHairInstanceToGameObjectsNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="AddHairInstanceToGameObjectsNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// A PrimPath-HairAsset mapping used to assign hair assets to the right GameObjects.
            /// </summary>
            public Dictionary<string, HairAsset> hairAssets;

            /// <summary>
            /// A mapping of prim path to their Master prim path, if any.
            /// Used to handle USD references and re-use hair assets.
            /// </summary>
            public Dictionary<string, string> objsToRefs;

            /// <summary>
            /// A PrimPath-GameObjects mapping used to get the GameObject representing a specific Prim.
            /// </summary>
            public Dictionary<string, GameObject> gameObjects;
        }

        /// <summary>
        /// Output ports of the <see cref="AddHairInstanceToGameObjectsNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// A PrimPath-GameObjects mapping, commonly used when reconstructing the hierarchy.
            /// </summary>
            public Dictionary<string, GameObject> gameObjects;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            Output.gameObjects = new Dictionary<string, GameObject>(Input.gameObjects.Count);
            foreach (var kvp in Input.gameObjects)
            {
                if (Input.objsToRefs.TryGetValue(kvp.Key, out var hairAssetName))
                {
                    if (Input.hairAssets.TryGetValue(hairAssetName, out var hairAsset))
                    {
                        HairInstance hairInstance = kvp.Value.AddComponent<HairInstance>();
                        hairInstance.strandGroupProviders = new HairInstance.GroupProvider[1];

                        hairInstance.settingsSystem.simulation = false;

                        HairInstance.GroupProvider groupProvider = new HairInstance.GroupProvider();
                        groupProvider.hairAsset = hairAsset;

                        hairInstance.strandGroupProviders[0] = groupProvider;
                        HairInstanceBuilder.BuildHairInstance(hairInstance, hairInstance.strandGroupProviders);
                    }
                    else
                    {
                        Input.GraphLogger.LogImportWarning($"Could not find hair asset '{hairAssetName}'.", null, NodeWarnings.CouldNotFindHairAsset);
                    }
                }
                Output.gameObjects.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
#endif
