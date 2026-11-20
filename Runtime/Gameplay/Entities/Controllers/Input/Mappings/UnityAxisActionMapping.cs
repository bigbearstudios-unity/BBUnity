using UnityEngine.InputSystem;

using BBUnity.Gameplay.Entities.Controllers.Input.Actions;

namespace BBUnity.Gameplay.Entities.Controllers.Input.Mappings {

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