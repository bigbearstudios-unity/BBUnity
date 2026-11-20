using UnityEngine;
using System;
using System.Collections.Generic;
using BBUnity.StateMachines;
using BBUnity.EditorAttributes;

namespace BBUnity.Entities.Internal {

    internal class InvalidAnimatorException : Exception {
        public InvalidAnimatorException() : base("An Animator component is required for the State Controller to function") { }
    }
}
