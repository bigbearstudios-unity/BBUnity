using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;

using BBUnity.Editor.Gameplay.Entities.Controllers;
using BBUnity.Editor.Toolkit;
using BBUnity.Gameplay.Entities.Building;

namespace BBUnity.Editor.Gameplay.Entities.Building {

    /// <summary>
    /// Renders a UnityComponentTypeReference as a single full-width dropdown button — no prefix
    /// label, since each entry in BBUnitySettings.Entities.AdditionalComponentTypes is self-explanatory in
    /// a list of nothing but these. Offers every non-abstract Component type in a UnityEngine.*
    /// namespace; deliberately never BBUnity's own types — those are already covered elsewhere
    /// (see ComponentTypeReferenceDrawer), and this list exists specifically for the ones that
    /// aren't.
    /// </summary>
    [CustomPropertyDrawer(typeof(UnityComponentTypeReference))]
    public class UnityComponentTypeReferenceDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            SerializedProperty typeNameProperty = property.FindPropertyRelative("_typeName");
            SerializedObject serializedObject = property.serializedObject;
            string typeNamePath = typeNameProperty.propertyPath;

            var field = new PickerField(null, picker => {
                var dropdown = new TypeDropdown(new AdvancedDropdownState(), GetAvailableUnityComponentTypes(), type => {
                    serializedObject.Update();
                    serializedObject.FindProperty(typeNamePath).stringValue = type.FullName;
                    serializedObject.ApplyModifiedProperties();
                }, "Unity Component");

                dropdown.Show(picker.ButtonBounds);
            });

            return field.Track(typeNameProperty, changed => {
                Type currentType = ResolveDisplayedType(changed.stringValue);
                return currentType != null ? TypeDropdown.BuildLabel(currentType) : "Not Selected";
            });
        }

        private static Type ResolveDisplayedType(string typeName) {
            if (string.IsNullOrEmpty(typeName)) { return null; }

            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                Type type = assembly.GetType(typeName);
                if (type != null) { return type; }
            }

            return null;
        }

        private static List<Type> GetAvailableUnityComponentTypes() {
            return TypeCache.GetTypesDerivedFrom<Component>()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition && t.Namespace != null && t.Namespace.StartsWith("UnityEngine"))
                .OrderBy(t => t.FullName)
                .ToList();
        }
    }
}
