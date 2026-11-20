using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Pipeline;

namespace BBUnity.Generation.Layout.Placers {

    /// <summary>Drops rooms at random spots, rejecting any that would touch another (with a gap between them).</summary>
    [Serializable]
    public class ScatterRoomPlacer : RoomPlacer {

        [SerializeField, Min(1), Tooltip("Minimum solid cells between two rooms.")]
        private int _gap = 3;

        [SerializeField, Min(1), Tooltip("How many random spots are tried for each room before giving up on it.")]
        private int _attemptsPerRoom = 40;

        public override List<RectInt> Place(RoomPlacementRequest request, GenerationRandom random) {
            var rooms = new List<RectInt>();

            for (int i = 0; i < request.RoomCount; i++) {
                for (int attempt = 0; attempt < _attemptsPerRoom; attempt++) {
                    Vector2Int size = request.RandomSize(random);

                    int maxX = request.GridWidth - request.Margin - size.x;
                    int maxY = request.GridHeight - request.Margin - size.y;
                    if (maxX < request.Margin || maxY < request.Margin) { continue; }

                    var area = new RectInt(random.Range(request.Margin, maxX), random.Range(request.Margin, maxY), size.x, size.y);
                    if (Collides(area, rooms)) { continue; }

                    rooms.Add(area);
                    break;
                }
            }

            return rooms;
        }

        private bool Collides(RectInt area, List<RectInt> rooms) {
            var padded = new RectInt(area.x - _gap, area.y - _gap, area.width + _gap * 2, area.height + _gap * 2);

            foreach (RectInt other in rooms) {
                if (padded.Overlaps(other)) { return true; }
            }

            return false;
        }
    }
}
