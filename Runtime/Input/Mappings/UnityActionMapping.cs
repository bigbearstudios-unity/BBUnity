using UnityEngine;
using UnityEngine.InputSystem;
using BBUnity.Input.Actions;

namespace BBUnity.Input.Mappings {

    [System.Serializable]
    public abstract class UnityActionMapping {
    
        /// <summary>
        /// The name is used to look up this mapping once it has been successfully mapped
        /// onto an action
        /// </summary>
        [SerializeField]
        protected string _action;

        public string Action {
            get { return _action; }
        }

        protected MappingType _type;

        public MappingType Type {
            get { return _type; }
        }

        public bool IsMovementType {
            get { return _type == MappingType.Movement; }
        }

        public bool IsButtonType {
            get { return _type == MappingType.Button; }
        }

        public abstract UnityInputAction BuildAction(PlayerInput input);
    }
}
    
    
