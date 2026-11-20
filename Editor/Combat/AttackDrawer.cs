using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Combat;
using BBUnity.Editor.Toolkit;

namespace BBUnity.Editor.Combat {

    /// <summary>
    /// Replaces the default "Element N" label on each Attacks list entry with its Id, and warns
    /// inline if its AttackCollider prefab isn't set — PerformAttack() no-ops (with its own
    /// runtime warning) without one, so it's worth catching at edit time too. DamageEntries is
    /// drawn via Unity's default list rendering — DamageEntryDrawer already gives each of
    /// its own entries a sensible label, and it's a plain (non-[SerializeReference]) list so the
    /// default "+"/"-" buttons work without the abstract-type problem SerializeReference lists have.
    ///
    /// AttackCollider is picked via a custom dropdown rather than Unity's default Object field —
    /// since AttackCollider is abstract (see AttackCollider2D), Unity's built-in Object Picker
    /// window is unreliable at finding prefabs whose only matching component is a concrete
    /// subclass of an abstract Component type (drag-and-drop still works; the picker's own search
    /// doesn't). Same fix as EnemySpawnNodeEditor's Enemy dropdown, and for the same reason.
    /// </summary>
    [CustomPropertyDrawer(typeof(Attack))]
    public class AttackDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            SerializedObject serializedObject = property.serializedObject;
            SerializedProperty idProperty = property.FindPropertyRelative("_id");
            SerializedProperty attackColliderProperty = property.FindPropertyRelative("_attackColliderPrefab");
            string attackColliderPath = attackColliderProperty.propertyPath;

            var root = new VisualElement();

            var idField = new PropertyField(idProperty);
            void UpdateIdLabel(SerializedProperty id) {
                idField.label = string.IsNullOrEmpty(id.stringValue) ? "(Unnamed Attack)" : id.stringValue;
            }
            UpdateIdLabel(idProperty);
            idField.TrackPropertyValue(idProperty, UpdateIdLabel);
            root.Add(idField);

            var colliderField = new PickerField("Attack Collider", picker => {
                SerializedProperty colliderProperty = serializedObject.FindProperty(attackColliderPath);
                AttackCollider current = colliderProperty.objectReferenceValue as AttackCollider;
                List<AttackCollider> available = FindAvailableAttackColliderPrefabs();

                var menu = new GenericMenu();
                if (available.Count == 0) {
                    menu.AddDisabledItem(new GUIContent("(no AttackCollider prefabs found in the project)"));
                }

                foreach (AttackCollider attackCollider in available) {
                    menu.AddItem(new GUIContent(attackCollider.gameObject.name), attackCollider == current, () => {
                        serializedObject.Update();
                        serializedObject.FindProperty(attackColliderPath).objectReferenceValue = attackCollider;
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.DropDown(picker.ButtonBounds);
            });
            root.Add(colliderField);

            HelpBox missingCollider = InspectorElements.CreateHelpBox("No AttackCollider prefab set — this attack will do nothing when performed.", HelpBoxMessageType.Warning);
            root.Add(missingCollider);

            void UpdateCollider(SerializedProperty collider) {
                AttackCollider current = collider.objectReferenceValue as AttackCollider;
                colliderField.Text = current != null ? current.gameObject.name : "(none)";
                InspectorElements.SetVisible(missingCollider, current == null);
            }
            UpdateCollider(attackColliderProperty);
            colliderField.TrackPropertyValue(attackColliderProperty, UpdateCollider);

            root.Add(new PropertyField(property.FindPropertyRelative("_spawnPosition"), "Spawn Position"));
            root.Add(new PropertyField(property.FindPropertyRelative("_tags"), "Tags"));
            root.Add(new PropertyField(property.FindPropertyRelative("_hitEffects"), "Hit Effects"));
            root.Add(new PropertyField(property.FindPropertyRelative("_damageEntries"), "Damage Entries"));

            return root;
        }

        private static List<AttackCollider> FindAvailableAttackColliderPrefabs() {
            var results = new List<AttackCollider>();

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab")) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                AttackCollider attackCollider = prefab != null ? prefab.GetComponent<AttackCollider>() : null;

                if (attackCollider != null) { results.Add(attackCollider); }
            }

            return results.OrderBy(attackCollider => attackCollider.gameObject.name).ToList();
        }
    }
}
