using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Generation.Parameters;

namespace BBUnity.Editor.Generation {

    /// <summary>Shared layout of IntParameter and FloatParameter: a mode selector, then one value or a from-to pair.</summary>
    internal static class ParameterDrawers {

        public static VisualElement Create<TField>(SerializedProperty property) where TField : VisualElement, IBindable, new() {
            SerializedProperty mode = property.FindPropertyRelative("_mode");
            SerializedProperty fixedValue = property.FindPropertyRelative("_fixed");
            SerializedProperty min = property.FindPropertyRelative("_min");
            SerializedProperty max = property.FindPropertyRelative("_max");

            var row = new VisualElement();
            row.AddToClassList("unity-base-field");
            row.AddToClassList("unity-base-field__aligned");
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            var label = new Label(property.displayName);
            label.AddToClassList("unity-base-field__label");
            row.Add(label);

            var modeField = new PropertyField(mode, string.Empty);
            modeField.style.width = 80;
            row.Add(modeField);

            TField single = Bound<TField>(fixedValue);
            TField from = Bound<TField>(min);
            TField to = Bound<TField>(max);
            var dash = new Label("to");
            dash.style.marginLeft = dash.style.marginRight = 4;

            row.Add(single);
            row.Add(from);
            row.Add(dash);
            row.Add(to);

            void Update() {
                bool isFixed = mode.enumValueIndex == (int)ParameterMode.Fixed;
                single.style.display = isFixed ? DisplayStyle.Flex : DisplayStyle.None;
                from.style.display = to.style.display = dash.style.display = isFixed ? DisplayStyle.None : DisplayStyle.Flex;
            }

            row.TrackPropertyValue(mode, _ => Update());
            Update();

            return row;
        }

        private static TField Bound<TField>(SerializedProperty property) where TField : VisualElement, IBindable, new() {
            var field = new TField();
            field.style.flexGrow = 1;
            field.style.flexBasis = 0;
            field.BindProperty(property);
            return field;
        }
    }

    [CustomPropertyDrawer(typeof(IntParameter))]
    public class IntParameterDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            return ParameterDrawers.Create<IntegerField>(property);
        }
    }

    [CustomPropertyDrawer(typeof(FloatParameter))]
    public class FloatParameterDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            return ParameterDrawers.Create<FloatField>(property);
        }
    }
}
