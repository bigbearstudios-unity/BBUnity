using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;

namespace BBUnity.Editor.Support.ComponentManagement {

    /// <summary>A rule as a box: its category name, then the list of matches that put components in it.</summary>
    [CustomPropertyDrawer(typeof(ComponentCategoryRule))]
    public class ComponentCategoryRuleDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement box = InspectorElements.CreateBox();

            var category = new TextField("Category");
            category.BindProperty(property.FindPropertyRelative("_category"));
            box.Add(category);

            box.Add(new PropertyField(property.FindPropertyRelative("_matches"), "Matches"));

            return box;
        }
    }
}
