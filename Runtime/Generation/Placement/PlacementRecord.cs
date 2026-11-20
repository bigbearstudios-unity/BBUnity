using UnityEngine;

namespace BBUnity.Generation.Placement {

    /// <summary>One placed thing: what, where (the bottom-left cell of its footprint) and in which room.</summary>
    public class PlacementRecord {

        public PlaceableDefinition Placeable { get; }
        public Vector2Int Cell { get; }
        public int RoomId { get; }

        public PlacementRecord(PlaceableDefinition placeable, Vector2Int cell, int roomId) {
            Placeable = placeable;
            Cell = cell;
            RoomId = roomId;
        }

        public RectInt Footprint {
            get { return new RectInt(Cell, Placeable.Clearance); }
        }
    }
}
