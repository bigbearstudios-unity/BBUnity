using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;

using BBUnity.Core;
using BBUnity.Editor.Gameplay.Entities.Controllers;
using BBUnity.Editor.Toolkit;
using BBUnity.Gameplay.Entities.Building;
using BBUnity.Gameplay.Entities.Controllers;

namespace BBUnity.Editor.Gameplay.Entities.Building {

    /// <summary>
    /// Renders a ComponentTypeReference as a dropdown button, reusing the same TypeDropdown
    /// widget StatisticTypeReferenceDrawer/StateControllerEditor use. The offered types are
    /// deliberately curated rather than "every Component in the project":
    /// - anything deriving from EntityController (BBUnity's own controllers are all fair game
    ///   automatically)
    /// - anything explicitly marked [CapabilityComponent] (BBUnity types that aren't
    ///   EntityControllers, e.g. AttackReceiver)
    /// - whatever built-in Unity types a project has opted into via BBUnitySettings —
    ///   BBUnity can't statistic types it doesn't own (Rigidbody2D, BoxCollider2D, ...), so
    ///   those are configured there instead.
    ///
    /// When the picked type has a registered settings class (see ComponentSettingsRegistry), the
    /// sibling ComponentSpec.Settings field is populated automatically — a no-op when this
    /// ComponentTypeReference isn't hosted by a ComponentSpec (e.g. SubclassSlot.BaseType), since
    /// there's no such sibling to find.
    /// </summary>
    [CustomPropertyDrawer(typeof(ComponentTypeReference))]
    public class ComponentTypeReferenceDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            SerializedProperty typeNameProperty = property.FindPropertyRelative("_typeName");
            SerializedObject serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;
            string typeNamePath = typeNameProperty.propertyPath;

            var field = new PickerField(property.displayName, picker => {
                var dropdown = new TypeDropdown(new AdvancedDropdownState(), GetAvailableComponentTypes(), type => {
                    serializedObject.Update();
                    serializedObject.FindProperty(typeNamePath).stringValue = type.AssemblyQualifiedName;
                    UpdateSiblingSettings(serializedObject, propertyPath, type);
                    serializedObject.ApplyModifiedProperties();
                }, "Component Type");

                dropdown.Show(picker.ButtonBounds);
            });

            return field.Track(typeNameProperty, changed => {
                Type currentType = string.IsNullOrEmpty(changed.stringValue) ? null : Type.GetType(changed.stringValue);
                return currentType != null ? TypeDropdown.BuildLabel(currentType) : "(none)";
            });
        }

        // ComponentSpec.Settings lives next to this ComponentTypeReference (as ComponentSpec._type),
        // not on it — reached by trimming the last path segment off this property's own path to
        // get back to the parent ComponentSpec. Silently does nothing if there's no such sibling
        // (e.g. SubclassSlot.BaseType, which isn't hosted by a ComponentSpec at all).
        private static void UpdateSiblingSettings(SerializedObject serializedObject, string typeReferencePath, Type pickedType) {
            int lastDot = typeReferencePath.LastIndexOf('.');
            if (lastDot < 0) { return; }

            SerializedProperty parent = serializedObject.FindProperty(typeReferencePath.Substring(0, lastDot));
            SerializedProperty settingsProperty = parent?.FindPropertyRelative("_settings");
            if (settingsProperty == null) { return; }

            settingsProperty.managedReferenceValue = ComponentSettingsRegistry.CreateSettingsFor(pickedType);
        }

        private static List<Type> GetAvailableComponentTypes() {
            var types = new HashSet<Type>();

            foreach (Type type in TypeCache.GetTypesDerivedFrom<EntityController>()) {
                if (!type.IsAbstract) { types.Add(type); }
            }

            foreach (Type type in TypeCache.GetTypesWithAttribute<CapabilityComponentAttribute>()) {
                if (!type.IsAbstract && typeof(Component).IsAssignableFrom(type)) { types.Add(type); }
            }

            BBUnitySettings settings = BBUnitySettings.Load();
            if (settings != null) {
                foreach (UnityComponentTypeReference reference in settings.Entities.AdditionalComponentTypes) {
                    Type type = reference?.ResolveType();
                    if (type != null) { types.Add(type); }
                }
            }

            return types.OrderBy(t => t.FullName).ToList();
        }
    }
}
