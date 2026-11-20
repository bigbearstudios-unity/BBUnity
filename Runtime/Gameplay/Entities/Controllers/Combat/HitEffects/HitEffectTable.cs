using UnityEngine;

using System.Collections.Generic;

namespace BBUnity.Gameplay.Entities.Controllers.Combat.HitEffects {

    /// <summary>
    /// What a combatant looks like when it is hit, as a list of HitEffectRules. Only the most
    /// specific matching row plays (the one with the most conditions; the earliest wins a tie), so
    /// a general row is a fallback that a specific one replaces rather than adds to. Plain data:
    /// it can sit directly on a controller or inside a CombatantDefinition.
    /// </summary>
    [System.Serializable]
    public class HitEffectTable {

        [SerializeField]
        private List<HitEffectRule> _rules = new List<HitEffectRule>();

        public IReadOnlyList<HitEffectRule> Rules {
            get { return _rules; }
        }

        /// <summary>
        /// The most specific rule matching the hit's tags and the receiver's surface tags, or null
        /// if none match.
        /// </summary>
        public HitEffectRule Resolve(IReadOnlyList<CombatTagDefinition> attackTags, IReadOnlyList<CombatTagDefinition> surfaceTags) {
            HitEffectRule best = null;

            foreach (HitEffectRule rule in _rules) {
                if (rule == null || !rule.Matches(attackTags, surfaceTags)) { continue; }

                if (best == null || rule.Specificity > best.Specificity) { best = rule; }
            }

            return best;
        }
    }
}
