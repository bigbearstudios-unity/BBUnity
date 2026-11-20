using UnityEngine;

namespace BBUnity.Combat.HitEffects {

    /// <summary>
    /// Everything a HitEffect can use to decide what to play and where: the hit itself, the
    /// combatant it landed on, and the point of contact.
    /// </summary>
    public readonly struct HitContext {

        public readonly HitInfo Hit;
        public readonly StatisticCombatController Receiver;

        /// <summary>
        /// Where the hit landed — the contact point if the hit carried one, otherwise the
        /// receiver's position.
        /// </summary>
        public readonly Vector3 Point;

        public HitContext(HitInfo hit, StatisticCombatController receiver) {
            Hit = hit;
            Receiver = receiver;
            Point = hit.HasPoint ? hit.Point : receiver.transform.position;
        }

        public GameObject Source {
            get { return Hit.Source; }
        }

        /// <summary>
        /// +1 if the hit traveled left to right (the receiver is to the right of the source), -1
        /// for right to left, and +1 when there is no source or they share an x position.
        /// </summary>
        public float Direction {
            get {
                if (Source == null) { return 1f; }

                float diff = Receiver.transform.position.x - Source.transform.position.x;
                return Mathf.Approximately(diff, 0f) ? 1f : Mathf.Sign(diff);
            }
        }
    }
}
