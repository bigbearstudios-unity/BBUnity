using UnityEngine.InputSystem;
using BBUnity.Input.Actions;


namespace BBUnity.Input.Mappings {

    [System.Serializable]
    public class UnityMovementActionMapping : UnityAxisActionMapping {

        public UnityMovementActionMapping() {
            _action = "Movement";
            _type = MappingType.Movement;
        }

        public override UnityInputAction BuildAction(PlayerInput input) {
            return new UnityInputAxisAction(input, this);
        }
    }
}