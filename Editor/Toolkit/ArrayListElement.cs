using System;
using System.Text;

using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BBUnity.Editor.Toolkit {

    /// <summary>
    /// A serialized array drawn as a column of boxed rows, each with a title, move up / move
    /// down / remove buttons and a caller-built body. Adding is left to the caller (a type
    /// picker, a plain button, ...), who then calls Rebuild(). It rebuilds itself when the array's
    /// size or managed-reference layout changes underneath it (undo, redo, another editor).
    /// </summary>
    internal class ArrayListElement : VisualElement {

        private readonly SerializedObject _serializedObject;
        private readonly string _path;
        private readonly Func<SerializedProperty, VisualElement> _createBody;
        private readonly Func<SerializedProperty, int, string> _getTitle;
        private readonly Action<SerializedProperty> _removing;
        private readonly bool _reorderable;
        private readonly VisualElement _rows = new VisualElement();

        private string _signature;

        /// <summary>Raised after every rebuild; lets a caller refresh anything derived from the list.</summary>
        public event Action Rebuilt;

        public ArrayListElement(
            SerializedObject serializedObject,
            string arrayPath,
            Func<SerializedProperty, VisualElement> createBody,
            Func<SerializedProperty, int, string> getTitle,
            Action<SerializedProperty> onRemoving = null,
            bool reorderable = true
        ) {
            _serializedObject = serializedObject;
            _path = arrayPath;
            _createBody = createBody;
            _getTitle = getTitle;
            _removing = onRemoving;
            _reorderable = reorderable;

            Add(_rows);
            Rebuild();

            schedule.Execute(CheckForExternalChange).Every(250);
        }

        public void Rebuild() {
            _serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty array = _serializedObject.FindProperty(_path);

            _rows.Clear();
            for (int i = 0; i < array.arraySize; i++) {
                _rows.Add(CreateRow(array.GetArrayElementAtIndex(i), i, array.arraySize));
            }

            _signature = Signature(array);

            if (panel != null) { _rows.Bind(_serializedObject); }

            Rebuilt?.Invoke();
        }

        private void CheckForExternalChange() {
            _serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty array = _serializedObject.FindProperty(_path);

            if (array != null && Signature(array) != _signature) { Rebuild(); }
        }

        // Size plus each managed element's reference id: catches add / remove / reorder / type
        // change from undo and redo without rebuilding while someone is typing in a field.
        private static string Signature(SerializedProperty array) {
            var builder = new StringBuilder();
            builder.Append(array.arraySize);

            for (int i = 0; i < array.arraySize; i++) {
                SerializedProperty element = array.GetArrayElementAtIndex(i);
                if (element.propertyType == SerializedPropertyType.ManagedReference) {
                    builder.Append(',').Append(element.managedReferenceId);
                }
            }

            return builder.ToString();
        }

        private VisualElement CreateRow(SerializedProperty element, int index, int count) {
            VisualElement box = InspectorElements.CreateBox();

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;

            var title = new Label(_getTitle(element, index));
            title.style.flexGrow = 1;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(title);

            if (_reorderable) {
                Button up = CreateSmallButton("▲", () => Move(index, index - 1));
                up.SetEnabled(index > 0);
                header.Add(up);

                Button down = CreateSmallButton("▼", () => Move(index, index + 1));
                down.SetEnabled(index < count - 1);
                header.Add(down);
            }

            header.Add(CreateSmallButton("✕", () => Remove(index)));
            box.Add(header);

            VisualElement body = _createBody(element);
            if (body != null) { box.Add(body); }

            return box;
        }

        private static Button CreateSmallButton(string text, Action onClick) {
            var button = new Button(onClick) { text = text };
            button.style.width = 22;
            button.style.marginLeft = button.style.marginRight = 1;
            return button;
        }

        private void Move(int from, int to) {
            _serializedObject.Update();
            SerializedProperty array = _serializedObject.FindProperty(_path);
            if (to < 0 || to >= array.arraySize) { return; }

            array.MoveArrayElement(from, to);
            _serializedObject.ApplyModifiedProperties();
            Rebuild();
        }

        private void Remove(int index) {
            _serializedObject.Update();
            SerializedProperty array = _serializedObject.FindProperty(_path);
            SerializedProperty element = array.GetArrayElementAtIndex(index);

            _removing?.Invoke(element);

            // Clearing the element first makes DeleteArrayElementAtIndex remove it rather than
            // just nulling it (the behavior for object and managed references).
            if (element.propertyType == SerializedPropertyType.ManagedReference) {
                element.managedReferenceValue = null;
            } else if (element.propertyType == SerializedPropertyType.ObjectReference && element.objectReferenceValue != null) {
                element.objectReferenceValue = null;
            }

            array.DeleteArrayElementAtIndex(index);
            _serializedObject.ApplyModifiedProperties();
            Rebuild();
        }
    }
}
