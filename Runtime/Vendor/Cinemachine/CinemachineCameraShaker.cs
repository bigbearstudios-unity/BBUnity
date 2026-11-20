using UnityEngine;

using Unity.Cinemachine;

using BBUnity.CameraSystem;

namespace BBUnity.Vendor.Cinemachine {

    /// <summary>
    /// Shakes the camera with a Cinemachine impulse: each Shake generates an impulse from the
    /// CinemachineImpulseSource on this GameObject, which a CinemachineImpulseListener on the
    /// camera (matching Impulse Channel) turns into movement. Lives in its own assembly that only
    /// compiles when Cinemachine is installed, so the rest of BBUnity doesn't depend on it.
    /// </summary>
    [AddComponentMenu("BBUnity/Vendor/Cinemachine/Cinemachine Camera Shaker")]
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CinemachineCameraShaker : CameraShaker {

        [SerializeField, Tooltip("The impulse's curve over its duration. Bump is a single sharp push that eases back.")]
        private CinemachineImpulseDefinition.ImpulseShapes _shape = CinemachineImpulseDefinition.ImpulseShapes.Bump;

        private CinemachineImpulseSource _source;

        private void Awake() {
            _source = GetComponent<CinemachineImpulseSource>();
        }

        public override void Shake(float strength, int duration, Vector2 direction) {
            if (_source == null || strength <= 0f || duration <= 0) { return; }

            // Legacy (the source's default) needs a Raw Signal asset and makes no impulse without one.
            _source.ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
            _source.ImpulseDefinition.ImpulseShape = _shape;
            _source.ImpulseDefinition.ImpulseDuration = duration / 1000f;
            _source.GenerateImpulseWithVelocity(new Vector3(direction.x, direction.y, 0f).normalized * strength);
        }
    }
}
