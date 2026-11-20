using UnityEngine;

using BBUnity.CameraSystem;

namespace BBUnity.Combat.HitEffects {

    /// <summary>
    /// Shakes the camera when a hit lands, through whichever CameraShaker is in the scene.
    /// </summary>
    [System.Serializable]
    public class ScreenShakeEffect : HitEffect {

        [SerializeField, Min(0f), Tooltip("How hard the camera shakes; 1 is a moderate hit.")]
        private float _strength = 1f;

        [SerializeField, Min(0), Tooltip("How long the shake lasts, in milliseconds.")]
        private int _duration = 150;

        [SerializeField, Tooltip("When on, the camera is pushed along the direction of the hit; when off it shakes in a random direction.")]
        private bool _alongHit = true;

        public override void Play(HitContext context) {
            CameraShaker shaker = CameraShaker.Current;
            if (shaker == null) { return; }

            Vector2 direction = _alongHit ? new Vector2(context.Direction, 0f) : Random.insideUnitCircle.normalized;
            shaker.Shake(_strength, _duration, direction);
        }
    }
}
