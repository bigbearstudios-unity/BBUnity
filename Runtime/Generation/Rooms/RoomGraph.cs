using System.Collections.Generic;

using UnityEngine;

namespace BBUnity.Generation.Rooms {

    /// <summary>The rooms of a level and the connections between them.</summary>
    public class RoomGraph {

        private readonly List<GridRoom> _rooms = new List<GridRoom>();
        private readonly List<RoomConnection> _connections = new List<RoomConnection>();

        public IReadOnlyList<GridRoom> Rooms {
            get { return _rooms; }
        }

        public IReadOnlyList<RoomConnection> Connections {
            get { return _connections; }
        }

        public GridRoom AddRoom(RectInt bounds, RoomTypeDefinition type = null) {
            var room = new GridRoom(_rooms.Count, bounds, type);
            _rooms.Add(room);
            return room;
        }

        public RoomConnection Connect(GridRoom a, GridRoom b, ConnectionKind kind, Vector2Int pointA, Vector2Int pointB) {
            var connection = new RoomConnection(a.Id, b.Id, kind, pointA, pointB);
            _connections.Add(connection);
            return connection;
        }

        public GridRoom GetRoom(int id) {
            return id >= 0 && id < _rooms.Count ? _rooms[id] : null;
        }

        public GridRoom FindRoomAt(Vector2Int cell) {
            foreach (GridRoom room in _rooms) {
                if (room.Contains(cell)) { return room; }
            }

            return null;
        }

        public IEnumerable<GridRoom> RoomsOfType(RoomTypeDefinition type) {
            foreach (GridRoom room in _rooms) {
                if (room.Type == type) { yield return room; }
            }
        }

        public IEnumerable<GridRoom> Neighbours(GridRoom room) {
            foreach (RoomConnection connection in _connections) {
                if (connection.Touches(room.Id)) { yield return _rooms[connection.Other(room.Id)]; }
            }
        }

        /// <summary>
        /// The shortest chain of rooms from one room to another (both included), or an empty list
        /// if they are not connected.
        /// </summary>
        public List<GridRoom> FindPath(GridRoom from, GridRoom to) {
            var path = new List<GridRoom>();
            if (from == null || to == null) { return path; }

            var cameFrom = new Dictionary<int, int> { { from.Id, -1 } };
            var queue = new Queue<GridRoom>();
            queue.Enqueue(from);

            while (queue.Count > 0) {
                GridRoom current = queue.Dequeue();
                if (current.Id == to.Id) { break; }

                foreach (GridRoom next in Neighbours(current)) {
                    if (cameFrom.ContainsKey(next.Id)) { continue; }

                    cameFrom[next.Id] = current.Id;
                    queue.Enqueue(next);
                }
            }

            if (!cameFrom.ContainsKey(to.Id)) { return path; }

            for (int id = to.Id; id != -1; id = cameFrom[id]) { path.Add(_rooms[id]); }
            path.Reverse();
            return path;
        }

        public bool AreConnected(GridRoom a, GridRoom b) {
            return FindPath(a, b).Count > 0;
        }
    }
}
