using System;
using System.Collections.Generic;
using UnityEngine;

namespace BBUnity.Animation2D.Exceptions {

    public class SpriteAnimatorNoFramesException : Exception {
        public SpriteAnimatorNoFramesException() : base("BBUnity SpriteAnimator: No frames have been set. Assign at least one sprite to the Frames array before calling Play().") { }
    }
}
