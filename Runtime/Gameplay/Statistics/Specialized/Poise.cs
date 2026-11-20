using UnityEngine;
using UnityEngine.Serialization;

using BBUnity.Gameplay.Statistics.Generic;

namespace BBUnity.Gameplay.Statistics.Specialized {
    
    [System.Serializable]
    public class Poise : DynamicIntegerStatistic {

        /// <summary>
        /// The time it takes for the poise to regenerate, in milliseconds. Once this time is hit
        /// the poise will be set to its maximum value instantly
        /// </summary>
        [SerializeField]
        private int _regenerationTime = 5000;
        
        /// <summary>
        /// The internal timer for the regeneration. Once triggered all
        /// poise values get reset to their default values. Counts down in
        /// milliseconds.
        /// </summary>
        [SerializeField, EditorAttributes.ReadOnly]
        private float _regenerationTimer = 0.0f;

        /// <summary>
        /// The threshold for when 'flinch' should be defined. This is reset upon
        /// 'flinching'
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_flitchThreshold")]
        private int _flinchThreshold;
        private int _flinchCounter = 0;

        public bool Staggered { get { return _current <= _minimum; } }
        public bool Flinched { get { return _flinchCounter > _flinchThreshold; } }

        private void ResetRegenerationTimer() {
            _regenerationTimer = _regenerationTime;
        }

        private void IncreaseFlinchValue(int amount) {
            _flinchCounter += amount;
        }

        public override void TakeDamage(int damage) {
            if (IsZero) { return; }

            DecreaseBy(damage);
            IncreaseFlinchValue(damage);
            ResetRegenerationTimer();
        }

        public void ResetFlinchCounter() {
            _flinchCounter = 0;
        }

        public override void Update(float deltaTime) {
            base.Update(deltaTime);

            if(_current < Maximum) {
                _regenerationTimer -= deltaTime * 1000f;

                if(_regenerationTimer <= 0.0f) {
                    Reset();
                }
            }
        }

        public override void Reset() {
            base.Reset();

            ResetFlinchCounter();
            ResetRegenerationTimer();
        }
    }
}