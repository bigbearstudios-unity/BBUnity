using UnityEngine;
using UnityEngine.InputSystem;

using BBUnity.Gameplay.Entities.Controllers.Input.Actions;

namespace BBUnity.Gameplay.Entities.Controllers.Input.Mappings {

    [System.Serializable]
    public class UnityButtonActionMapping : UnityActionMapping {

        [SerializeField, Tooltip("The amount of time the input will stay buffered, in milliseconds")]
        float _bufferThresholdMs = 0.0f;

        /// <summary>
        /// The buffer threshold in seconds, for InputBuffer/Update(delta) — both work in
        /// seconds to match Time.deltaTime. _bufferThresholdMs is milliseconds purely as an
        /// inspector-facing unit; converted here at the one point it matters.
        /// </summary>
        public float BufferThreshold {
            get { return _bufferThresholdMs / 1000f; }
        }

        public UnityButtonActionMapping() {
            _type = MappingType.Button;
        }

        /// <summary>
        /// Builds the action. In the case of a button action it can build either a 
        /// UnityInputBufferedButtonAction or UnityInputButtonAction depending on
        /// the inputBufferThreshold
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override UnityInputAction BuildAction(PlayerInput input) {
            if(_bufferThresholdMs > float.Epsilon) {
                return new UnityInputBufferedButtonAction(input, this);
            } else {
                return new UnityInputButtonAction(input, this);
            }
        }
    }
}