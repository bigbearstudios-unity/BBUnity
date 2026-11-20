using UnityEngine;

namespace BBUnity.Entities.Building {

    /// <summary>
    /// Every commonly-tuned Rigidbody (3D) setting, applied to whichever Rigidbody EntityBuilder
    /// just added. Vampire Empire itself is 2D-only (see Rigidbody2DSettings) — this exists for
    /// other BBUnity-based projects, same reasoning as Entity3D.
    /// </summary>
    [System.Serializable]
    public class RigidbodySettings : IComponentSettings {

        [SerializeField]
        private float _mass = 1f;

        [SerializeField]
        private float _linearDamping = 0f;

        [SerializeField]
        private float _angularDamping = 0.05f;

        [SerializeField]
        private bool _useGravity = true;

        [SerializeField]
        private bool _isKinematic = false;

        [SerializeField]
        private RigidbodyConstraints _constraints = RigidbodyConstraints.None;

        [SerializeField]
        private CollisionDetectionMode _collisionDetectionMode = CollisionDetectionMode.Discrete;

        [SerializeField]
        private RigidbodyInterpolation _interpolation = RigidbodyInterpolation.None;

        public void Apply(Component instance) {
            if (instance is not Rigidbody rigidbody) { return; }

            rigidbody.mass = _mass;
            rigidbody.linearDamping = _linearDamping;
            rigidbody.angularDamping = _angularDamping;
            rigidbody.useGravity = _useGravity;
            rigidbody.isKinematic = _isKinematic;
            rigidbody.constraints = _constraints;
            rigidbody.collisionDetectionMode = _collisionDetectionMode;
            rigidbody.interpolation = _interpolation;
        }
    }
}
