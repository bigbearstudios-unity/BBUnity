using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Placement {

    /// <summary>
    /// Finds the cells where a placeable fits inside a room. A candidate is the bottom-left cell of
    /// the placeable's clearance footprint; the footprint must be empty (non-blocking), inside the
    /// room, and arranged against the surface the placeable needs.
    /// </summary>
    public static class PlacementCandidateFinder {

        public static List<Vector2Int> Find(GridLayer layer, GridRoom room, PlaceableDefinition placeable, IReadOnlyList<RectInt> occupied) {
            var candidates = new List<Vector2Int>();
            Vector2Int size = placeable.Clearance;
            RectInt bounds = room.Bounds;

            for (int y = bounds.yMin; y + size.y <= bounds.yMax; y++) {
                for (int x = bounds.xMin; x + size.x <= bounds.xMax; x++) {
                    var footprint = new RectInt(x, y, size.x, size.y);

                    if (!IsFree(layer, footprint)) { continue; }
                    if (!MatchesSurface(layer, footprint, placeable.Surface)) { continue; }
                    if (Overlaps(footprint, placeable.MinSpacing, occupied)) { continue; }

                    candidates.Add(new Vector2Int(x, y));
                }
            }

            return candidates;
        }

        private static bool IsFree(GridLayer layer, RectInt footprint) {
            for (int y = footprint.yMin; y < footprint.yMax; y++) {
                for (int x = footprint.xMin; x < footprint.xMax; x++) {
                    if (layer.BlocksMovement(x, y)) { return false; }
                }
            }

            return true;
        }

        private static bool MatchesSurface(GridLayer layer, RectInt footprint, PlacementSurface surface) {
            switch (surface) {
                case PlacementSurface.Floor:
                    for (int x = footprint.xMin; x < footprint.xMax; x++) {
                        if (!layer.SupportsStanding(x, footprint.yMin - 1)) { return false; }
                    }
                    return true;

                case PlacementSurface.Ceiling:
                    for (int x = footprint.xMin; x < footprint.xMax; x++) {
                        if (!layer.BlocksMovement(x, footprint.yMax)) { return false; }
                    }
                    return true;

                case PlacementSurface.Wall:
                    return ColumnBlocked(layer, footprint.xMin - 1, footprint) || ColumnBlocked(layer, footprint.xMax, footprint);

                default:
                    return true;
            }
        }

        private static bool ColumnBlocked(GridLayer layer, int x, RectInt footprint) {
            for (int y = footprint.yMin; y < footprint.yMax; y++) {
                if (!layer.BlocksMovement(x, y)) { return false; }
            }

            return true;
        }

        public static bool Overlaps(RectInt footprint, int spacing, IReadOnlyList<RectInt> occupied) {
            var padded = new RectInt(footprint.x - spacing, footprint.y - spacing, footprint.width + spacing * 2, footprint.height + spacing * 2);

            foreach (RectInt other in occupied) {
                if (padded.Overlaps(other)) { return true; }
            }

            return false;
        }
    }
}
