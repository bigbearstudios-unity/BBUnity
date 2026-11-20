using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Platformer.Movement;

namespace BBUnity.Generation.Platformer.Reachability {

    /// <summary>
    /// Works out every standable cell a player can reach from some starting cells by walking and
    /// jumping (using JumpSimulator). A standable cell is one the player's body fits in with
    /// something to stand on. This is deliberately a little conservative — the player can steer
    /// in the air and jump at any height, the simulation tries a fixed set of jumps — so a level
    /// it passes is one the player can finish.
    /// </summary>
    public sealed class PlatformerReachability {

        private static readonly float[] JumpScales = { 0.6f, 1f };
        private static readonly float[] HorizontalScales = { -1f, -0.5f, 0f, 0.5f, 1f };

        private readonly GridLayer _layer;
        private readonly JumpSimulator _simulator;

        public PlatformerReachability(GridLayer layer, PlatformerMovementProfile profile) {
            _layer = layer;
            _simulator = new JumpSimulator(layer, profile);
        }

        public JumpSimulator Simulator {
            get { return _simulator; }
        }

        public bool IsStandable(Vector2Int cell) {
            return _simulator.IsStandable(cell);
        }

        /// <summary>The standable cell closest to the middle of `area`, if there is one in it.</summary>
        public Vector2Int? FindStandableNear(RectInt area) {
            Vector2 middle = area.center;
            Vector2Int? best = null;
            float bestDistance = float.MaxValue;

            for (int y = area.yMin; y < area.yMax; y++) {
                for (int x = area.xMin; x < area.xMax; x++) {
                    var cell = new Vector2Int(x, y);
                    if (!IsStandable(cell)) { continue; }

                    float distance = (new Vector2(x + 0.5f, y) - middle).sqrMagnitude;
                    if (distance < bestDistance) { bestDistance = distance; best = cell; }
                }
            }

            return best;
        }

        /// <summary>
        /// Every standable cell reachable from the starting cells. If `apexes` is given, the top of
        /// every jump tried is added to it (where a stepping-stone platform would be useful).
        /// </summary>
        public HashSet<Vector2Int> ReachableFrom(IEnumerable<Vector2Int> starts, List<Vector2> apexes = null) {
            var reached = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();

            foreach (Vector2Int start in starts) {
                if (IsStandable(start) && reached.Add(start)) { queue.Enqueue(start); }
            }

            while (queue.Count > 0) {
                Vector2Int cell = queue.Dequeue();

                foreach (int direction in new[] { -1, 1 }) {
                    var next = new Vector2Int(cell.x + direction, cell.y);
                    if (IsStandable(next) && reached.Add(next)) { queue.Enqueue(next); }

                    Try(cell, 0f, direction, reached, queue, apexes);
                }

                foreach (float jump in JumpScales) {
                    foreach (float horizontal in HorizontalScales) { Try(cell, jump, horizontal, reached, queue, apexes); }
                }
            }

            return reached;
        }

        private void Try(Vector2Int cell, float jump, float horizontal, HashSet<Vector2Int> reached, Queue<Vector2Int> queue, List<Vector2> apexes) {
            bool landed = _simulator.Simulate(cell, jump, horizontal, out Vector2Int landing, out Vector2 apex);

            if (apexes != null && jump > 0f) { apexes.Add(apex); }
            if (landed && landing != cell && reached.Add(landing)) { queue.Enqueue(landing); }
        }

        public static bool AnyIn(HashSet<Vector2Int> reached, RectInt area) {
            foreach (Vector2Int cell in reached) {
                if (area.Contains(cell)) { return true; }
            }

            return false;
        }
    }
}
