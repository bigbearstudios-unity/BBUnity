using System;
using System.Collections.Generic;
using UnityEngine;

namespace BBUnity.Animation2D.Exceptions {

    public class SpriteAnimatorImmutableWhilePlayingException : Exception {
        public SpriteAnimatorImmutableWhilePlayingException() : base("BBUnity SpriteAnimator: Cannot modify frames while the animation is playing. Call StopAnimation() first.") { }
    }
}
