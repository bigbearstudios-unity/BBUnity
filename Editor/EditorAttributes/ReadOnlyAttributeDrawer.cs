using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BBUnity.Editor.EditorAttributes {
    [CustomPropertyDrawer(typeof(BBUnity.EditorAttributes.ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var field = new PropertyField(property);
            field.SetEnabled(false);
            return field;
        }
    }
}
