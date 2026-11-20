using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Grid;

namespace BBUnity.Generation.Layout {

    /// <summary>Questions about a grid's open space, shared by layout steps and validation.</summary>
    public static class GridAnalysis {

        private static readonly Vector2Int[] Directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        /// <summary>Every open (non-blocking) cell connected to `start` by up/down/left/right steps.</summary>
        public static HashSet<Vector2Int> Reachable(GridLayer layer, Vector2Int start) {
            var found = new HashSet<Vector2Int>();
            if (layer.BlocksMovement(start.x, start.y)) { return found; }

            var queue = new Queue<Vector2Int>();
            found.Add(start);
            queue.Enqueue(start);

            while (queue.Count > 0) {
                Vector2Int cell = queue.Dequeue();

                foreach (Vector2Int direction in Directions) {
                    Vector2Int next = cell + direction;
                    if (layer.BlocksMovement(next.x, next.y) || !found.Add(next)) { continue; }

                    queue.Enqueue(next);
                }
            }

            return found;
        }

        /// <summary>The separate open regions of the layer, largest first.</summary>
        public static List<List<Vector2Int>> OpenRegions(GridLayer layer) {
            var seen = new HashSet<Vector2Int>();
            var regions = new List<List<Vector2Int>>();

            for (int y = 0; y < layer.Height; y++) {
                for (int x = 0; x < layer.Width; x++) {
                    var cell = new Vector2Int(x, y);
                    if (layer.BlocksMovement(x, y) || seen.Contains(cell)) { continue; }

                    var region = new List<Vector2Int>(Reachable(layer, cell));
                    foreach (Vector2Int member in region) { seen.Add(member); }
                    regions.Add(region);
                }
            }

            regions.Sort((a, b) => b.Count.CompareTo(a.Count));
            return regions;
        }

        public static int CountOpen(GridLayer layer) {
            int count = 0;
            for (int y = 0; y < layer.Height; y++) {
                for (int x = 0; x < layer.Width; x++) {
                    if (!layer.BlocksMovement(x, y)) { count++; }
                }
            }

            return count;
        }
    }
}
