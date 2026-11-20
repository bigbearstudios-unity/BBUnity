using UnityEngine;
using UnityEngine.Serialization;
using BBUnity.Entities.Building;


namespace BBUnity.Combat {

    /// <summary>
    /// Lives on a hurtbox collider — typically a child of the entity, kept separate from the
    /// main movement collider so it can sit on its own physics layer (a GameObject's layer
    /// applies to every collider on it). Forwards a resolved hit up to the entity's own
    /// StatisticCombatController, which is the only thing that knows how to mitigate/apply it —
    /// this class has no combat logic of its own.
    /// </summary>
    [AddComponentMenu("BBUnity/Combat/Attack Receiver")]
    [CapabilityComponent]
    public class AttackReceiver : MonoBehaviour {

        [SerializeField, Tooltip("The StatisticCombatController this hurtbox reports hits to. Found automatically (this GameObject or its parents) if left unassigned.")]
        [FormerlySerializedAs("_attributeController")]
        private StatisticCombatController _statisticController;

        private void Awake() {
            if (_statisticController == null) {
                // GetComponentInParent, not GetComponentInChildren: per the class doc comment
                // above, this typically lives on a hurtbox *child* and forwards hits *up* to the
                // entity's own StatisticCombatController on the root — GetComponentInChildren
                // can only ever find one further down the hierarchy from here, never on an
                // ancestor, so it silently never matched that shape at all.
                _statisticController = GetComponentInParent<StatisticCombatController>();
            }

            if (_statisticController == null) {
                Debug.LogWarning($"AttackReceiver on '{gameObject.name}' has no StatisticCombatController assigned or found on itself/its parents.", this);
            }
        }

        public void ReceiveHit(HitInfo hit) {
            if (_statisticController == null) {
                Debug.LogWarning($"AttackReceiver on '{gameObject.name}' received a hit but has no StatisticCombatController — it was ignored.", this);
                return;
            }

            _statisticController.ApplyHit(hit);
        }
    }
}
