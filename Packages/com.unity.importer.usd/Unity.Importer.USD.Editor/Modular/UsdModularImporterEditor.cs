using System.IO;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UnityEditor.Importer.USD
{
    [CustomEditor(typeof(UsdModularImporter))]
    [CanEditMultipleObjects]
    class UsdModularImporterEditor : ModularImporterEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var areAllUsdz = targets.Cast<AssetImporter>()
                .Select(i => Path.GetExtension(i.assetPath))
                .All(e => e == ".usdz");
            if (areAllUsdz)
            {
                return base.CreateInspectorGUI();
            }

            var isUsdRootProp = serializedObject.FindProperty(nameof(UsdModularImporter.isUsdRoot));
            var ownRoot = new VisualElement();
            ownRoot.Add(new PropertyField(isUsdRootProp, "Import"));
            var graphUI = base.CreateInspectorGUI();
            graphUI.style.display = isUsdRootProp.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            ownRoot.Add(graphUI);

            var nonImportedContainer = new VisualElement();
            var usdInformationBox = new Box();
            usdInformationBox.Add(new Label("This USD file isn’t imported, but may be referenced by other USD files.\nTypically, you should only mark the root of a composition for import."));
            nonImportedContainer.Add(usdInformationBox);

            nonImportedContainer.Add(new IMGUIContainer(ApplyRevertGUI));
            nonImportedContainer.style.display = isUsdRootProp.boolValue ? DisplayStyle.None : DisplayStyle.Flex;

            ownRoot.Add(nonImportedContainer);

            ownRoot.TrackPropertyValue(isUsdRootProp, property =>
            {
                graphUI.style.display = property.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
                nonImportedContainer.style.display = property.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
            });

            return ownRoot;
        }
    }
}
