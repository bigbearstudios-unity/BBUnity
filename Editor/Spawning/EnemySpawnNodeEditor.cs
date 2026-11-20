using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Spawning;

namespace BBUnity.Editor.Spawning {

    /// <summary>
    /// Renders Enemy as a dropdown of every Enemy-marked prefab in the project rather than a
    /// plain drag-and-drop object field — a full Editor rather than a PropertyDrawer, since
    /// Unity's CustomPropertyDrawer hook doesn't apply to Component-typed (ObjectReference)
    /// fields, only to plain serializable classes/structs and PropertyAttributes.
    /// </summary>
    [CustomEditor(typeof(EnemySpawnNode))]
    public class EnemySpawnNodeEditor : UnityEditor.Editor {

        public override VisualElement CreateInspectorGUI() {
            VisualElement root = InspectorElements.CreateDefault(serializedObject, "_enemy", "m_Script");
            SerializedProperty enemyProperty = serializedObject.FindProperty("_enemy");

            var picker = new PickerField("Enemy", field => {
                serializedObject.UpdateIfRequiredOrScript();
                Enemy current = serializedObject.FindProperty("_enemy").objectReferenceValue as Enemy;
                List<Enemy> available = FindAvailableEnemyPrefabs();

                var menu = new GenericMenu();
                if (available.Count == 0) {
                    menu.AddDisabledItem(new GUIContent("(no Enemy prefabs found in the project)"));
                }

                foreach (Enemy enemy in available) {
                    menu.AddItem(new GUIContent(enemy.gameObject.name), enemy == current, () => {
                        serializedObject.Update();
                        serializedObject.FindProperty("_enemy").objectReferenceValue = enemy;
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.DropDown(field.ButtonBounds);
            });

            picker.Track(enemyProperty, property => {
                Enemy current = property.objectReferenceValue as Enemy;
                return current != null ? current.gameObject.name : "(none)";
            });

            root.Add(picker);
            return root;
        }

        private static List<Enemy> FindAvailableEnemyPrefabs() {
            var results = new List<Enemy>();

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab")) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Enemy enemy = prefab != null ? prefab.GetComponent<Enemy>() : null;

                if (enemy != null) { results.Add(enemy); }
            }

            return results.OrderBy(enemy => enemy.gameObject.name).ToList();
        }
    }
}
