using System;

using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BBUnity.Editor.Toolkit {

    /// <summary>
    /// A labeled, full-width button that looks like an inspector field and opens a picker (a
    /// TypeDropdown or GenericMenu) when clicked. The UI Toolkit counterpart of
    /// EditorGUI.DropdownButton behind a PrefixLabel.
    /// </summary>
    internal class PickerField : VisualElement {

        private readonly Button _button;

        public PickerField(string label, Action<PickerField> onClicked) {
            AddToClassList("unity-base-field");
            AddToClassList("unity-base-field__aligned");
            style.flexDirection = FlexDirection.Row;

            if (!string.IsNullOrEmpty(label)) {
                var labelElement = new Label(label);
                labelElement.AddToClassList("unity-base-field__label");
                Add(labelElement);
            }

            _button = new Button(() => onClicked(this));
            _button.style.flexGrow = 1;
            _button.style.unityTextAlign = TextAnchor.MiddleLeft;
            Add(_button);
        }

        public string Text {
            get { return _button.text; }
            set { _button.text = value; }
        }

        /// <summary>Where a popup should anchor — the button, in panel space.</summary>
        public Rect ButtonBounds {
            get { return _button.worldBound; }
        }

        /// <summary>Shows textFor(property) on the button now and whenever the property changes.</summary>
        public PickerField Track(SerializedProperty property, Func<SerializedProperty, string> textFor) {
            Text = textFor(property);
            this.TrackPropertyValue(property, changed => Text = textFor(changed));
            return this;
        }
    }
}
