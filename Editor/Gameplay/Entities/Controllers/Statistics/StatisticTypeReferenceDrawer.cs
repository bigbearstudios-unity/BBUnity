using System;

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;

namespace BBUnity.Editor.Gameplay.Entities.Controllers.Statistics {

    /// <summary>
    /// Renders an StatisticTypeReference as a dropdown button (reusing the same TypeDropdown and
    /// type scan as StatisticController's own "Add Statistic" button) instead of a raw type-name
    /// string field the developer would have to type correctly by hand.
    /// </summary>
    [CustomPropertyDrawer(typeof(BBUnity.Gameplay.Entities.Controllers.Combat.StatisticTypeReference))]
    public class StatisticTypeReferenceDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            return Create(property, property.displayName);
        }

        // Shared with the entry drawers that want their own label ("Target Statistic").
        internal static VisualElement Create(SerializedProperty property, string label) {
            SerializedProperty typeNameProperty = property.FindPropertyRelative("_statisticTypeName");
            SerializedObject serializedObject = property.serializedObject;
            string typeNamePath = typeNameProperty.propertyPath;

            var field = new PickerField(label, picker => {
                var dropdown = new TypeDropdown(new AdvancedDropdownState(), StatisticControllerEditor.GetAvailableStatisticTypes(), type => {
                    serializedObject.Update();
                    serializedObject.FindProperty(typeNamePath).stringValue = type.AssemblyQualifiedName;
                    serializedObject.ApplyModifiedProperties();
                }, "Statistic Type");

                dropdown.Show(picker.ButtonBounds);
            });

            return field.Track(typeNameProperty, changed => {
                Type currentType = string.IsNullOrEmpty(changed.stringValue) ? null : Type.GetType(changed.stringValue);
                return currentType != null ? TypeDropdown.BuildLabel(currentType) : "(none)";
            });
        }
    }
}
