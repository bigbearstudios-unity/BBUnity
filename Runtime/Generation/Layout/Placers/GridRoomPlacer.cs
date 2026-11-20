using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Pipeline;

namespace BBUnity.Generation.Layout.Placers {

    /// <summary>
    /// Divides the level into a grid of columns and rows and puts at most one room in randomly
    /// chosen grid cells, nudged around inside them. Gives the orderly, castle-like arrangement
    /// that scattering doesn't; the room count is capped at the number of grid cells.
    /// </summary>
    [Serializable]
    public class GridRoomPlacer : RoomPlacer {

        [SerializeField, Min(1)]
        private int _columns = 4;

        [SerializeField, Min(1)]
        private int _rows = 3;

        public override List<RectInt> Place(RoomPlacementRequest request, GenerationRandom random) {
            var rooms = new List<RectInt>();

            int usableWidth = request.GridWidth - request.Margin * 2;
            int usableHeight = request.GridHeight - request.Margin * 2;
            int cellWidth = usableWidth / _columns;
            int cellHeight = usableHeight / _rows;
            if (cellWidth < 3 || cellHeight < 3) { return rooms; }

            var cells = new List<Vector2Int>();
            for (int row = 0; row < _rows; row++) {
                for (int column = 0; column < _columns; column++) { cells.Add(new Vector2Int(column, row)); }
            }

            random.Shuffle(cells);

            // Leave a one-cell wall on every side of a grid cell so neighbouring rooms never touch.
            for (int i = 0; i < Mathf.Min(request.RoomCount, cells.Count); i++) {
                Vector2Int size = request.RandomSize(random);
                size.x = Mathf.Clamp(size.x, 1, cellWidth - 2);
                size.y = Mathf.Clamp(size.y, 1, cellHeight - 2);

                int originX = request.Margin + cells[i].x * cellWidth + 1;
                int originY = request.Margin + cells[i].y * cellHeight + 1;

                rooms.Add(new RectInt(
                    originX + random.Range(0, cellWidth - 2 - size.x),
                    originY + random.Range(0, cellHeight - 2 - size.y),
                    size.x,
                    size.y
                ));
            }

            return rooms;
        }
    }
}
