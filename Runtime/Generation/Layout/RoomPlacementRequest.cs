using UnityEngine;

using BBUnity.Generation.Pipeline;

namespace BBUnity.Generation.Layout {

    /// <summary>What a RoomPlacer is asked for: how many rooms, how big, inside how large a grid.</summary>
    public class RoomPlacementRequest {

        public int GridWidth;
        public int GridHeight;
        public int RoomCount;
        public int MinRoomWidth;
        public int MaxRoomWidth;
        public int MinRoomHeight;
        public int MaxRoomHeight;

        /// <summary>Cells to keep clear of the grid edge so every room has a wall around it.</summary>
        public int Margin;

        public Vector2Int RandomSize(GenerationRandom random) {
            return new Vector2Int(random.Range(MinRoomWidth, MaxRoomWidth), random.Range(MinRoomHeight, MaxRoomHeight));
        }
    }
}
