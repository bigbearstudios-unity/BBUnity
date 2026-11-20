using System;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Entities.Building;

namespace BBUnity.Editor.Entities.Building {

    /// <summary>
    /// Labels a ComponentSpec list element with its picked component's name (e.g. "Rigidbody2D")
    /// instead of Unity's default "Element 0" — otherwise indistinguishable at a glance once a
    /// definition has more than one or two. Only overrides the label; everything else (the Type
    /// dropdown, FieldWirings list, Settings if any) still renders exactly as the default
    /// property drawer would, as the foldout's children.
    /// </summary>
    [CustomPropertyDrawer(typeof(ComponentSpec))]
    public class ComponentSpecDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            SerializedObject serializedObject = property.serializedObject;
            string path = property.propertyPath;
            SerializedProperty typeNameProperty = property.FindPropertyRelative("_type._typeName");
            SerializedProperty settingsProperty = property.FindPropertyRelative("_settings");

            var foldout = new Foldout { value = property.isExpanded, text = BuildLabel(typeNameProperty) };
            foldout.RegisterValueChangedCallback(change => {
                if (change.target != foldout) { return; }

                SerializedProperty spec = serializedObject.FindProperty(path);
                if (spec != null) { spec.isExpanded = change.newValue; }
            });

            var children = new VisualElement();
            foldout.Add(children);

            void Rebuild() {
                serializedObject.UpdateIfRequiredOrScript();
                SerializedProperty spec = serializedObject.FindProperty(path);

                children.Clear();
                InspectorElements.AddChildren(children, spec);
                if (children.panel != null) { children.Bind(serializedObject); }
            }

            Rebuild();

            foldout.TrackPropertyValue(typeNameProperty, changed => {
                foldout.text = BuildLabel(changed);
                Rebuild();
            });

            return foldout;
        }

        private static string BuildLabel(SerializedProperty typeNameProperty) {
            if (typeNameProperty == null || string.IsNullOrEmpty(typeNameProperty.stringValue)) {
                return "(no type)";
            }

            Type type = Type.GetType(typeNameProperty.stringValue);
            return type != null ? TypeDropdown.BuildLabel(type) : "(unresolved type)";
        }
    }
}
