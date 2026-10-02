using System.Collections;
using System.Text.RegularExpressions;
using UnityEditor.UIElements;
using UnityEngine.Importer;
using UnityEngine.UIElements;

namespace UnityEditor.Importer
{
    [CustomEditor(typeof(ImporterGraph))]
    class ImporterGraphEditor : Editor
    {
        public VisualTreeAsset m_Tree;

        public override VisualElement CreateInspectorGUI()
        {
            var root = m_Tree.CloneTree();
            var container = root.Q("ImportSettingsContainer");
            var importSettingsProp = serializedObject.FindProperty("m_ImportSettings");
            for (int i = 0; i < importSettingsProp.arraySize; ++i)
            {
                container.Add(new PropertyField(importSettingsProp.GetArrayElementAtIndex(i)));
            }
            return root;
        }
    }

    [CustomPropertyDrawer(typeof(ImportSetting<>), true)]
    class ImportSettingEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            using var idProp = property.FindPropertyRelative("id");
            return new PropertyField(property.FindPropertyRelative("value"), idProp.stringValue);
        }
    }

    [CustomPropertyDrawer(typeof(Node<,>), true)]
    class NodeEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new Label(property.managedReferenceValue.ToString());
        }
    }

    [CustomPropertyDrawer(typeof(Edge), true)]
    [CustomPropertyDrawer(typeof(ResultEdge), true)]
    [CustomPropertyDrawer(typeof(SettingEdge), true)]
    class ComponentEditor : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var match = Regex.Match(property.propertyPath, @".*\.Array\.data\[([0-9]+)\]$");
            if (match.Success && typeof(IList).IsAssignableFrom(fieldInfo.FieldType))
            {
                return new Label(
                    (fieldInfo.GetValue(property.serializedObject.targetObject) as IList)
                    [int.Parse(match.Groups[1].Value)]
                        .ToString());
            }
            return new Label(fieldInfo.GetValue(property.serializedObject.targetObject)?.ToString());
        }
    }
}
