using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BBUnity.Editor.Toolkit {

    /// <summary>
    /// Small helpers shared by BBUnity's UI Toolkit inspectors and drawers: the UI Toolkit
    /// equivalents of DrawPropertiesExcluding, "every visible child of this property", help boxes
    /// and boxed sections.
    /// </summary>
    internal static class InspectorElements {

        /// <summary>The direct visible children of a property, each as its own copy.</summary>
        public static IEnumerable<SerializedProperty> VisibleChildren(SerializedProperty property) {
            SerializedProperty iterator = property.Copy();
            SerializedProperty end = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end)) {
                enterChildren = false;
                yield return iterator.Copy();
            }
        }

        /// <summary>Adds a PropertyField for every visible child of the property.</summary>
        public static void AddChildren(VisualElement container, SerializedProperty property) {
            foreach (SerializedProperty child in VisibleChildren(property)) {
                container.Add(new PropertyField(child));
            }
        }

        /// <summary>
        /// The default inspector minus the excluded property paths. The script field is drawn
        /// disabled, like Unity's own, unless "m_Script" is excluded.
        /// </summary>
        public static VisualElement CreateDefault(SerializedObject serializedObject, params string[] excluded) {
            var container = new VisualElement();
            var excludedPaths = new HashSet<string>(excluded);

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren)) {
                enterChildren = false;
                if (excludedPaths.Contains(iterator.propertyPath)) { continue; }

                var field = new PropertyField(iterator.Copy());
                if (iterator.propertyPath == "m_Script") { field.SetEnabled(false); }
                container.Add(field);
            }

            return container;
        }

        public static HelpBox CreateHelpBox(string message, HelpBoxMessageType type) {
            return new HelpBox(message, type);
        }

        public static void SetVisible(VisualElement element, bool visible) {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static Label CreateHeading(string text) {
            var label = new Label(text);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginTop = 6;
            label.style.marginBottom = 2;
            return label;
        }

        public static VisualElement CreateBox() {
            var box = new VisualElement();
            Color border = new Color(0.5f, 0.5f, 0.5f, 0.4f);

            box.style.borderTopWidth = box.style.borderBottomWidth = box.style.borderLeftWidth = box.style.borderRightWidth = 1;
            box.style.borderTopColor = box.style.borderBottomColor = box.style.borderLeftColor = box.style.borderRightColor = border;
            box.style.borderTopLeftRadius = box.style.borderTopRightRadius = box.style.borderBottomLeftRadius = box.style.borderBottomRightRadius = 3;
            box.style.paddingTop = box.style.paddingBottom = 3;
            box.style.paddingLeft = box.style.paddingRight = 4;
            box.style.marginTop = 2;
            return box;
        }
    }
}
