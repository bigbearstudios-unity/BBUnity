using System;

using UnityEngine;

namespace BBUnity.Platformer.Generation.Movement {

    /// <summary>
    /// What the player can do, measured in grid cells and seconds: how fast it jumps and runs and
    /// how big its body is. Level generation plans around it — how high a platform can be, how
    /// wide a gap — so a generated level is always one this player can finish.
    /// </summary>
    [Serializable]
    public struct PlatformerMovementProfile {

        [SerializeField, Min(0.01f), Tooltip("Upward speed at the start of a jump, in cells per second.")]
        private float _jumpSpeed;

        [SerializeField, Min(0.01f), Tooltip("Downward acceleration while airborne, in cells per second squared.")]
        private float _gravity;

        [SerializeField, Min(0.01f), Tooltip("Top running speed, in cells per second.")]
        private float _runSpeed;

        [SerializeField, Min(0.01f), Tooltip("Top falling speed, in cells per second.")]
        private float _maxFallSpeed;

        [SerializeField, Min(1), Tooltip("Width of the player's body, in cells.")]
        private int _bodyWidth;

        [SerializeField, Min(1), Tooltip("Height of the player's body, in cells.")]
        private int _bodyHeight;

        public PlatformerMovementProfile(float jumpSpeed, float gravity, float runSpeed, float maxFallSpeed, int bodyWidth, int bodyHeight) {
            _jumpSpeed = jumpSpeed;
            _gravity = gravity;
            _runSpeed = runSpeed;
            _maxFallSpeed = maxFallSpeed;
            _bodyWidth = bodyWidth;
            _bodyHeight = bodyHeight;
        }

        /// <summary>Roughly the default PlatformingPlayerMovementController at one unit per cell.</summary>
        public static PlatformerMovementProfile Default {
            get { return new PlatformerMovementProfile(25f, 110f, 9f, 40f, 1, 2); }
        }

        public float JumpSpeed {
            get { return _jumpSpeed; }
        }

        public float Gravity {
            get { return _gravity; }
        }

        public float RunSpeed {
            get { return _runSpeed; }
        }

        public float MaxFallSpeed {
            get { return _maxFallSpeed; }
        }

        public int BodyWidth {
            get { return Mathf.Max(1, _bodyWidth); }
        }

        public int BodyHeight {
            get { return Mathf.Max(1, _bodyHeight); }
        }

        /// <summary>How high a full jump rises, in cells.</summary>
        public float MaxJumpHeight {
            get { return _jumpSpeed * _jumpSpeed / (2f * _gravity); }
        }

        /// <summary>How far a full jump travels across level ground at top speed, in cells.</summary>
        public float MaxJumpDistance {
            get { return _runSpeed * 2f * _jumpSpeed / _gravity; }
        }
    }
}
