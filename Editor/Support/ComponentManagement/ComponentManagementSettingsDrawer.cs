using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using BBUnity.Core;
using BBUnity.Editor.Toolkit;
using BBUnity.Editor.Windows;

namespace BBUnity.Editor.Support.ComponentManagement {

    /// <summary>
    /// The Component Management section of BBUnitySettings: the on/off switch, a dropdown of
    /// presets (ending in "Create New Preset...") and a shortcut to the preset window.
    /// </summary>
    [CustomPropertyDrawer(typeof(BBUnitySettings.ComponentManagementSettings))]
    public class ComponentManagementSettingsDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            SerializedProperty preset = property.FindPropertyRelative("_preset");
            SerializedObject serializedObject = property.serializedObject;
            string presetPath = preset.propertyPath;

            var foldout = new Foldout { text = property.displayName, value = property.isExpanded };
            foldout.RegisterValueChangedCallback(change => {
                if (change.target == foldout) { property.isExpanded = change.newValue; }
            });

            foldout.Add(new PropertyField(property.FindPropertyRelative("_enabled")));

            var picker = new PickerField("Preset", field => ShowPresetMenu(serializedObject, presetPath, field));
            picker.Track(preset, changed => ComponentPresets.GetById(changed.stringValue).name);
            foldout.Add(picker);

            foldout.Add(new Button(() => ComponentPresetsWindow.Open(null)) { text = "Manage Presets..." });

            return foldout;
        }

        private static void ShowPresetMenu(SerializedObject serializedObject, string presetPath, PickerField field) {
            string currentId = ComponentPresets.GetById(serializedObject.FindProperty(presetPath).stringValue) is { } current ? ComponentPresets.GetId(current) : string.Empty;
            var menu = new GenericMenu();

            void AddPreset(ComponentCategoryPresetDefinition preset) {
                string id = ComponentPresets.GetId(preset);
                menu.AddItem(new GUIContent(preset.name), id == currentId, () => Select(serializedObject, presetPath, id));
            }

            AddPreset(ComponentPresets.BuiltIn);
            foreach (ComponentCategoryPresetDefinition preset in ComponentPresets.GetCustom()) { AddPreset(preset); }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Create New Preset..."), false, () => {
                ComponentCategoryPresetDefinition created = ComponentPresets.CreateFrom(ComponentPresets.GetActive(), "New Preset");
                Select(serializedObject, presetPath, ComponentPresets.GetId(created));
                ComponentPresetsWindow.Open(created);
            });

            menu.DropDown(field.ButtonBounds);
        }

        private static void Select(SerializedObject serializedObject, string presetPath, string id) {
            serializedObject.Update();
            serializedObject.FindProperty(presetPath).stringValue = id;
            serializedObject.ApplyModifiedProperties();

            ComponentPresets.NotifyChanged();
        }
    }
}
