using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using TMPro;

using BBUnity.Editor.Toolkit;
using BBUnity.Gameplay.UI.FloatingText;

namespace BBUnity.Editor.Gameplay.UI.FloatingText {

    /// <summary>
    /// Adds the "TextMeshPro isn't available" warning on top of the default inspector — checked
    /// via TMP_Settings.instance, the same signal Unity's own TextMeshPro tooling uses to decide
    /// whether TMP Essentials have been imported into the project.
    /// </summary>
    [CustomEditor(typeof(FloatingTextController), editorForChildClasses: true)]
    public class FloatingTextControllerEditor : UnityEditor.Editor {

        public override VisualElement CreateInspectorGUI() {
            VisualElement root = InspectorElements.CreateDefault(serializedObject);

            HelpBox warning = InspectorElements.CreateHelpBox(
                "TextMeshPro is not available in this project — import TMP Essentials (Window > TextMeshPro > Import TMP Essential Resources), or switch Renderer Type to Unity.",
                HelpBoxMessageType.Warning
            );
            root.Add(warning);

            SerializedProperty rendererTypeProperty = serializedObject.FindProperty("_rendererType");
            void Update(SerializedProperty property) {
                var rendererType = (FloatingTextRendererType)property.enumValueIndex;
                InspectorElements.SetVisible(warning, rendererType == FloatingTextRendererType.TextMeshPro && TMP_Settings.instance == null);
            }

            Update(rendererTypeProperty);
            root.TrackPropertyValue(rendererTypeProperty, Update);

            return root;
        }
    }
}
