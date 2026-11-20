using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections.Generic;

using BBUnity.Gameplay.Entities.Controllers.Input.Mappings;
using BBUnity.Gameplay.Entities.Controllers.Input.Actions;

using BBUnity.Gameplay.Entities.Controllers.Input.Internal;
namespace BBUnity.Gameplay.Entities.Controllers.Input {

    [AddComponentMenu("BBUnity/Gameplay/Entities/Controllers/Input/Unity Player Input Controller")]
    [RequireComponent(typeof(PlayerInput))]
    public class UnityPlayerInputController : InputController {
        [SerializeField, Tooltip("The reference to the Unity Player Input Component")]
        private PlayerInput _playerInput = null;

        [SerializeReference, Tooltip("The individual mappings to our PlayerInput")]
        private List<UnityActionMapping> _actionMappings;

        /// <summary>
        /// List of the actions, these are mapped to the key used to get the action
        /// </summary>
        private Dictionary<string, UnityInputAction> _actions;

        public void _Editor_AddMovementMapping() {
            _actionMappings.Add(new UnityMovementActionMapping());
        }

        public void _Editor_AddButtonMapping() {
            _actionMappings.Add(new UnityButtonActionMapping());
        }

        public void _Editor_AddAxisMapping() {
            _actionMappings.Add(new UnityAxisActionMapping());
        }

        private void Awake() {
            if(_playerInput ==  null) { throw new InvalidPlayerInputObjectException(); }

            _actions = BuildActionFromMappings(_actionMappings);            
        }

        private void Update() {
            float deltaTime = Time.deltaTime;

            foreach(UnityInputAction action in _actions.Values) {
                action.Update(deltaTime);
            }
        }

        /// <summary>
        /// Creates a brand new instance of Dictionary<string, UnityInputAction> which will be filled
        /// with the mappings 
        /// </summary>
        private Dictionary<string, UnityInputAction> BuildActionFromMappings(List<UnityActionMapping> mappings) {
            Dictionary<string, UnityInputAction> actions = new Dictionary<string, UnityInputAction>();

            foreach(UnityActionMapping mapping in mappings) {
                actions.Add(
                    mapping.Action,
                    mapping.BuildAction(_playerInput)
                );
            }

            return actions;
        }

        public UnityInputAxisAction Movement {
            get { return (UnityInputAxisAction)GetAction("Movement"); }
        }

        public UnityInputAxisAction Axis(string key) {
            return (UnityInputAxisAction)GetAction(key);
        }

        public UnityInputButtonAction Button(string key) {
            return (UnityInputButtonAction)GetAction(key);
        }

        private UnityInputAction GetAction(string key) {
            if (!_actions.TryGetValue(key, out UnityInputAction action)) {
                throw new MissingActionMappingException(key, gameObject.name);
            }

            return action;
        }

        public bool HasMovementMapping {
            get {
                foreach(UnityActionMapping mapping in _actionMappings) {
                    if(mapping.IsMovementType) { return true; }
                }

                return false;
            }
        }

        private void Reset() {
            if(_actionMappings == null) { _actionMappings = new List<UnityActionMapping>(); }

            if(!HasMovementMapping) {
                _Editor_AddMovementMapping();
            }

            if(_playerInput == null) {
                _playerInput = GetComponent<PlayerInput>();
            }
        }
    }
}