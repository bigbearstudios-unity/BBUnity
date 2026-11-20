using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace BBUnity.Input.Internal {

    internal class InvalidPlayerInputObjectException : System.Exception {
        public InvalidPlayerInputObjectException() : base("A Player Input object is required by the UnityPlayerInputController") { }
    }
}
