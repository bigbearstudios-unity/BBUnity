using UnityEditor;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Gameplay.Entities.Controllers.Combat;

namespace BBUnity.Editor.Gameplay.Entities.Controllers.Combat {

    /// <summary>
    /// The default inspector, plus the same de-aliasing pass the StatisticCombatController
    /// inspector runs over its Attacks: a CombatActionDefinition's DamageEntries hold [SerializeReference]
    /// DamageValues, and Unity's list "+" would otherwise leave a new entry sharing its
    /// predecessor's value (see ManagedReferenceDeduplicator). Applies to a game's subclasses too.
    /// </summary>
    [CustomEditor(typeof(CombatActionDefinition), true)]
    public class CombatActionDefinitionEditor : UnityEditor.Editor {

        public override VisualElement CreateInspectorGUI() {
            VisualElement root = InspectorElements.CreateDefault(serializedObject);
            ManagedReferenceDeduplicator.Track(root, serializedObject, "_damageEntries", "_hitEffects");
            return root;
        }
    }
}
