using System.Collections.Generic;

using UnityEngine;

namespace BBUnity.Generation.Rooms {

    /// <summary>A rectangular region of the grid with a type, as laid out by generation.</summary>
    public class GridRoom {

        public int Id { get; }
        public RectInt Bounds { get; set; }
        public RoomTypeDefinition Type { get; set; }
        public List<string> Tags { get; } = new List<string>();

        public GridRoom(int id, RectInt bounds, RoomTypeDefinition type = null) {
            Id = id;
            Bounds = bounds;
            Type = type;
        }

        public Vector2Int Center {
            get { return new Vector2Int(Bounds.xMin + Bounds.width / 2, Bounds.yMin + Bounds.height / 2); }
        }

        public bool Contains(Vector2Int cell) {
            return Bounds.Contains(cell);
        }
    }
}
