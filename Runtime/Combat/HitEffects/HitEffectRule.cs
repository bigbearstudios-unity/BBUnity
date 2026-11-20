using UnityEngine;

using System.Collections.Generic;
using BBUnity.Tags;

namespace BBUnity.Combat.HitEffects {

    /// <summary>
    /// One row of a HitEffectTable: "when a hit carrying these attack tags lands on a receiver
    /// with these surface tags, play these effects". Either list can be empty, meaning "any" — a
    /// row with both empty is the default that applies when nothing more specific does.
    /// </summary>
    [System.Serializable]
    public class HitEffectRule {

        [SerializeField, Tooltip("The hit must carry all of these tags (e.g. Fire). Empty matches any hit.")]
        private List<CombatTagDefinition> _attackTags = new List<CombatTagDefinition>();

        [SerializeField, Tooltip("The receiver must have all of these surface tags (e.g. Foliage). Empty matches any receiver.")]
        private List<CombatTagDefinition> _receiverTags = new List<CombatTagDefinition>();

        [SerializeField]
        private HitEffectList _effects = new HitEffectList();

        public IReadOnlyList<CombatTagDefinition> AttackTags {
            get { return _attackTags; }
        }

        public IReadOnlyList<CombatTagDefinition> ReceiverTags {
            get { return _receiverTags; }
        }

        public HitEffectList Effects {
            get { return _effects; }
        }

        /// <summary>
        /// How many conditions this row sets — the more it has, the more specific it is, and the
        /// more it wins over a general row.
        /// </summary>
        public int Specificity {
            get { return _attackTags.Count + _receiverTags.Count; }
        }

        public bool Matches(IReadOnlyList<CombatTagDefinition> attackTags, IReadOnlyList<CombatTagDefinition> surfaceTags) {
            return ContainsAll(attackTags, _attackTags) && ContainsAll(surfaceTags, _receiverTags);
        }

        private static bool ContainsAll(IReadOnlyList<CombatTagDefinition> have, List<CombatTagDefinition> required) {
            foreach (CombatTagDefinition tag in required) {
                if (tag == null) { continue; }

                bool found = false;
                for (int i = 0; i < have.Count; i++) {
                    if (have[i] == tag) { found = true; break; }
                }

                if (!found) { return false; }
            }

            return true;
        }
    }
}
