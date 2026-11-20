using UnityEngine.InputSystem;
using BBUnity.Input.Actions;


namespace BBUnity.Input.Mappings {

    [System.Serializable]
    public class UnityAxisActionMapping : UnityActionMapping {

        public UnityAxisActionMapping() {
            _type = MappingType.Axis;
        }

        public override UnityInputAction BuildAction(PlayerInput input) {
            return new UnityInputAxisAction(input, this);
        }
    }
}