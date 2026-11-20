using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Parameters;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Layout {

    /// <summary>Joins all of a result's rooms: plans the passages, carves them with a connector and records them in the room graph.</summary>
    public static class RoomLinking {

        public static void ConnectAll(GenerationContext context, GridLayer layer, RoomConnector connector, IntParameter corridorWidth, IntParameter extraConnections) {
            int width = context.Resolve("layout.corridorWidth", corridorWidth);
            int extra = context.Resolve("layout.extraConnections", extraConnections);
            GenerationRandom random = context.Random.Fork("layout.connect");

            foreach ((GridRoom a, GridRoom b) in ConnectionPlanner.Plan(context.Result.Rooms.Rooms, extra, random)) {
                connector.Carve(layer, a, b, width, random);

                Vector2Int pointA = Clamp(b.Center, a.Bounds);
                Vector2Int pointB = Clamp(a.Center, b.Bounds);
                bool horizontal = Mathf.Abs(b.Center.x - a.Center.x) >= Mathf.Abs(b.Center.y - a.Center.y);
                context.Result.Rooms.Connect(a, b, horizontal ? ConnectionKind.Horizontal : ConnectionKind.Vertical, pointA, pointB);
            }
        }

        private static Vector2Int Clamp(Vector2Int point, RectInt bounds) {
            return new Vector2Int(Mathf.Clamp(point.x, bounds.xMin, bounds.xMax - 1), Mathf.Clamp(point.y, bounds.yMin, bounds.yMax - 1));
        }
    }
}
