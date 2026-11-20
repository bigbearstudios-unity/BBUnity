using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;

namespace BBUnity.Editor.Support.ComponentManagement {

    /// <summary>
    /// One match on a single row: a Type / Namespace selector, then a type picker or a namespace
    /// text field to suit.
    /// </summary>
    [CustomPropertyDrawer(typeof(ComponentMatch))]
    public class ComponentMatchDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            SerializedProperty kind = property.FindPropertyRelative("_kind");
            SerializedProperty typeName = property.FindPropertyRelative("_typeName");
            SerializedProperty namespacePrefix = property.FindPropertyRelative("_namespace");

            SerializedObject serializedObject = property.serializedObject;
            string typeNamePath = typeName.propertyPath;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            var kindField = new PropertyField(kind, string.Empty);
            kindField.style.width = 100;
            row.Add(kindField);

            var typePicker = new PickerField(null, picker => {
                var dropdown = new TypeDropdown(new AdvancedDropdownState(), GetAvailableTypes(), type => {
                    serializedObject.Update();
                    serializedObject.FindProperty(typeNamePath).stringValue = type.FullName;
                    serializedObject.ApplyModifiedProperties();
                }, "Component Type");

                dropdown.Show(picker.ButtonBounds);
            });
            typePicker.style.flexGrow = 1;
            typePicker.Track(typeName, changed => string.IsNullOrEmpty(changed.stringValue) ? "Not Selected" : changed.stringValue);
            row.Add(typePicker);

            var namespaceField = new TextField { tooltip = "A namespace prefix, e.g. Unity.Cinemachine. Matches that namespace and everything below it." };
            namespaceField.style.flexGrow = 1;
            namespaceField.BindProperty(namespacePrefix);
            row.Add(namespaceField);

            void UpdateVisibility() {
                bool isType = kind.enumValueIndex == (int)ComponentMatch.MatchKind.Type;
                InspectorElements.SetVisible(typePicker, isType);
                InspectorElements.SetVisible(namespaceField, !isType);
            }

            row.TrackPropertyValue(kind, _ => UpdateVisibility());
            UpdateVisibility();

            return row;
        }

        // Abstract bases (Collider2D, Behaviour...) are the useful ones for rules, so unlike the
        // entity-building pickers nothing is filtered out beyond what can't be named.
        private static List<Type> GetAvailableTypes() {
            return TypeCache.GetTypesDerivedFrom<Component>()
                .Where(t => t.IsPublic && !t.IsGenericTypeDefinition)
                .OrderBy(t => t.FullName)
                .ToList();
        }
    }
}
