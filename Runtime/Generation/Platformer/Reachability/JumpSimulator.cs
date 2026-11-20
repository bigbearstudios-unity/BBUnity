using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Platformer.Movement;

namespace BBUnity.Generation.Platformer.Reachability {

    /// <summary>
    /// Plays a jump (or a walk off a ledge) through the grid using the player's movement profile and
    /// reports where it lands. The body is a box that is blocked by blocking cells and lands on
    /// anything that supports standing — including one-way platforms, which only catch it on the
    /// way down. Generation and validation both use this one simulation, so a level is judged
    /// the same way it was built.
    /// </summary>
    public sealed class JumpSimulator {

        private const float Epsilon = 0.001f;
        private const float WalkLimit = 2f;
        private const float MaxSeconds = 6f;

        private readonly GridLayer _layer;
        private readonly PlatformerMovementProfile _profile;
        private readonly float _step;
        private readonly float _halfWidth;

        public JumpSimulator(GridLayer layer, PlatformerMovementProfile profile) {
            _layer = layer;
            _profile = profile;
            _halfWidth = profile.BodyWidth / 2f;

            float fastest = Mathf.Max(profile.JumpSpeed, Mathf.Max(profile.MaxFallSpeed, profile.RunSpeed));
            _step = Mathf.Min(1f / 60f, 0.4f / fastest);
        }

        public PlatformerMovementProfile Profile {
            get { return _profile; }
        }

        /// <summary>True if the body fits at this cell and something supports it from below.</summary>
        public bool IsStandable(Vector2Int cell) {
            if (!_layer.InBounds(cell.x, cell.y) || cell.y < 1) { return false; }
            if (Overlaps(cell.x + 0.5f, cell.y)) { return false; }

            for (int column = ColumnMin(cell.x + 0.5f); column <= ColumnMax(cell.x + 0.5f); column++) {
                if (_layer.SupportsStanding(column, cell.y - 1)) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Starts at a standable cell with a jump of `jumpScale` x the full jump speed (0 means just
        /// walking off the edge) and horizontal speed of `horizontalScale` x top speed. Returns true
        /// with the standable cell it lands on; `apex` is where it stops rising either way.
        /// </summary>
        public bool Simulate(Vector2Int start, float jumpScale, float horizontalScale, out Vector2Int landing, out Vector2 apex) {
            float startX = start.x + 0.5f;
            float x = startX;
            float y = start.y;
            float vx = horizontalScale * _profile.RunSpeed;
            float vy = jumpScale * _profile.JumpSpeed;
            bool walking = jumpScale <= 0f;
            bool apexFound = vy <= 0f;

            apex = new Vector2(x, y);
            landing = default;

            int maxSteps = Mathf.CeilToInt(MaxSeconds / _step);
            for (int step = 0; step < maxSteps; step++) {
                float nextX = x + vx * _step;
                if (!Overlaps(nextX, y)) {
                    x = nextX;
                } else if (walking) {
                    return false;
                } else {
                    vx = 0f;
                }

                if (walking && Mathf.Abs(x - startX) > WalkLimit) { return false; }

                vy = Mathf.Max(vy - _profile.Gravity * _step, -_profile.MaxFallSpeed);

                if (vy > 0f) {
                    float nextY = y + vy * _step;
                    if (Overlaps(x, nextY)) { vy = 0f; } else { y = nextY; }
                    continue;
                }

                if (!apexFound) {
                    apex = new Vector2(x, y);
                    apexFound = true;
                }

                float fallenY = y + vy * _step;
                if (TryLand(x, y, fallenY, out float landY)) {
                    y = landY;

                    // Still on the ledge it started from: keep walking until it drops off.
                    if (walking && Mathf.Abs(landY - start.y) < Epsilon) {
                        vy = 0f;
                        continue;
                    }

                    landing = new Vector2Int(Mathf.FloorToInt(x), Mathf.RoundToInt(y));
                    return _layer.InBounds(landing.x, landing.y) && IsStandable(landing);
                }

                y = fallenY;
                if (y < -1f) { return false; }
            }

            return false;
        }

        private int ColumnMin(float x) {
            return Mathf.FloorToInt(x - _halfWidth + Epsilon);
        }

        private int ColumnMax(float x) {
            return Mathf.FloorToInt(x + _halfWidth - Epsilon);
        }

        private bool Overlaps(float x, float y) {
            int bottom = Mathf.FloorToInt(y + Epsilon);
            int top = Mathf.FloorToInt(y + _profile.BodyHeight - Epsilon);

            for (int row = bottom; row <= top; row++) {
                for (int column = ColumnMin(x); column <= ColumnMax(x); column++) {
                    if (_layer.BlocksMovement(column, row)) { return true; }
                }
            }

            return false;
        }

        // The highest cell top the body's feet cross while moving from oldY down to newY, if that
        // cell blocks or supports standing.
        private bool TryLand(float x, float oldY, float newY, out float landY) {
            landY = float.MinValue;
            int firstRow = Mathf.FloorToInt(oldY + Epsilon);
            int lastRow = Mathf.FloorToInt(newY);

            for (int row = firstRow; row >= lastRow; row--) {
                float top = row + 1;
                if (top > oldY + Epsilon || top < newY - Epsilon) { continue; }

                for (int column = ColumnMin(x); column <= ColumnMax(x); column++) {
                    if (_layer.BlocksMovement(column, row) || _layer.SupportsStanding(column, row)) {
                        landY = Mathf.Max(landY, top);
                    }
                }
            }

            return landY > float.MinValue;
        }
    }
}
