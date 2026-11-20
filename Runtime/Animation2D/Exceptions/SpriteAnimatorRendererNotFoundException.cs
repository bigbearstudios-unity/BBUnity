using System;
using System.Collections.Generic;
using UnityEngine;

namespace BBUnity.Animation2D.Exceptions {

    public class SpriteAnimatorRendererNotFoundException : Exception {
        public SpriteAnimatorRendererNotFoundException() : base("BBUnity SpriteAnimator: A SpriteRenderer could not be found. Ensure the GameObject has a SpriteRenderer component attached.") { }
    }
}
