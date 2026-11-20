using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using BBUnity.Gameplay.Entities.Controllers.Input.Mappings;
using BBUnity.Gameplay.Entities.Controllers.Input.Actions;

namespace BBUnity.Gameplay.Entities.Controllers.Input.Internal {

    internal class MissingActionMappingException : System.Exception {
        public MissingActionMappingException(string key, string gameObjectName)
            : base($"No action mapping named '{key}' was found on the UnityPlayerInputController attached to '{gameObjectName}'. Check the Action Mappings list in the inspector.") { }
    }
}
