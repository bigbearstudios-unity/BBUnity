using UnityEngine;
using System;
using System.Collections.Generic;
using BBUnity.StateMachines;
using BBUnity.EditorAttributes;
using BBUnity.Gameplay.Entities.Controllers;
using BBUnity.Gameplay.Entities.Controllers.States;

namespace BBUnity.Gameplay.Entities.Controllers.Internal {

    internal class InvalidAnimatorException : Exception {
        public InvalidAnimatorException() : base("An Animator component is required for the State Controller to function") { }
    }
}
