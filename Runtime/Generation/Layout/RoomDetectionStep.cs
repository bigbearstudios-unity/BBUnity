using System;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Parameters;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Layout {

    /// <summary>
    /// Turns open space that wasn't laid out as rooms (a cave) into rooms and connections. The
    /// level is divided into blocks; each block with enough open cells becomes a room bounded by
    /// the open cells in it, and two neighbouring rooms are connected wherever open space runs
    /// across the border between their blocks. This is what lets population, cameras and
    /// validation treat a cave the same way as a room layout.
    /// </summary>
    [Serializable]
    public class RoomDetectionStep : GenerationStep {

        [SerializeField]
        private IntParameter _blockWidth = IntParameter.Fixed(24);

        [SerializeField]
        private IntParameter _blockHeight = IntParameter.Fixed(16);

        [SerializeField, Min(1), Tooltip("A block with fewer open cells than this is too small to count as a room.")]
        private int _minOpenCells = 20;

        [SerializeField, Tooltip("The type given to every room this step creates.")]
        private RoomTypeDefinition _roomType;

        public override string DisplayName {
            get { return "Room Detection"; }
        }

        public override void Execute(GenerationContext context) {
            GridLayer layer = context.Result.Grid.Main;
            int blockWidth = Mathf.Max(4, context.Resolve("detect.blockWidth", _blockWidth));
            int blockHeight = Mathf.Max(4, context.Resolve("detect.blockHeight", _blockHeight));
            int columns = Mathf.CeilToInt(layer.Width / (float)blockWidth);
            int rows = Mathf.CeilToInt(layer.Height / (float)blockHeight);

            var rooms = new GridRoom[columns, rows];
            for (int row = 0; row < rows; row++) {
                for (int column = 0; column < columns; column++) {
                    rooms[column, row] = CreateRoom(context, layer, new RectInt(column * blockWidth, row * blockHeight, blockWidth, blockHeight));
                }
            }

            for (int row = 0; row < rows; row++) {
                for (int column = 0; column < columns; column++) {
                    if (rooms[column, row] == null) { continue; }

                    if (column + 1 < columns && rooms[column + 1, row] != null) {
                        LinkAcross(context, layer, rooms[column, row], rooms[column + 1, row], (column + 1) * blockWidth, row * blockHeight, blockHeight, true);
                    }

                    if (row + 1 < rows && rooms[column, row + 1] != null) {
                        LinkAcross(context, layer, rooms[column, row], rooms[column, row + 1], (row + 1) * blockHeight, column * blockWidth, blockWidth, false);
                    }
                }
            }

            if (context.Result.Rooms.Rooms.Count == 0) {
                context.AddIssue(IssueSeverity.Warning, "Room Detection found no open areas large enough to be rooms.");
            }
        }

        private GridRoom CreateRoom(GenerationContext context, GridLayer layer, RectInt block) {
            int count = 0;
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;

            for (int y = block.yMin; y < Mathf.Min(block.yMax, layer.Height); y++) {
                for (int x = block.xMin; x < Mathf.Min(block.xMax, layer.Width); x++) {
                    if (layer.BlocksMovement(x, y)) { continue; }

                    count++;
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            if (count < _minOpenCells) { return null; }

            return context.Result.Rooms.AddRoom(new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1), _roomType);
        }

        // Looks along the border between two blocks for the first place open space continues
        // across it. `border` is the first cell of the second block; along runs the border.
        private static void LinkAcross(GenerationContext context, GridLayer layer, GridRoom first, GridRoom second, int border, int alongStart, int alongLength, bool horizontal) {
            for (int along = alongStart; along < alongStart + alongLength; along++) {
                Vector2Int before = horizontal ? new Vector2Int(border - 1, along) : new Vector2Int(along, border - 1);
                Vector2Int after = horizontal ? new Vector2Int(border, along) : new Vector2Int(along, border);

                if (layer.BlocksMovement(before.x, before.y) || layer.BlocksMovement(after.x, after.y)) { continue; }

                context.Result.Rooms.Connect(first, second, horizontal ? ConnectionKind.Horizontal : ConnectionKind.Vertical, before, after);
                return;
            }
        }
    }
}
