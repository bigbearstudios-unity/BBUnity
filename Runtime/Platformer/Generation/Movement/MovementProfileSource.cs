using System;

using UnityEngine;
using BBUnity.Movement;


namespace BBUnity.Platformer.Generation.Movement {

    public enum MovementProfileMode {
        FromController,
        Manual
    }

    /// <summary>
    /// Where a template gets its movement profile: read from the player's real controller (so the
    /// level always matches how the player actually moves), or typed in by hand (for testing, or
    /// before a player exists). The controller reports world units, so the cell size converts
    /// them to cells.
    /// </summary>
    [Serializable]
    public class MovementProfileSource {

        [SerializeField]
        private MovementProfileMode _mode = MovementProfileMode.Manual;

        [SerializeField, Tooltip("The player prefab's movement controller. Its jump and run numbers and capsule size are read from it.")]
        private PlatformingPlayerMovementController _controller;

        [SerializeField, Min(0.01f), Tooltip("World units per grid cell — the tile size.")]
        private float _cellSize = 1f;

        [SerializeField]
        private PlatformerMovementProfile _manual = PlatformerMovementProfile.Default;

        public MovementProfileSource() { }

        public MovementProfileSource(PlatformerMovementProfile manual) {
            _mode = MovementProfileMode.Manual;
            _manual = manual;
        }

        public float CellSize {
            get { return _cellSize; }
        }

        public PlatformerMovementProfile Resolve() {
            if (_mode == MovementProfileMode.FromController && _controller != null) {
                return FromController(_controller, _cellSize);
            }

            return _manual;
        }

        public static PlatformerMovementProfile FromController(PlatformingPlayerMovementController controller, float cellSize) {
            cellSize = Mathf.Max(0.01f, cellSize);

            int bodyWidth = 1, bodyHeight = 2;
            CapsuleCollider2D capsule = controller.GetComponent<CapsuleCollider2D>();
            if (capsule != null) {
                bodyWidth = Mathf.Max(1, Mathf.CeilToInt(capsule.size.x / cellSize - 0.001f));
                bodyHeight = Mathf.Max(1, Mathf.CeilToInt(capsule.size.y / cellSize - 0.001f));
            }

            return new PlatformerMovementProfile(
                controller.JumpSpeed / cellSize,
                controller.AirborneGravity / cellSize,
                controller.MaximumHorizontalSpeed / cellSize,
                controller.MaximumFallSpeed / cellSize,
                bodyWidth,
                bodyHeight
            );
        }
    }
}
