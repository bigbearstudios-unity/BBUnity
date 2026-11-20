using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BBUnity.Editor.EditorAttributes {
    [CustomPropertyDrawer(typeof(BBUnity.EditorAttributes.LayerAttribute))]
    public class LayerAttributeDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var field = new LayerField(property.displayName);
            field.BindProperty(property);
            return field;
        }
    }
}
