using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using BBUnity.Gameplay.Entities.Controllers.Input.Mappings;
using BBUnity.Gameplay.Entities.Controllers.Input.Actions;

namespace BBUnity.Gameplay.Entities.Controllers.Input.Internal {

    internal class InvalidPlayerInputObjectException : System.Exception {
        public InvalidPlayerInputObjectException() : base("A Player Input object is required by the UnityPlayerInputController") { }
    }
}
