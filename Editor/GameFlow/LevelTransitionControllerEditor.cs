using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.GameFlow;

namespace BBUnity.Editor.GameFlow {

    /// <summary>
    /// Shows only the settings that belong to the chosen Transition Type.
    /// </summary>
    [CustomEditor(typeof(LevelTransitionController))]
    public class LevelTransitionControllerEditor : UnityEditor.Editor {

        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();

            SerializedProperty typeProperty = serializedObject.FindProperty("_transitionType");
            root.Add(new PropertyField(typeProperty, "Transition Type"));

            root.Add(new PropertyField(serializedObject.FindProperty("_minimumTransitionTime"), "Minimum Transition Time (ms)"));
            root.Add(new PropertyField(serializedObject.FindProperty("_stopTimeOnTransition"), "Stop Time on Transition"));
            root.Add(new PropertyField(serializedObject.FindProperty("_transitionCooldown"), "Transition Cooldown (ms)"));

            var fade = new VisualElement();
            fade.Add(new PropertyField(serializedObject.FindProperty("_fadeColor"), "Fade Color"));
            fade.Add(new PropertyField(serializedObject.FindProperty("_fadeOutTime"), "Fade Out Time (ms)"));
            fade.Add(new PropertyField(serializedObject.FindProperty("_fadeInTime"), "Fade In Time (ms)"));
            root.Add(fade);

            HelpBox loadingScreen = InspectorElements.CreateHelpBox("Loading Screen isn't built yet — level transitions cut instantly for now.", HelpBoxMessageType.Info);
            root.Add(loadingScreen);

            void Update(SerializedProperty property) {
                var type = (LevelTransitionType)property.enumValueIndex;
                InspectorElements.SetVisible(fade, type == LevelTransitionType.Fade);
                InspectorElements.SetVisible(loadingScreen, type == LevelTransitionType.LoadingScreen);
            }

            Update(typeProperty);
            root.TrackPropertyValue(typeProperty, Update);

            return root;
        }
    }
}
