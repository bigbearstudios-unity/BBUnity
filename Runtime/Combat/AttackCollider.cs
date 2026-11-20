using System.Collections.Generic;

using UnityEngine;

using BBUnity.Pools;

namespace BBUnity.Combat {

    /// <summary>
    /// Lives on a spawned attack effect (a "slash") rather than on the attacker's body — this
    /// project's attack effects are themselves the hitbox, since different attacks need
    /// differently shaped/sized/timed ones rather than sharing one fixed collider on the
    /// attacker. Carries no damage numbers itself: _attackId just looks one up on the attacker's
    /// StatisticCombatController.Attacks at the moment of impact (see
    /// StatisticCombatController.CreateHit).
    ///
    /// Effects are pooled, so this can't hold a baked-in attacker reference —
    /// StatisticCombatController.PerformAttack() calls SetSource()/SetAttackId() right after
    /// spawning one. IPoolBehavior.OnSpawn is how it forgets who it already hit between reuses
    /// (see BBUnity.Pools.PoolBehavior).
    ///
    /// Dimension-agnostic on purpose — no physics/collision detection lives here, since that's
    /// inherently 2D- or 3D-specific (Collider2D vs Collider, OnTriggerEnter2D vs
    /// OnTriggerEnter). See AttackCollider2D for the 2D implementation; a subclass just needs to
    /// call ProcessHit() from whatever collision callback its dimension uses.
    /// </summary>
    public abstract class AttackCollider : MonoBehaviour, IPoolBehavior {

        // Always set at runtime via SetAttackId() (see PerformAttack) — not [SerializeField],
        // there's nothing to configure by hand in the Editor.
        private string _attackId;

        private GameObject _source;
        private readonly HashSet<AttackReceiver> _hitReceivers = new HashSet<AttackReceiver>();

        public void SetSource(GameObject source) {
            _source = source;
        }

        public void SetAttackId(string attackId) {
            _attackId = attackId;
        }

        /// <summary>
        /// Call this from whatever collision/trigger callback the concrete dimension uses, with
        /// whatever GameObject was hit, and the point of contact if the collision gave one.
        /// </summary>
        protected void ProcessHit(GameObject other, Vector3? contactPoint = null) {
            if (_source == null) {
                Debug.LogWarning($"AttackCollider on '{gameObject.name}' has no source set — SetSource() must be called before it can register hits (see StatisticCombatController.PerformAttack).", this);
                return;
            }

            if (other == _source) { return; }

            AttackReceiver receiver = other.GetComponentInParent<AttackReceiver>();
            if (receiver == null || _hitReceivers.Contains(receiver)) { return; }

            StatisticCombatController attacker = _source.GetComponent<StatisticCombatController>();
            if (attacker == null) {
                Debug.LogWarning($"AttackCollider on '{gameObject.name}' has a source ('{_source.name}') with no StatisticCombatController — it can't deal damage.", this);
                return;
            }

            _hitReceivers.Add(receiver);

            HitInfo hit = attacker.CreateHit(_attackId);
            if (contactPoint.HasValue) { hit = hit.WithPoint(contactPoint.Value); }

            receiver.ReceiveHit(hit);
        }

        public void OnCreate(PoolBehavior behavior) { }

        public void OnSpawn(PoolBehavior behavior) {
            _hitReceivers.Clear();
        }

        public void OnReturn(PoolBehavior behavior) {
            _source = null;
        }
    }
}
