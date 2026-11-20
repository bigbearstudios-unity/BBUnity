using UnityEngine;

using System.Collections.Generic;
using BBUnity.Combat.HitEffects;
using BBUnity.Tags;

namespace BBUnity.Combat {

    /// <summary>
    /// A hit's raw, pre-mitigation numbers — resolved by the attacker's
    /// StatisticCombatController (see Attack.DamageEntries) before being handed to the
    /// defender's AttackReceiver. The defender's own StatisticCombatController.ApplyHit()
    /// applies its Reductions to these before touching any statistic — neither side needs to know
    /// how the other is configured, or which statistics the other even has.
    /// </summary>
    public readonly struct HitInfo {

        public readonly GameObject Source;
        public readonly IReadOnlyList<StatisticDamage> Damages;

        /// <summary>
        /// The tags of the Attack or CombatActionDefinition that produced this hit; empty if it had none.
        /// </summary>
        public readonly IReadOnlyList<CombatTagDefinition> Tags;

        /// <summary>
        /// The effects the Attack or CombatActionDefinition behind this hit plays whenever it lands,
        /// whatever it landed on; null if it has none.
        /// </summary>
        public readonly HitEffectList ActionEffects;

        /// <summary>
        /// Where the hit made contact, in world space. Only meaningful when HasPoint is true.
        /// </summary>
        public readonly Vector3 Point;

        public readonly bool HasPoint;

        public HitInfo(GameObject source, IReadOnlyList<StatisticDamage> damages, IReadOnlyList<CombatTagDefinition> tags = null, HitEffectList actionEffects = null)
            : this(source, damages, tags, actionEffects, default, false) { }

        private HitInfo(GameObject source, IReadOnlyList<StatisticDamage> damages, IReadOnlyList<CombatTagDefinition> tags, HitEffectList actionEffects, Vector3 point, bool hasPoint) {
            Source = source;
            Damages = damages;
            Tags = tags ?? System.Array.Empty<CombatTagDefinition>();
            ActionEffects = actionEffects;
            Point = point;
            HasPoint = hasPoint;
        }

        /// <summary>
        /// A copy of this hit that made contact at the given world position.
        /// </summary>
        public HitInfo WithPoint(Vector3 point) {
            return new HitInfo(Source, Damages, Tags, ActionEffects, point, true);
        }

        /// <summary>
        /// True if the Attack or CombatActionDefinition behind this hit carries the given tag.
        /// </summary>
        public bool HasTag(CombatTagDefinition tag) {
            if (tag == null) { return false; }

            foreach (CombatTagDefinition candidate in Tags) {
                if (candidate == tag) { return true; }
            }

            return false;
        }
    }
}
