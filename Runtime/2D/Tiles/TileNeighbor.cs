using UnityEngine;

namespace BBUnity.Tiles {

    /// <summary>The eight cells around a tile, clockwise from the one above it.</summary>
    public enum TileNeighbor {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    /// <summary>What a rule needs of one neighbouring cell.</summary>
    public enum NeighborRequirement {
        /// <summary>Doesn't matter.</summary>
        Any,

        /// <summary>The neighbour is the same cell type as this tile.</summary>
        Same,

        /// <summary>The neighbour is empty or a different cell type.</summary>
        Different
    }

    public static class TileNeighbors {

        public const int Count = 8;

        private static readonly Vector2Int[] Offsets = {
            new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1),
            new Vector2Int(0, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 0), new Vector2Int(-1, 1)
        };

        public static Vector2Int Offset(TileNeighbor neighbor) {
            return Offsets[(int)neighbor];
        }

        public static int Bit(TileNeighbor neighbor) {
            return 1 << (int)neighbor;
        }
    }
}
