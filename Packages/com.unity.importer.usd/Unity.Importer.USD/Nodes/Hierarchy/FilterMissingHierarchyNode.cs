using System.Collections.Generic;
using pxr;
using UnityEngine;
using UnityEngine.Importer;

namespace Unity.Importer.USD
{
    /// <summary>
    /// This node will traverse the usd stage and filter any PrimPath that is not present in the gameobjects input.
    /// These missing PrimPath will then be output, sorted with the provided filterTfType.
    /// </summary>
    [NodeMetadata("FilterMissingHierarchyNode", 1)]
    public class FilterMissingHierarchyNode : Node<FilterMissingHierarchyNode.InputPort, FilterMissingHierarchyNode.OutputPort>
    {
        /// <summary>
        /// Input ports of the <see cref="FilterMissingHierarchyNode"/>.
        /// </summary>
        public class InputPort : InputPorts
        {
            /// <summary>
            /// Used as a filter for the missing prims.
            /// </summary>
            public TfType filterTfType;

            /// <summary>
            /// The current Usd Stage.
            /// </summary>
            public UsdStage stage;

            /// <summary>
            /// A PrimPath-GameObjects mapping to filter.
            /// </summary>
            public Dictionary<string, GameObject> gameObjects;
        }

        /// <summary>
        /// Output ports of the <see cref="FilterMissingHierarchyNode"/>.
        /// </summary>
        public class OutputPort : OutputPorts
        {
            /// <summary>
            /// Prims of the specified filterTfType.
            /// </summary>
            public List<UsdPrim> PrimOfTfType = new();

            /// <summary>
            /// Prims of a different TfType than filterTfType.
            /// </summary>
            public List<UsdPrim> PrimOfNonTfType = new();
        }

        /// <summary>
        /// See the documentation for &lt;Node{TInput,TOutput}.Run&gt; in the com.unity.importer package documentation for details
        /// </summary>
        public override void Run()
        {
            var explored = new HashSet<string>();
            foreach (var kvp in Input.gameObjects)
            {
                var path = kvp.Key;
                var parentPath = path.Substring(0, path.LastIndexOf('/'));

                while (parentPath != string.Empty && path != "/" && path.Contains('/'))
                {
                    if (!explored.Contains(parentPath) && !Input.gameObjects.ContainsKey(parentPath))
                    {
                        var prim = Input.stage.GetPrimAtPath(new SdfPath(parentPath));
                        if (prim != null && prim.IsValid())
                        {
                            if (prim.IsA(Input.filterTfType))
                            {
                                Output.PrimOfTfType.Add(prim);
                            }
                            else
                            {
                                Output.PrimOfNonTfType.Add(prim);
                            }
                        }
                        else
                        {
                            Input.GraphLogger.LogImportWarning(
                                $"Could not find the parent prim path '{parentPath}' in the usd file.", null,
                                NodeWarnings.CouldNotFindParentPrim);
                        }

                        explored.Add(parentPath);
                    }
                    parentPath = parentPath.Substring(0, parentPath.LastIndexOf('/'));
                }
            }
        }
    }
}
