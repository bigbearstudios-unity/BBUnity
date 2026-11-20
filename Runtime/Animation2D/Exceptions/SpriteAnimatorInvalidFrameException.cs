using System;
using System.Collections.Generic;
using UnityEngine;

namespace BBUnity.Animation2D.Exceptions {

    public class SpriteAnimatorInvalidFrameException : Exception {
        public SpriteAnimatorInvalidFrameException() : base("BBUnity SpriteAnimator: The requested frame index is out of range. Check that the index is between 0 and Frames.Length - 1.") { }
    }
}
