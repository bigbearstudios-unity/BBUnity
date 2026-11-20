using UnityEngine;

namespace BBUnity.Gameplay.Entities.Building {

    /// <summary>
    /// Every commonly-tuned Rigidbody2D setting, applied to whichever Rigidbody2D EntityBuilder
    /// just added — see ComponentSettingsRegistry (Editor) for how a ComponentSpec picks this up
    /// automatically when its Component Type is set to Rigidbody2D.
    /// </summary>
    [System.Serializable]
    public class Rigidbody2DSettings : IComponentSettings {

        [SerializeField]
        private RigidbodyType2D _bodyType = RigidbodyType2D.Dynamic;

        [SerializeField]
        private float _mass = 1f;

        [SerializeField]
        private float _linearDamping = 0f;

        [SerializeField]
        private float _angularDamping = 0.05f;

        [SerializeField]
        private float _gravityScale = 1f;

        [SerializeField]
        private RigidbodyConstraints2D _constraints = RigidbodyConstraints2D.None;

        [SerializeField]
        private CollisionDetectionMode2D _collisionDetectionMode = CollisionDetectionMode2D.Discrete;

        [SerializeField]
        private RigidbodySleepMode2D _sleepMode = RigidbodySleepMode2D.StartAwake;

        [SerializeField]
        private RigidbodyInterpolation2D _interpolation = RigidbodyInterpolation2D.None;

        public void Apply(Component instance) {
            if (instance is not Rigidbody2D rigidbody) { return; }

            rigidbody.bodyType = _bodyType;
            rigidbody.mass = _mass;
            rigidbody.linearDamping = _linearDamping;
            rigidbody.angularDamping = _angularDamping;
            rigidbody.gravityScale = _gravityScale;
            rigidbody.constraints = _constraints;
            rigidbody.collisionDetectionMode = _collisionDetectionMode;
            rigidbody.sleepMode = _sleepMode;
            rigidbody.interpolation = _interpolation;
        }
    }
}
