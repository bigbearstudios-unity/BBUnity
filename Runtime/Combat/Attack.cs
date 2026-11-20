using UnityEngine;

using System.Collections.Generic;
using BBUnity.Combat.HitEffects;
using BBUnity.Tags;

namespace BBUnity.Combat {

    /// <summary>
    /// One named attack on an StatisticCombatController's Attacks list — e.g. a boss with 5
    /// attacks has 5 of these, each fully self-contained: which AttackCollider prefab to spawn,
    /// where to spawn it, and a list of DamageEntries describing which statistics it damages and
    /// by how much. Triggered via StatisticCombatController.PerformAttack(Id).
    /// </summary>
    [System.Serializable]
    public class Attack {

        [SerializeField, Tooltip("Looked up by StatisticCombatController.PerformAttack(id).")]
        private string _id;

        [SerializeField, Tooltip("A prefab carrying an AttackCollider component — dragging in a GameObject without one is rejected by this field, since it's typed as the component rather than GameObject.")]
        private AttackCollider _attackColliderPrefab;

        [SerializeField, Tooltip("Where the AttackCollider is spawned — typically a child Transform on this entity, e.g. a weapon tip.")]
        private Transform _spawnPosition;

        [SerializeField, Tooltip("Carried by every hit this attack produces — see CombatTagDefinition.")]
        private List<CombatTagDefinition> _tags = new List<CombatTagDefinition>();

        [SerializeField, Tooltip("Played at the point of contact every time this lands, whatever it hits — e.g. the spark of the blade. What the receiver does in response is up to the receiver.")]
        private HitEffectList _hitEffects = new HitEffectList();

        [SerializeField]
        private List<DamageEntry> _damageEntries = new List<DamageEntry>();

        public string Id {
            get { return _id; }
        }

        public AttackCollider AttackColliderPrefab {
            get { return _attackColliderPrefab; }
        }

        public Transform SpawnPosition {
            get { return _spawnPosition; }
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
