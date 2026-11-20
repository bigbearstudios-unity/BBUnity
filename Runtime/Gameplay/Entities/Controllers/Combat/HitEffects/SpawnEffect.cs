using UnityEngine;

namespace BBUnity.Gameplay.Entities.Controllers.Combat.HitEffects {

    /// <summary>
    /// Spawns a pooled prefab at the point of contact — a hit spark, a puff of dust. The prefab
    /// should switch itself off when it finishes (e.g. a SpriteAnimator set to deactivate), which
    /// returns it to the pool for the next hit.
    /// </summary>
    [System.Serializable]
    public class SpawnEffect : HitEffect {

        [SerializeField, Tooltip("The effect to spawn. Should deactivate itself when it has finished playing.")]
        private GameObject _prefab;

        [SerializeField, Tooltip("Added to the point of contact. X is mirrored when the hit travels right to left.")]
        private Vector2 _offset;

        [SerializeField, Tooltip("When on, the effect is flipped to point the way the hit traveled; when off it keeps the prefab's own orientation.")]
        private bool _faceHitDirection = true;

        public override void Play(HitContext context) {
            if (_prefab == null || context.Receiver == null) { return; }

            float direction = _faceHitDirection ? context.Direction : 1f;
            Vector3 position = context.Point + new Vector3(_offset.x * direction, _offset.y, 0f);

            GameObject instance = context.Receiver.SpawnPooled("HitEffect_" + _prefab.name, _prefab, position);
            if (instance == null) { return; }

            Vector3 scale = _prefab.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            instance.transform.localScale = scale;
        }
    }
}
