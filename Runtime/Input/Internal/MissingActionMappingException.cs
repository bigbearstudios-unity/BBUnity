using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace BBUnity.Input.Internal {

    internal class MissingActionMappingException : System.Exception {
        public MissingActionMappingException(string key, string gameObjectName)
            : base($"No action mapping named '{key}' was found on the UnityPlayerInputController attached to '{gameObjectName}'. Check the Action Mappings list in the inspector.") { }
    }
}
