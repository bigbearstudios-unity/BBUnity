using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBUnity.Movement.Internal {

    internal sealed class CollisionState {
        bool _currentFrame;
        bool _previousFrame;

        internal void SetState(bool current) {
            _previousFrame = _currentFrame;
            _currentFrame = current;
        }

        public bool CurrentFrame {
            get { return _currentFrame; }
        }

        public bool PreviousFrame {
            get { return _previousFrame; }
        }
    }
}
