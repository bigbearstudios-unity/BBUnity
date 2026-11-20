using UnityEngine;

namespace BBUnity.Gameplay.Entities.Controllers.Combat {

    /// <summary>
    /// 2D implementation of AttackCollider — requires a Collider2D (trigger) and forwards
    /// OnTriggerEnter2D into the base's dimension-agnostic hit processing.
    /// </summary>
    [AddComponentMenu("BBUnity/Gameplay/Entities/Controllers/Combat/Attack Collider 2D")]
    [RequireComponent(typeof(Collider2D))]
    public class AttackCollider2D : AttackCollider {

        private void OnTriggerEnter2D(Collider2D col) {
            ProcessHit(col.gameObject, (Vector3)col.ClosestPoint(transform.position));
        }
    }
}
