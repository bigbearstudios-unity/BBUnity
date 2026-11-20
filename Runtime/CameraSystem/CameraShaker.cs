using UnityEngine;

namespace BBUnity.CameraSystem {

    /// <summary>
    /// Shakes the game camera. Abstract so the code asking for a shake (ScreenShakeEffect) doesn't
    /// depend on how the camera is driven; the component that actually does it — see
    /// CinemachineCameraShaker — sits with the camera and registers itself as Current while it is
    /// enabled. Asking for a shake with no shaker in the scene does nothing.
    /// </summary>
    public abstract class CameraShaker : MonoBehaviour {

        /// <summary>
        /// The enabled shaker, or null if there isn't one.
        /// </summary>
        public static CameraShaker Current { get; private set; }

        protected virtual void OnEnable() {
            Current = this;
        }

        protected virtual void OnDisable() {
            if (Current == this) { Current = null; }
        }

        /// <param name="strength">How hard to shake; 1 is a moderate hit.</param>
        /// <param name="duration">How long the shake lasts, in milliseconds.</param>
        /// <param name="direction">The direction of the shake. Need not be normalized.</param>
        public abstract void Shake(float strength, int duration, Vector2 direction);
    }
}
