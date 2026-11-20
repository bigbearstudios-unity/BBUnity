using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BBUnity.Editor.EditorAttributes {

    /// <summary>
    /// A dropdown of the project's sorting layers that stores the chosen layer's id, as
    /// Unity's own Sorting Layer fields do.
    /// </summary>
    [CustomPropertyDrawer(typeof(BBUnity.EditorAttributes.SortingLayerAttribute))]
    public class SortingLayerAttributeDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            SerializedObject serializedObject = property.serializedObject;
            string path = property.propertyPath;

            var dropdown = new DropdownField(property.displayName);
            dropdown.AddToClassList("unity-base-field__aligned");

            Refresh(dropdown, property.intValue);

            dropdown.RegisterValueChangedCallback(change => {
                SortingLayer[] layers = SortingLayer.layers;
                int index = dropdown.index;
                if (index < 0 || index >= layers.Length) { return; }

                serializedObject.Update();
                serializedObject.FindProperty(path).intValue = layers[index].id;
                serializedObject.ApplyModifiedProperties();
            });

            dropdown.TrackPropertyValue(property, changed => Refresh(dropdown, changed.intValue));

            return dropdown;
        }

        private static void Refresh(DropdownField dropdown, int layerId) {
            SortingLayer[] layers = SortingLayer.layers;
            List<string> names = layers.Select(layer => layer.name).ToList();

            int index = System.Array.FindIndex(layers, layer => layer.id == layerId);
            if (index < 0) {
                names.Add($"(missing layer {layerId})");
                index = names.Count - 1;
            }

            dropdown.choices = names;
            dropdown.SetValueWithoutNotify(names[index]);
        }
    }
}
