using UnityEngine;

namespace BBUnity.Utilities {

    /// <summary>
    /// Keeps this object reading the right way round when something above it is flipped by a
    /// negative scale — put it on a floating health bar, a text label or a VFX anchor that sits
    /// under a character which turns around by flipping its root (see EnemyAIController). Each
    /// frame (and as soon as it is enabled) it sets the sign of its own localScale.x to match its
    /// parent's, so the combined scale stays positive; its size is left alone.
    ///
    /// Only the horizontal flip is handled, which is all a 2D character turning left/right needs.
    /// </summary>
    [AddComponentMenu("BBUnity/Utilities/Flip Compensator")]
    public class FlipCompensator : MonoBehaviour {

        private void OnEnable() {
            Apply();
        }

        private void LateUpdate() {
            Apply();
        }

        private void Apply() {
            Transform parent = transform.parent;
            if (parent == null) { return; }

            float sign = parent.lossyScale.x < 0f ? -1f : 1f;

            Vector3 scale = transform.localScale;
            float desired = Mathf.Abs(scale.x) * sign;
            if (scale.x == desired) { return; }

            scale.x = desired;
            transform.localScale = scale;
        }
    }
}
