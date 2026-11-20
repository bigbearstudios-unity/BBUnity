using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Gameplay.Entities.Controllers.Statistics;
using BBUnity.Editor.Toolkit;
using BBUnity.Gameplay.Entities.Controllers;

namespace BBUnity.Editor.Gameplay.Entities.Controllers {

    // Inherits StatisticControllerEditor rather than duplicating it, so
    // StatisticCombatController still gets the exact same "Add Statistic" list/button
    // StatisticController itself has (the auto-seeded Health entry included).
    //
    // Attacks and Reductions are both plain (non-[SerializeReference]) lists — Attack and
    // ReductionEntry aren't polymorphic themselves, only fields inside them are, and those already
    // have their own registered drawers (AttackDrawer, ReductionEntryDrawer, and
    // the DamageValue/Reduction/StatisticTypeReference drawers they in turn rely on) — so Unity's
    // default list rendering (default "+"/"-", no abstract-type problem) is all that's needed here.
    [CustomEditor(typeof(StatisticCombatController), editorForChildClasses: true)]
    public class StatisticCombatControllerEditor : StatisticControllerEditor {

        protected override string[] GetExcludedProperties() {
            return new[] { "_entityStatistics", "_attacks", "_reductions", "m_Script" };
        }

        protected override void CreateAdditionalContent(VisualElement root) {
            root.Add(InspectorElements.CreateHeading("Combat"));
            root.Add(new PropertyField(serializedObject.FindProperty("_attacks")));
            root.Add(new PropertyField(serializedObject.FindProperty("_reductions")));

            // Attacks -> DamageEntries -> DamageValue are all nested under one property, so a
            // single recursive pass from each root catches (and repairs) aliasing anywhere in
            // that chain, and the receiver hit effects table holds [SerializeReference] effects
            // too — see ManagedReferenceDeduplicator for why this is needed at all.
            ManagedReferenceDeduplicator.Track(root, serializedObject, "_attacks", "_reductions", "_receiverHitEffects");
        }
    }
}
