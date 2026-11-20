using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BBUnity.Gameplay.Entities.Controllers;
using BBUnity.Gameplay.Entities.Controllers.Platforming.Internal;

namespace BBUnity.Gameplay.Entities.Controllers.Platforming {

    public sealed class MovementState {

        /*
         * The CollisionState for each direction
         */
        private CollisionState _up = new();
        private CollisionState _down = new();
        private CollisionState _right = new();
        private CollisionState _left = new();

        /*
         * The internal velocity, this will be updated every frame
         */
        Vector2 _velocity = Vector2.zero;

        /// <summary>
        /// Internal method to set the current collision states for each direction
        /// </summary>
        /// <param name="up"></param>
        /// <param name="down"></param>
        /// <param name="right"></param>
        /// <param name="left"></param>
        internal void SetCollisionState(bool up = false, bool down = false, bool right = false, bool left = false) {
            _up.SetState(up);
            _down.SetState(down);
            _right.SetState(right);
            _left.SetState(left);
        }

        /// <summary>
        /// Internal method to set the velocity on the state. This will kept it
        /// in-sync with the velocity on the controller.
        /// </summary>
        /// <param name="velocity"></param>
        internal void SetVelocity(Vector2 velocity) {
            _velocity = velocity;
        }

        public bool IsGrounded {
            get { return _down.CurrentFrame; }
        }

        public bool IsAirborne {
            get { return !IsGrounded; }
        }

        public bool HasLanded {
            get { return WasAirborne && IsGrounded; }
        }

        public bool HasBecomeAirborne {
            get { return WasGrounded && IsAirborne; }
        }

        public bool WasGrounded {
            get { return _down.PreviousFrame; }
        }

        public bool WasAirborne {
            get { return !WasGrounded; }
        }

        public bool IsHittingHead {
            get { return _up.CurrentFrame; }
        }

        public Vector3 Velocity {
            get { return _velocity; }
        }

        public bool IsMovingHorizontally {
            get { return _velocity.x > float.Epsilon || _velocity.x < -float.Epsilon; }
        }

        public bool IsMovingVertically {
            get { return _velocity.y > float.Epsilon || _velocity.y < -float.Epsilon; }
        }

        public bool IsStandingStill {
            get { return !IsMovingHorizontally; }
        }

        public bool IsRaising {
            get { return _velocity.y > float.Epsilon; }
        }

        public bool IsFalling {
            get { return _velocity.y < -float.Epsilon; }
        }
    }
}
