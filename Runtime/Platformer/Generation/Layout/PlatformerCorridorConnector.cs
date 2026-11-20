using System;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Layout;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Rooms;

namespace BBUnity.Platformer.Generation.Layout {

    /// <summary>
    /// Joins two rooms with passages at floor level, so a player walks out of one room and into
    /// the next. A passage runs from each room's floor to a shared meeting column, and a shaft in
    /// that column joins the two floor heights; where the heights differ by more than a jump,
    /// the platform step adds the stepping stones up the shaft.
    /// </summary>
    [Serializable]
    public class PlatformerCorridorConnector : RoomConnector {

        [SerializeField, Min(2), Tooltip("Minimum height of a passage, in cells. Should be at least the player's body height plus one.")]
        private int _minHeight = 3;

        public override void Carve(GridLayer layer, GridRoom from, GridRoom to, int width, GenerationRandom random) {
            int height = Mathf.Max(_minHeight, width);
            int meeting = (from.Center.x + to.Center.x) / 2;
            int fromFloor = from.Bounds.yMin;
            int toFloor = to.Bounds.yMin;

            CarvePassage(layer, from.Bounds, meeting, fromFloor, height);
            CarvePassage(layer, to.Bounds, meeting, toFloor, height);

            int shaftBottom = Mathf.Min(fromFloor, toFloor);
            int shaftTop = Mathf.Max(fromFloor, toFloor) + height;
            int shaftWidth = Mathf.Max(1, width);
            Carving.Rect(layer, new RectInt(meeting - (shaftWidth - 1) / 2, shaftBottom, shaftWidth, shaftTop - shaftBottom));
        }

        // From the room's edge nearest the meeting column to that column, at the room's floor.
        private static void CarvePassage(GridLayer layer, RectInt room, int meeting, int floor, int height) {
            if (meeting >= room.xMin && meeting < room.xMax) { return; }

            int edge = meeting < room.xMin ? room.xMin : room.xMax - 1;
            int left = Mathf.Min(edge, meeting);
            Carving.Rect(layer, new RectInt(left, floor, Mathf.Abs(edge - meeting) + 1, height));
        }
    }
}
