using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Gameplay.Entities.Controllers;

namespace BBUnity.Editor.Toolkit {

    /// <summary>
    /// "Pick a concrete type, then edit its fields inline" for a single [SerializeReference]
    /// field: a type dropdown on top of the chosen instance's own fields. Picking a type replaces
    /// the field's value in place (there is exactly one per slot — this isn't a list).
    /// </summary>
    internal class ManagedReferenceField : VisualElement {

        private readonly SerializedObject _serializedObject;
        private readonly string _path;
        private readonly PickerField _picker;
        private readonly VisualElement _children = new VisualElement();

        private long _lastReferenceId = long.MinValue;

        public ManagedReferenceField(SerializedProperty property, string label, Func<List<Type>> getTypes, string dropdownTitle) {
            _serializedObject = property.serializedObject;
            _path = property.propertyPath;

            _picker = new PickerField(label, field => {
                var dropdown = new TypeDropdown(new AdvancedDropdownState(), getTypes(), Pick, dropdownTitle);
                dropdown.Show(field.ButtonBounds);
            });

            _children.style.marginLeft = 15;

            Add(_picker);
            Add(_children);

            Refresh();
            schedule.Execute(CheckForExternalChange).Every(250);
        }

        private void Pick(Type type) {
            _serializedObject.Update();
            _serializedObject.FindProperty(_path).managedReferenceValue = Activator.CreateInstance(type);
            _serializedObject.ApplyModifiedProperties();
            Refresh();
        }

        private void CheckForExternalChange() {
            _serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty property = _serializedObject.FindProperty(_path);

            if (property != null && property.managedReferenceId != _lastReferenceId) { Refresh(); }
        }

        private void Refresh() {
            _serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty property = _serializedObject.FindProperty(_path);
            _lastReferenceId = property.managedReferenceId;

            object instance = property.managedReferenceValue;
            _picker.Text = instance != null ? TypeDropdown.BuildLabel(instance.GetType()) : "(none)";

            _children.Clear();
            if (instance != null) { InspectorElements.AddChildren(_children, property); }

            if (panel != null) { _children.Bind(_serializedObject); }
        }
    }
}
