using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Importer;
using UnityEngine.UIElements;

namespace UnityEditor.Importer
{
    /// <summary>
    /// This class is serialized as part of the <see cref="ModularImporterEditor"/> to keep track of
    /// which override value is actually a local override or a default value from the base graph.
    /// It has its own PropertyDrawer to display each value in our inspector.
    /// </summary>
    /// <seealso cref="ModularImporterEditor"/>
    public class ImporterGraphOverrideContainer : ScriptableObject
    {
        /// <summary>
        /// This struct represents an <see cref="ImportSetting{T}"/> from an <see cref="ImporterGraph"/>
        /// and it overridden state from a <see cref="ModularImporter"/>.
        /// </summary>
        [Serializable]
        public struct GraphValueState
        {
            /// <summary>
            /// The <see cref="ImportSetting{T}"/> value.
            /// </summary>
            [SerializeReference] public IGraphValue value;
            /// <summary>
            /// The current overriden state of the setting.
            /// True if the settings has an override in the ModularImporter, false otherwise.
            /// </summary>
            public bool isOverride;

            /// <summary>
            /// Creates a new instance of <see cref="GraphValueState"/>.
            /// </summary>
            /// <param name="value">An <see cref="ImportSetting{T}"/>.</param>
            /// <param name="isOverride">The overriden state of the value.</param>
            public GraphValueState(IGraphValue value, bool isOverride)
            {
                this.value = value;
                this.isOverride = isOverride;
            }
        }

        /// <summary>
        /// List of importer settings and their current overridden state.
        /// </summary>
        public List<GraphValueState> values = new();

        /// <summary>
        /// Creates a new <see cref="ImporterGraphOverrideContainer"/>
        /// populated with all the <see cref="ImportSetting{T}"/> from the given <paramref name="graph"/>
        /// and its overridden values and states from the <paramref name="overrides"/>.
        /// Use this method from an <see cref="Editor"/> to create a temporary container that allows to display the mixed values
        /// from an <see cref="ImporterGraph"/> and a <see cref="ModularImporter"/>.
        /// </summary>
        /// <param name="graph">The reference <see cref="ImporterGraph"/> that contains all the settings.</param>
        /// <param name="overrides">List of override settings, usually coming from a <see cref="ModularImporter"/>.</param>
        /// <returns>A new instance of the container with all the settings and their overridden status.</returns>
        /// <seealso cref="ModularImporterEditor"/>
        public static ImporterGraphOverrideContainer CreateOverrideContainer(ImporterGraph graph, IEnumerable<IGraphValue> overrides)
        {
            var container = ScriptableObject.CreateInstance<ImporterGraphOverrideContainer>();
            UpdateImporterGraphOverride(graph, overrides, container);
            return container;
        }

        /// <summary>
        /// Updates an existing <see cref="ImporterGraphOverrideContainer"/> with new values from an <see cref="ImporterGraph"/> and its overridden values.
        /// </summary>
        /// <param name="graph">The <see cref="ImporterGraph"/> to use as a reference for its <see cref="ImportSetting{T}"/>.</param>
        /// <param name="overrides">List of override settings, usually coming from a <see cref="ModularImporter"/>.</param>
        /// <param name="container">The container to update with the graph and overrides values.</param>
        public static void UpdateImporterGraphOverride(ImporterGraph graph, IEnumerable<IGraphValue> overrides,
            ImporterGraphOverrideContainer container)
        {
            if (graph == null)
            {
                return;
            }

            // ImporterSettings are always used through a DeepCopy to avoid editing the graph values themselves.
            // The graph may contains Object references, this breaks AssetImporter serialization and need to be changed into LazyLoadReferences.
            var settingsCopy = graph.ImportSettings.DeepCopy().ConvertObjectToLazyLoadObject();

            // Same as the ImporterSettings from the graph, the overrides are only used through a deep copy.
            var overrideDeepCopy = overrides.DeepCopy();
            var overridesCopy = new Dictionary<string, IGraphValue>();
            foreach (var value in overrideDeepCopy)
            {
                overridesCopy.Add(value.Id, value);
            }

            using var comparer = new ImportSettingComparer();
            bool isDirty = false;
            // Each property is added to the container in the order found in the graph
            // so they are always displayed in the same order.
            var index = 0;
            foreach (var settingCopy in settingsCopy)
            {
                if (overridesCopy.TryGetValue(settingCopy.Id, out var overriddenValue))
                {
                    //Override exists but is not in the container, add it at the right index
                    if (container.values.Count <= index || container.values[index].value.Id != settingCopy.Id)
                    {
                        container.values.Insert(index, new GraphValueState(overriddenValue, true));
                        isDirty = true;
                    }
                    //Override exists, is in the container, but it's not flagged as an override or value is different, update it
                    else if (container.values[index].isOverride == false || !comparer.Equals(container.values[index].value, overriddenValue))
                    {
                        container.values[index] = new GraphValueState(overriddenValue, true);
                        isDirty = true;
                    }
                }
                else
                {
                    //Override does not exist and is not in the container, add a non overriden OverrideValues at the right index
                    if (container.values.Count <= index || container.values[index].value.Id != settingCopy.Id)
                    {
                        container.values.Insert(index, new GraphValueState(settingCopy, false));
                        isDirty = true;
                    }
                    //Override does not exist and is flagged as override in the container or its value is not the default one, "reset" it
                    else if (container.values[index].isOverride == true || !comparer.Equals(container.values[index].value, settingCopy))
                    {
                        container.values[index] = new GraphValueState(settingCopy, false);
                        isDirty = true;
                    }
                }

                index++;
            }
            if (isDirty)
                EditorUtility.SetDirty(container);
        }
    }

    [CustomPropertyDrawer(typeof(ImporterGraphOverrideContainer.GraphValueState))]
    class OverrideValuesDrawer : PropertyDrawer
    {
        // The following strings are default styles used inside Unity for overrides and should not be changed.
        private const string k_PrefabOverrideBarName = "unity-importer-binding-prefab-override-bar";
        private const string k_PrefabOverrideBarUssClassName = "unity-binding__prefab-override-bar";
        private const string k_PrefabOverrideBarNotApplicableUssClassName = "unity-binding__prefab-override-bar-not-applicable";
        private const string k_PrefabOverrideUssClassName = "unity-binding--prefab-override";

        private const string k_StructInMultiSelectionTooltip = "Structs cannot be edited in multi-selection";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            using var valueProperty = property.FindPropertyRelative(nameof(ImporterGraphOverrideContainer.GraphValueState.value));
            bool isPropertyAMultiSelectionStruct = false;
            if (property.serializedObject.targetObjects.Length > 1)
            {
                using var propChild = property.FindPropertyRelative("value");
                var value = (IGraphValue)propChild.managedReferenceValue;
                var type = value?.Value?.GetType();
                isPropertyAMultiSelectionStruct = type != null && type.IsValueType && !type.IsEnum && !type.IsPrimitive;
            }

            var prop = new PropertyField(valueProperty);
            if (isPropertyAMultiSelectionStruct)
            {
                prop.SetEnabled(false);
                prop.tooltip = k_StructInMultiSelectionTooltip;
            }

            using var overridden = property.FindPropertyRelative(nameof(ImporterGraphOverrideContainer.GraphValueState.isOverride));
            var blueLine = CreateBlueLine();
            UpdateBlueLine(overridden, blueLine, prop);

            container.Add(prop);
            container.Add(blueLine);

            container.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                ReplacePrefabOverrideBarStyle(blueLine, prop, evt.target as VisualElement);
            });
            container.TrackPropertyValue(overridden, serializedProperty =>
            {
                UpdateBlueLine(serializedProperty, blueLine, prop);
            });

            return container;
        }

        private static void UpdateBlueLine(SerializedProperty serializedProperty, VisualElement blueLine, PropertyField prop)
        {
            if (serializedProperty.boolValue || serializedProperty.hasMultipleDifferentValues)
            {
                blueLine.style.display = DisplayStyle.Flex;
                prop.AddToClassList(k_PrefabOverrideUssClassName);
                blueLine.RemoveFromClassList(k_PrefabOverrideBarNotApplicableUssClassName);
                blueLine.RemoveFromClassList(k_PrefabOverrideBarUssClassName);
                blueLine.AddToClassList(serializedProperty.hasMultipleDifferentValues ? k_PrefabOverrideBarNotApplicableUssClassName : k_PrefabOverrideBarUssClassName);
            }
            else
            {
                prop.RemoveFromClassList(k_PrefabOverrideUssClassName);
                blueLine.style.display = DisplayStyle.None;
            }
        }

        private static VisualElement CreateBlueLine()
        {
            var blueLine = new VisualElement() { name = k_PrefabOverrideBarName };
            blueLine.style.position = Position.Absolute;
            return blueLine;
        }

        /// <summary>
        /// This method is a more or less a direct copy of the internal <see cref="BindingsStyleHelpers.UpdatePrefabOverrideOrLivePropertyBarStyle"/>
        /// </summary>
        /// <param name="blueBar"></param>
        /// <param name="element"></param>
        /// <param name="container"></param>
        private static void ReplacePrefabOverrideBarStyle(VisualElement blueBar, VisualElement element, VisualElement container)
        {
            if (container == null)
                return;

            // Move the bar to where the control is in the container.
            var top = element.worldBound.y - container.worldBound.y;
            if (float.IsNaN(top))     // If this is run before the container has been layered out.
                return;

            var elementHeight = element.resolvedStyle.height;

            // This is needed so if you have 2 overridden fields their blue
            // bars touch (and it looks like one long bar). They normally wouldn't
            // because most fields have a small margin.
            var bottomOffset = element.resolvedStyle.marginBottom;

            blueBar.style.top = top;
            blueBar.style.height = elementHeight + bottomOffset;
            // Our UI is moved to the right twice. 15 is the hardcoded indentation value in Unity
            // We have to move left twice to be on the border of the inspector window.
            blueBar.style.marginLeft = -30f;
        }
    }
}
