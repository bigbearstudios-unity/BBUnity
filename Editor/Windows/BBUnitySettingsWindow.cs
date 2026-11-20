using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Core;
using BBUnity.Editor.Toolkit;

namespace BBUnity.Editor.Windows {

    /// <summary>
    /// Edits the project's BBUnitySettings. BBUnitySettings is loaded through Resources, so it
    /// must live at Assets/Resources/BBUnitySettings.asset; when the project has none, the window
    /// offers a button that creates it there.
    /// </summary>
    public class BBUnitySettingsWindow : EditorWindow {

        private const string ResourcesFolder = "Assets/Resources";
        private const string SettingsAssetPath = ResourcesFolder + "/BBUnitySettings.asset";

        private VisualElement _content;

        [MenuItem("Window/BBUnity/Settings")]
        private static void Open() {
            GetWindow<BBUnitySettingsWindow>("BBUnity Settings");
        }

        public void CreateGUI() {
            _content = new ScrollView();
            _content.style.flexGrow = 1;
            _content.style.paddingLeft = _content.style.paddingRight = 6;
            _content.style.paddingTop = 6;
            rootVisualElement.Add(_content);

            Refresh();
        }

        private void OnEnable() {
            EditorApplication.projectChanged += OnProjectChanged;
        }

        private void OnDisable() {
            EditorApplication.projectChanged -= OnProjectChanged;
        }

        // Settings created, deleted or moved outside this window.
        private void OnProjectChanged() {
            if (_content != null) { Refresh(); }
        }

        private void Refresh() {
            _content.Clear();

            BBUnitySettings settings = BBUnitySettings.Load();
            if (settings != null) {
                _content.Add(new InspectorElement(settings));
                return;
            }

            _content.Add(InspectorElements.CreateHelpBox(
                $"This project has no BBUnity Settings. They are loaded from {SettingsAssetPath}.",
                HelpBoxMessageType.Info
            ));

            _content.Add(new Button(CreateSettings) { text = "Create Settings" });
        }

        private void CreateSettings() {
            if (!AssetDatabase.IsValidFolder(ResourcesFolder)) {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            var settings = CreateInstance<BBUnitySettings>();
            AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            AssetDatabase.SaveAssets();

            EditorGUIUtility.PingObject(settings);
            Refresh();
        }
    }
}
