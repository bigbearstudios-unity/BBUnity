using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BBUnity.Gameplay.Entities.Controllers;
using BBUnity.Gameplay.Entities.Controllers.Platforming.Internal;

namespace BBUnity.Gameplay.Entities.Controllers.Platforming.Internal {

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
