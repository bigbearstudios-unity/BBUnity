using UnityEngine;

using System.Collections.Generic;
using BBUnity.Combat.HitEffects;
using BBUnity.Tags;

namespace BBUnity.Combat {

    /// <summary>
    /// Something a combatant can do that affects another combatant, defined as an asset rather
    /// than on the entity: what it does to the target (DamageEntries, resolved exactly like an
    /// Attack's), the Tags every resulting hit carries, and how soon it can be used again. A game
    /// subclasses it to add whatever else its own actions need (a parry's window, a fireball's
    /// range, ...) — BBUnity only knows the shared part.
    ///
    /// Using one is up to the caller: check StatisticCombatController.TryUse() to respect the
    /// cooldown, then build the hit with CreateHit(action) and give it to the target's ApplyHit().
    /// Costs (mana, stamina, ...) are not modeled yet.
    /// </summary>
    [CreateAssetMenu(fileName = "Action_", menuName = "BBUnity/Combat/Combat Action Definition")]
    public class CombatActionDefinition : ScriptableObject {

        [SerializeField, Min(0), Tooltip("Milliseconds after a successful TryUse() before it can be used again. 0 means no cooldown.")]
        private int _cooldown = 0;

        [SerializeField, Tooltip("Carried by every hit this action produces — see CombatTagDefinition.")]
        private List<CombatTagDefinition> _tags = new List<CombatTagDefinition>();

        [SerializeField, Tooltip("Played at the point of contact every time this lands, whatever it hits — e.g. the spark of the blade. What the receiver does in response is up to the receiver.")]
        private HitEffectList _hitEffects = new HitEffectList();

        [SerializeField, Tooltip("What this action does to its target, per statistic.")]
        private List<DamageEntry> _damageEntries = new List<DamageEntry>();

        public int Cooldown {
            get { return _cooldown; }
        }

        public IReadOnlyList<CombatTagDefinition> Tags {
            get { return _tags; }
        }

        public HitEffectList HitEffects {
            get { return _hitEffects; }
        }

        public IReadOnlyList<DamageEntry> DamageEntries {
            get { return _damageEntries; }
        }
    }
}
