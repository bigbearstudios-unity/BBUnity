using UnityEngine;

namespace BBUnity.Movement.Internal {

    /// <summary>
    /// The movement controller's view of player input. Written from Update (by whatever state is
    /// active) and read from FixedUpdate — two loops that don't run in lockstep — so anything that
    /// is a one-frame event rather than a held state is latched here until FixedUpdate has
    /// actually seen it, otherwise a press that lands on a frame with no physics step is simply
    /// overwritten by the next Update and lost.
    /// </summary>
    internal class InputState {

        private float _horizontalMovement;
        private bool _jumpRequested;
        private bool _jumpHeld;

        /// <summary>
        /// A jump was pressed and hasn't been consumed by a physics step yet. Stays true across
        /// any number of Updates until ConsumeJumpRequest() is called.
        /// </summary>
        public bool JumpRequested {
            get { return _jumpRequested; }
        }

        /// <summary>
        /// Whether the jump button is currently held down. Releasing it while rising ends the
        /// jump early.
        /// </summary>
        public bool JumpHeld {
            get { return _jumpHeld; }
        }

        public float HorizontalMovement {
            get { return _horizontalMovement; }
        }

        public bool HasHorizontalMovement {
            get { return _horizontalMovement > float.Epsilon || _horizontalMovement < -float.Epsilon; }
        }

        public void SetHorizontalMovement(float horizontalMovement, bool snapMovement) {
            _horizontalMovement = horizontalMovement;

            if(snapMovement && HasHorizontalMovement) {
                _horizontalMovement = Mathf.Sign(_horizontalMovement);
            }
        }

        /// <param name="jumpPressed">True on the frame the button went down. Latched — a false
        /// value never clears an unconsumed request.</param>
        /// <param name="jumpHeld">True for as long as the button is held.</param>
        public void SetJump(bool jumpPressed, bool jumpHeld) {
            if(jumpPressed) { _jumpRequested = true; }

            _jumpHeld = jumpHeld;
        }

        /// <summary>
        /// Called by the physics step once it has acted on (or deliberately ignored) the request.
        /// </summary>
        public void ConsumeJumpRequest() {
            _jumpRequested = false;
        }
    }
}
