using UnityEngine;

namespace BBUnity.Generation.Rooms {

    public enum ConnectionKind {
        Horizontal,
        Vertical,
        Door
    }

    /// <summary>A link between two rooms, with the doorway cell on each side.</summary>
    public class RoomConnection {

        public int RoomA { get; }
        public int RoomB { get; }
        public ConnectionKind Kind { get; }
        public Vector2Int PointA { get; }
        public Vector2Int PointB { get; }

        public RoomConnection(int roomA, int roomB, ConnectionKind kind, Vector2Int pointA, Vector2Int pointB) {
            RoomA = roomA;
            RoomB = roomB;
            Kind = kind;
            PointA = pointA;
            PointB = pointB;
        }

        public bool Touches(int roomId) {
            return RoomA == roomId || RoomB == roomId;
        }

        public int Other(int roomId) {
            return roomId == RoomA ? RoomB : RoomA;
        }
    }
}
