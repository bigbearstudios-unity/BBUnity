using UnityEngine.Tilemaps;

using BBUnity.Generation.Grid;

namespace BBUnity.Tiles {

    /// <summary>
    /// Works out which tile a grid cell gets. Pure logic with no scene objects, so it serves the
    /// editor's live previews, the tilemap painter and runtime generation alike.
    /// </summary>
    public static class TileRuleResolver {

        /// <summary>A bit per TileNeighbor, set where that neighbour is the same cell type as `type`.</summary>
        public static int ComputeSameMask(GridLayer layer, int x, int y, CellTypeDefinition type, bool outOfBoundsCountsAsSame) {
            int mask = 0;

            for (int i = 0; i < TileNeighbors.Count; i++) {
                var neighbor = (TileNeighbor)i;
                int nx = x + TileNeighbors.Offset(neighbor).x;
                int ny = y + TileNeighbors.Offset(neighbor).y;

                bool same = layer.InBounds(nx, ny) ? layer.Get(nx, ny) == type : outOfBoundsCountsAsSame;
                if (same) { mask |= TileNeighbors.Bit(neighbor); }
            }

            return mask;
        }

        /// <summary>The tile for the cell at x, y (its own cell type is read from the layer).</summary>
        public static TileBase Resolve(GridLayer layer, int x, int y, TileRuleSetDefinition ruleSet, int seed) {
            CellTypeDefinition type = layer.Get(x, y);
            if (type == null || ruleSet == null) { return null; }

            int mask = ComputeSameMask(layer, x, y, type, ruleSet.OutOfBoundsCountsAsSame);
            return ruleSet.Resolve(mask, Roll(x, y, seed));
        }

        /// <summary>A repeatable 0..1 value for a cell, so random variants don't change between regenerations.</summary>
        public static float Roll(int x, int y, int seed) {
            unchecked {
                int hash = x * 73856093 ^ y * 19349663 ^ seed * 83492791;
                return (hash & 0x7FFFFFFF) % 10000 / 10000f;
            }
        }
    }
}
