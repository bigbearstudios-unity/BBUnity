using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Support.Scenes;

namespace BBUnity.Editor.Support.Scenes {

    /// <summary>
    /// Renders a SceneReference as a dropdown of the scenes currently enabled in Build Settings —
    /// that's deliberate, not just "every scene in the project": a scene disabled or missing from
    /// Build Settings can't actually be loaded at runtime via SceneManager, so offering it here
    /// would just be a trap.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            SerializedProperty sceneNameProperty = property.FindPropertyRelative("_sceneName");
            SerializedObject serializedObject = property.serializedObject;
            string sceneNamePath = sceneNameProperty.propertyPath;

            var field = new PickerField(property.displayName, picker => {
                serializedObject.UpdateIfRequiredOrScript();
                string current = serializedObject.FindProperty(sceneNamePath).stringValue;
                string[] sceneNames = GetBuildSettingsSceneNames();

                var menu = new GenericMenu();
                if (sceneNames.Length == 0) {
                    menu.AddDisabledItem(new GUIContent("(no scenes enabled in Build Settings)"));
                }

                foreach (string sceneName in sceneNames) {
                    menu.AddItem(new GUIContent(sceneName), sceneName == current, () => {
                        serializedObject.Update();
                        serializedObject.FindProperty(sceneNamePath).stringValue = sceneName;
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.DropDown(picker.ButtonBounds);
            });

            return field.Track(sceneNameProperty, changed => string.IsNullOrEmpty(changed.stringValue) ? "(none)" : changed.stringValue);
        }

        internal static string[] GetBuildSettingsSceneNames() {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                .ToArray();
        }

        public static string GetScenePath(string sceneName) {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
                if (scene.enabled && Path.GetFileNameWithoutExtension(scene.path) == sceneName) {
                    return scene.path;
                }
            }

            return null;
        }
    }
}
