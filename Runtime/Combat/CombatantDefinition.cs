using UnityEngine;
using UnityEngine.Serialization;

using System.Collections.Generic;


using BBUnity.Combat.HitEffects;
using BBUnity.Statistics;
using BBUnity.Tags;
namespace BBUnity.Combat {

    /// <summary>
    /// Reusable combat data for an StatisticCombatController, kept in an asset instead of on
    /// each prefab — one "Goblin" definition shared by every goblin prefab, one "Player" one, and
    /// so on. Assigned in the controller's Definition field.
    ///
    /// The controller never touches this asset's data at runtime: it takes its own copy on
    /// Awake, so damage and buffs on one entity can't leak into the asset or another entity.
    ///
    /// The definition is additive. Anything set directly on the controller still works, and wins
    /// over the definition where the two overlap:
    /// - Statistics: a controller statistic with the same Name replaces the definition's.
    /// - Reductions: if the controller has any reduction for an statistic, the definition's
    ///   reductions for that statistic are ignored.
    /// - Surface tags: the controller's are added to the definition's.
    /// - Receiver hit effects: the most specific matching row from either table plays; the
    ///   controller's wins a tie.
    /// </summary>
    [CreateAssetMenu(fileName = "New Combatant Definition", menuName = "BBUnity/Combat/Combatant Definition")]
    public class CombatantDefinition : ScriptableObject {

        [SerializeField]
        [FormerlySerializedAs("_attributes")]
        private EntityStatisticList _statistics = new EntityStatisticList();

        [SerializeField, Tooltip("What this combatant mitigates, per statistic, when hit — same as the controller's own Reductions.")]
        private List<ReductionEntry> _reductions = new List<ReductionEntry>();

        [SerializeField, Tooltip("What this combatant is made of (Flesh, Bone, Foliage, ...), for choosing how it looks when hit. Added to the controller's own Surface Tags.")]
        private List<CombatTagDefinition> _surfaceTags = new List<CombatTagDefinition>();

        [SerializeField, Tooltip("How this combatant looks when hit — same as the controller's own Receiver Hit Effects.")]
        private HitEffectTable _receiverHitEffects = new HitEffectTable();

        public IReadOnlyList<CombatTagDefinition> SurfaceTags {
            get { return _surfaceTags; }
        }

        public HitEffectTable ReceiverHitEffects {
            get { return _receiverHitEffects; }
        }

        public EntityStatisticList Statistics {
            get { return _statistics; }
        }

        public IReadOnlyList<ReductionEntry> Reductions {
            get { return _reductions; }
        }
    }
}
