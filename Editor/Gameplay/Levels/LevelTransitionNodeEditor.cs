using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Support.Scenes;
using BBUnity.Editor.Toolkit;
using BBUnity.Gameplay.Levels;

namespace BBUnity.Editor.Gameplay.Levels {

    [CustomEditor(typeof(LevelTransitionNode))]
    public class LevelTransitionNodeEditor : UnityEditor.Editor {

        private VisualElement _target;

        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();

            SerializedProperty targetSceneProperty = serializedObject.FindProperty("_targetScene");
            SerializedProperty sceneNameProperty = targetSceneProperty.FindPropertyRelative("_sceneName");

            root.Add(new PropertyField(targetSceneProperty));

            // Everything below the scene picker depends on the chosen scene, so it's rebuilt
            // whenever the scene name changes.
            _target = new VisualElement();
            root.Add(_target);

            RefreshTarget();
            root.TrackPropertyValue(sceneNameProperty, _ => RefreshTarget());

            return root;
        }

        private void RefreshTarget() {
            serializedObject.UpdateIfRequiredOrScript();
            _target.Clear();

            string sceneName = serializedObject.FindProperty("_targetScene._sceneName").stringValue;
            if (string.IsNullOrEmpty(sceneName)) {
                _target.Add(InspectorElements.CreateHelpBox("No scene selected — pick a Target Scene above.", HelpBoxMessageType.Error));
                return;
            }

            string scenePath = SceneReferenceDrawer.GetScenePath(sceneName);
            List<PlayerSpawnNode> spawnNodes = scenePath != null
                ? SceneComponentScanner.FindComponentsInScene<PlayerSpawnNode>(scenePath)
                : new List<PlayerSpawnNode>();

            if (spawnNodes.Count == 0) {
                _target.Add(InspectorElements.CreateHelpBox($"Scene '{sceneName}' has no PlayerSpawnNodes.", HelpBoxMessageType.Error));
                return;
            }

            _target.Add(CreateSpawnNodeDropdown(spawnNodes));
        }

        private VisualElement CreateSpawnNodeDropdown(List<PlayerSpawnNode> spawnNodes) {
            SerializedProperty idProperty = serializedObject.FindProperty("_targetSpawnNodeId");

            var picker = new PickerField("Target Spawn Node", field => {
                serializedObject.UpdateIfRequiredOrScript();
                string currentId = serializedObject.FindProperty("_targetSpawnNodeId").stringValue;

                var menu = new GenericMenu();
                foreach (PlayerSpawnNode node in spawnNodes) {
                    string id = node.Id;
                    menu.AddItem(new GUIContent(id), id == currentId, () => {
                        serializedObject.Update();
                        serializedObject.FindProperty("_targetSpawnNodeId").stringValue = id;
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.DropDown(field.ButtonBounds);
            });

            return picker.Track(idProperty, property => string.IsNullOrEmpty(property.stringValue) ? "(none)" : property.stringValue);
        }
    }
}
