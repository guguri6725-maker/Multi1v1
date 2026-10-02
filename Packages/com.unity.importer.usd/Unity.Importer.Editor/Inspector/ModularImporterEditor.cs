using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.AssetImporters;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Importer;
using UnityEngine.UIElements;

namespace UnityEditor.Importer
{
    /// <summary>
    /// Default Editor for any asset imported using a <see cref="ModularImporter"/>.
    /// The inspector displays every exposed <see cref="ImporterGraph.ImportSettings"/> from the <see cref="ImporterGraph"/> and allow users to override a setting on specific asset.
    /// </summary>
    [CustomEditor(typeof(ModularImporter), true)]
    [CanEditMultipleObjects]
    public class ModularImporterEditor : AssetImporterEditor
    {
        // Container for all the importer settings of the graph.
        // This container is the one displayed in the Editor and getting users inputs.
        // When changed, the new values are propagated to the serializedObject of the Editor
        // so the Importer is always in sync.
        private SerializedObject overrideContainerSerializedObject;
        private ImporterGraphOverrideContainer[] overrideContainers;

        // Saved state from the last applied values of each target of the Editor (what was used for the last import)
        // This is used to revert changes and compare if anything need to be applied.
        private SerializedObject[] targetsSerialization;
        private SerializedObject[] cachingSerialization;
        private ModularImporter[] caching;
        private Hash128[] previousHash;

        /// <inheritdoc cref="Editor.CreateInspectorGUI"/>
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var graphProp = serializedObject.FindProperty("m_Graph");
            var graphUIProp = new PropertyField(graphProp);
            root.Add(graphUIProp);

            var graphValues = new VisualElement() { name = "AllImportSettings" };
            root.Add(graphValues);

            BuildImportSettingsUI(graphValues, graphProp);

            root.RegisterCallback<SerializedPropertyChangeEvent>(UpdateOverride);

            graphUIProp.TrackPropertyValue(graphProp, property =>
            {
                OnGraphChanges(graphValues, property);
            });

            root.TrackSerializedObjectValue(serializedObject, _ =>
            {
                if (TryUpdateAssetHashes())
                {
                    SaveChanges();
                    return;
                }
                UpdateHasModifiedState();
            });

            root.Add(new IMGUIContainer(ApplyRevertGUI));

            return root;
        }

        private void BuildImportSettingsUI(VisualElement graphValues, SerializedProperty graphProp)
        {
            if (graphProp.hasMultipleDifferentValues)
            {
                var box = new Box();
                box.Add(new Label("Cannot edit multiple asset settings with different importer graphs."));
                graphValues.Add(box);
            }
            else if (graphProp.objectReferenceValue == null)
            {
                var box = new Box();
                box.Add(new Label("An ImporterGraph is required to edit the asset settings."));
                graphValues.Add(box);
            }
            else
            {
                var container = new Foldout() { text = "Import Settings", value = true };
                graphValues.Add(container);
                using var settings = overrideContainerSerializedObject.FindProperty(nameof(ImporterGraphOverrideContainer.values));
                for (int i = 0; i < settings.arraySize; ++i)
                {
                    using var property = settings.GetArrayElementAtIndex(i);
                    var prop = new PropertyField(property);
                    container.Add(prop);

                    // The inspector will always try to bind serialized properties to the actual serializedObject.
                    // This callback is fixing the binding to correctly target overrideContainerSerializedObject.
                    container.RegisterCallback<AttachToPanelEvent>(_ =>
                    {
                        prop.Unbind();
                        prop.Bind(property.serializedObject);
                    });
                }
            }
        }

        private bool TryUpdateAssetHashes()
        {
            bool hashChanged = false;
            for (int i = 0; i < targets.Length; i++)
            {
                var importer = (AssetImporter)targets[i];
                var hash = AssetDatabase.GetAssetDependencyHash(importer.assetPath);
                if (hash != previousHash[i])
                {
                    hashChanged = true;
                    previousHash[i] = hash;
                }
            }
            return hashChanged;
        }

        private void UpdateHasModifiedState()
        {
            hasUnsavedChanges = false;
            for (int i = 0; i < cachingSerialization.Length; i++)
            {
                cachingSerialization[i].Update();
                targetsSerialization[i].Update();
                using var iteratorCaching = cachingSerialization[i].GetIterator();
                using var iteratorTarget = targetsSerialization[i].GetIterator();

                bool enterChild = true;
                bool startChecking = false;
                while (iteratorCaching.Next(enterChild) && iteratorTarget.Next(enterChild))
                {
                    enterChild = false;

                    if (startChecking)
                    {
                        if (!SerializedProperty.DataEquals(iteratorCaching, iteratorTarget))
                        {
                            hasUnsavedChanges = true;
                            break;
                        }
                    }
                    else if (iteratorCaching.propertyPath == "m_Script")
                    {
                        startChecking = true;
                    }
                }

                if (hasUnsavedChanges)
                    break;
            }
            UpdateContainerData();
        }

        /// <inheritdoc cref="AssetImporterEditor.OnEnable"/>
        public override void OnEnable()
        {
            base.OnEnable();
            EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;

            overrideContainers = new ImporterGraphOverrideContainer[targets.Length];
            caching = new ModularImporter[targets.Length];
            cachingSerialization = new SerializedObject[targets.Length];
            targetsSerialization = new SerializedObject[targets.Length];
            previousHash = new Hash128[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                var importer = (ModularImporter)targets[i];
                overrideContainers[i] = ImporterGraphOverrideContainer.CreateOverrideContainer(importer.Graph.asset,
                    importer.ImportSettingOverrides);

                caching[i] = Instantiate(importer);
                cachingSerialization[i] = new SerializedObject(caching[i]);
                targetsSerialization[i] = new SerializedObject(importer);
                previousHash[i] = AssetDatabase.GetAssetDependencyHash(importer.assetPath);
            }

            overrideContainerSerializedObject = new SerializedObject(overrideContainers);
        }

        /// <inheritdoc cref="AssetImporterEditor.OnDisable"/>
        public override void OnDisable()
        {
            overrideContainerSerializedObject.Dispose();
            for (var i = 0; i < targets.Length; i++)
            {
                cachingSerialization[i].Dispose();
                targetsSerialization[i].Dispose();
                DestroyImmediate(overrideContainers[i]);
                DestroyImmediate(caching[i]);
            }

            EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
            base.OnDisable();
        }

        /// <summary>
        /// This method is called when the Inspector is closed or the selection changes and the Discard Changes button is pressed.
        /// When this happen, we reset each target to their previously saved state (last Apply).
        /// </summary>
        /// <inheritdoc cref="Editor.DiscardChanges"/>
        public override void DiscardChanges()
        {
            base.DiscardChanges();

            overrideContainerSerializedObject.ApplyModifiedProperties();
            for (int i = 0; i < targets.Length; i++)
            {
                var importer = (ModularImporter)targets[i];
                EditorUtility.CopySerialized(caching[i], importer);
                EditorUtility.SetDirty(importer);
                targetsSerialization[i].Update();
                ImporterGraphOverrideContainer.UpdateImporterGraphOverride(importer.Graph.asset, importer.ImportSettingOverrides, overrideContainers[i]);
            }

            overrideContainerSerializedObject.Update();
            serializedObject.Update();

            for (int i = 0; i < targets.Length; i++)
            {
                EditorUtility.ClearDirty(targets[i]);
            }
        }

        /// <inheritdoc cref="AssetImporterEditor.SaveChanges"/>
        public override void SaveChanges()
        {
            // This Start/StopAssetEditing prevents Unity from destroying the Inspector during the re-import
            // while we are still processing data on the Editor caching.
            AssetDatabase.StartAssetEditing();
            base.SaveChanges();
            serializedObject.ApplyModifiedProperties();
            bool shouldUpdate = true;
            for (int i = 0; i < targets.Length; i++)
            {
                // it is possible the editor is already closed here and the caching objects got destroyed in native code.
                if (caching[i] != null)
                {
                    var importer = (ModularImporter)targets[i];
                    EditorUtility.CopySerialized(importer, caching[i]);
                    EditorUtility.SetDirty(importer);
                    AssetDatabase.ImportAsset(importer.assetPath);
                }
                else
                {
                    shouldUpdate = false;
                }
            }
            // Only update when all caching are still valid, if one of them was destroyed the Editor is in closing mode
            // and the underlying SerializedObject are already destroyed.
            if (shouldUpdate)
                UpdateHasModifiedState();
            AssetDatabase.StopAssetEditing();
            TryUpdateAssetHashes();
        }

        private void OnGraphChanges(VisualElement propertyContainer, SerializedProperty graph)
        {
            serializedObject.ApplyModifiedProperties();
            overrideContainerSerializedObject.ApplyModifiedProperties();

            propertyContainer.Clear();

            UpdateContainerData();

            BuildImportSettingsUI(propertyContainer, graph);
            propertyContainer.SendEvent(new AttachToPanelEvent());
        }

        private void UpdateContainerData()
        {
            for (int i = 0; i < targets.Length; i++)
            {
                var importer = (ModularImporter)targets[i];
                ImporterGraphOverrideContainer.UpdateImporterGraphOverride(importer.Graph.asset,
                    new List<IGraphValue>(importer.ImportSettingOverrides), overrideContainers[i]);
            }

            overrideContainerSerializedObject.Update();
        }

        #region ContextMenu

        private void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
        {
            if (!TryGetOverrideProperty(property, out var value, out var overridden))
                return;

            if (overridden.boolValue || overridden.hasMultipleDifferentValues)
            {
                menu.AddItem(new GUIContent("Revert value"), false, RevertToGraphValue, value.propertyPath);
            }
        }

        private void RevertToGraphValue(object data)
        {
            var path = (string)data;
            using var property = overrideContainerSerializedObject.FindProperty(path);
            var revertedPropertyId = ((IGraphValue)property.managedReferenceValue).Id;

            overrideContainerSerializedObject.ApplyModifiedProperties();
            for (int i = 0; i < targets.Length; i++)
            {
                IGraphValue copySetting = null;
                var graphAsset = ((ModularImporter)targets[i]).Graph.asset;
                if (graphAsset == null)
                    continue;

                foreach (var s in graphAsset.ImportSettings)
                {
                    if (s.Id == revertedPropertyId)
                    {
                        copySetting = s.DeepCopy();
                        break;
                    }
                }

                if (copySetting == null)
                    continue;

                copySetting = copySetting.ConvertObjectToLazyLoadObject();

                var extraData = overrideContainers[i];
                for (var index = 0; index < extraData.values.Count; index++)
                {
                    var value = extraData.values[index];
                    if (value.value.Id == revertedPropertyId)
                    {
                        if (value.isOverride)
                        {
                            extraData.values[index] = new ImporterGraphOverrideContainer.GraphValueState()
                            {
                                value = copySetting,
                                isOverride = false
                            };
                            EditorUtility.SetDirty(extraData);
                            var importer = (ModularImporter)targets[i];
                            foreach (var settingOverride in importer.ImportSettingOverrides)
                            {
                                if (settingOverride.Id == revertedPropertyId)
                                {
                                    importer.RemoveImportSettingOverride(settingOverride);
                                    EditorUtility.SetDirty(importer);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }

            overrideContainerSerializedObject.Update();
        }

        #endregion

        #region Serialization Changes

        private bool TryGetOverrideProperty(SerializedProperty property, out SerializedProperty value,
            out SerializedProperty overridden)
        {
            value = null;
            overridden = null;

            if (property.serializedObject == null)
                return false;

            var match = Regex.Match(property.propertyPath, @"^((values\.Array\.data\[[0-9]+\]).value$)");
            if (match.Success)
            {
                value = property.serializedObject.FindProperty(match.Groups[1].Value);
                overridden = property.serializedObject.FindProperty(match.Groups[2].Value + ".isOverride");
            }
            return value != null && overridden != null;
        }

        private void UpdateOverride(SerializedPropertyChangeEvent evt)
        {
            if (!TryGetOverrideProperty(evt.changedProperty, out var property, out var overridden))
                return;

            var newValue = (IGraphValue)property.managedReferenceValue;
            // newValue can be null if the property changed is a foldout for example.
            if (newValue == null)
                return;

            using var importSettingsComparer = new ImportSettingComparer();

            // When there is a multi-selection and the value is not the same on each asset that means it hasn't been changed from the UI
            // so we shouldn't do anything here because it has to be the initial setup call and not a user change.
            if (property.hasMultipleDifferentValues)
            {
                // let's make sure they are all different, because it may not be true,
                // the hasMultipleDifferentValues is not being updated correctly when it's a serializedReference...
                for (int i = 0; i < overrideContainers.Length; i++)
                {
                    foreach (var value in overrideContainers[i].values)
                    {
                        if (value.value.Id == newValue.Id)
                        {
                            if (!importSettingsComparer.Equals(value.value, newValue))
                                return;
                        }
                    }
                }
            }

            // When overriden is true everywhere, we can just forward the change to the importer and let it deal with it.
            if (!overridden.hasMultipleDifferentValues && overridden.boolValue)
            {
                UpdateImportersValue(newValue);
                return;
            }

            bool contains = true;
            var graph = ((ModularImporter)target).Graph.asset;
            foreach (var setting in graph.ImportSettings)
            {
                if (setting.Id == newValue.Id)
                {
                    contains = importSettingsComparer.Equals(setting, newValue);
                    break;
                }
            }

            // if the graph asset doesn't contain the value, it means it was changed and is an override now.
            if (!contains)
            {
                overridden.boolValue = true;
                overrideContainerSerializedObject.ApplyModifiedProperties();

                UpdateImportersValue(newValue);
            }
        }

        private void UpdateImportersValue(IGraphValue graphValue)
        {
            using ImportSettingComparer importSettingsComparer = new ImportSettingComparer();
            serializedObject.ApplyModifiedProperties();
            Undo.RecordObjects(targets, "Inspector");
            var copy = graphValue.DeepCopy();
            for (int i = 0; i < targets.Length; i++)
            {
                var importer = (ModularImporter)targets[i];
                // we only change the override data on the importer if it's different from the existing one.
                bool localContains = false;
                foreach (var settingOverride in importer.ImportSettingOverrides)
                {
                    if (settingOverride.Id == copy.Id)
                    {
                        localContains = importSettingsComparer.Equals(settingOverride, copy);
                        if (!localContains)
                            importer.RemoveImportSettingOverride(settingOverride);
                        break;
                    }
                }

                if (!localContains)
                {
                    importer.AddImportSettingOverride(copy);
                    EditorUtility.SetDirty(importer);
                }
            }

            serializedObject.Update();
        }

        #endregion
    }
}
