using UnityEngine;

namespace BBUnity.World.Generation {

    /// <summary>Maps grid cells to world positions: the grid's bottom-left corner and the size of one cell.</summary>
    public readonly struct GridToWorld {

        public Vector2 Origin { get; }
        public float CellSize { get; }

        public GridToWorld(Vector2 origin, float cellSize) {
            Origin = origin;
            CellSize = Mathf.Max(0.0001f, cellSize);
        }

        /// <summary>The world position of a cell's bottom-left corner.</summary>
        public Vector2 CellCorner(Vector2Int cell) {
            return Origin + new Vector2(cell.x, cell.y) * CellSize;
        }

        /// <summary>The world rectangle covered by a rectangle of cells.</summary>
        public Rect ToWorld(RectInt cells) {
            return new Rect(CellCorner(cells.min), new Vector2(cells.width, cells.height) * CellSize);
        }

        /// <summary>The bottom centre of a footprint of cells — where an element's pivot goes.</summary>
        public Vector2 FootprintBottomCenter(RectInt footprint) {
            return Origin + new Vector2(footprint.x + footprint.width / 2f, footprint.y) * CellSize;
        }
    }
}
