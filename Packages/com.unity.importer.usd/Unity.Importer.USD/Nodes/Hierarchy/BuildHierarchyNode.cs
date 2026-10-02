using System;
using System.Collections.Generic;
using pxr;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will use the provided PrimPath-GameObjects mapping and build a single Unity GameObjects hierarchy from it.
    /// </summary>
    [NodeMetadata("BuildHierarchyNode", 1)]
    public class BuildHierarchyNode : Node<BuildHierarchyNode.InputPort, BuildHierarchyNode.OutputPort>
    {
        private const string baseFieldName = "gameObjects_";

        /// <summary>
        /// Input ports of the <see cref="BuildHierarchyNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// Creates a additional root game object when there is only 1 game object.
            /// The root game object is always created when there are multiple game objects.
            /// </summary>
            public bool preserveSceneRoot;

            /// <summary>
            /// The current USD Stage, used to get this USD PseudoRoot.
            /// </summary>
            public UsdStage stage;

            /// <summary>
            /// A PrimPath-GameObjects to converted as a Unity GameObjects hierarchy.
            /// </summary>
            public Dictionary<string, GameObject> gameObjects;
        }

        /// <summary>
        /// Output ports of the <see cref="BuildHierarchyNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// The resulting root of the Unity GameObjects hierarchy.
            /// </summary>
            public GameObject root;
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            var rootCount = 0;
            GameObject rootGameObject = null;
            foreach (var go in Input.gameObjects)
            {
                if (go.Key.LastIndexOf('/') == 0)
                {
                    rootCount++;
                    rootGameObject = go.Value;
                }
            }

            var createSceneRoot = Input.preserveSceneRoot || rootCount != 1;
            var root = Input.stage.GetPseudoRoot();
            var sceneRootGameObject = createSceneRoot ? new GameObject("root") : null;
            var pathToTransform = new Dictionary<string, Transform>();
            foreach (var kvp in Input.gameObjects)
            {
                pathToTransform.Add(kvp.Key, kvp.Value.transform);
            }

            foreach (var go in Input.gameObjects)
            {
                var lastSeparatorIndex = go.Key.LastIndexOf('/');
                var parentPath = go.Key.Substring(0, lastSeparatorIndex > 0 ? lastSeparatorIndex : 0);
                if (pathToTransform.TryGetValue(parentPath, out Transform parent))
                {
                    go.Value.transform.SetParent(parent, false);
                }
                else if (parentPath == String.Empty || parentPath == root.GetPath())
                {
                    if (createSceneRoot)
                    {
                        go.Value.transform.SetParent(sceneRootGameObject.transform, false);
                    }
                }
                else
                {
                    Input.GraphLogger.LogImportWarning($"Could not find parent '{parentPath}' when trying to parent the GameObject at path '{go.Key}'", null, NodeWarnings.CouldNotFindParentGameObject);
                }
            }
            Output.root = createSceneRoot ? sceneRootGameObject : rootGameObject;
        }
    }
}
