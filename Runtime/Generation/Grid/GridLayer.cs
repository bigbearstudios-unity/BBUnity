using UnityEngine;

namespace BBUnity.Generation.Grid {

    /// <summary>
    /// One layer of a GenerationGrid: a width x height array of cell types, null meaning empty.
    /// Reads outside the grid are treated as solid wall (BlocksMovement is true), so carving and
    /// placement code never has to special-case the edges.
    /// </summary>
    public class GridLayer {

        private readonly CellTypeDefinition[] _cells;

        public string Name { get; }
        public int Width { get; }
        public int Height { get; }

        public GridLayer(string name, int width, int height) {
            Name = name;
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            _cells = new CellTypeDefinition[Width * Height];
        }

        public bool InBounds(int x, int y) {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public bool InBounds(Vector2Int cell) {
            return InBounds(cell.x, cell.y);
        }

        /// <summary>The cell type at x, y; null for an empty cell or one outside the grid.</summary>
        public CellTypeDefinition Get(int x, int y) {
            return InBounds(x, y) ? _cells[y * Width + x] : null;
        }

        public CellTypeDefinition Get(Vector2Int cell) {
            return Get(cell.x, cell.y);
        }

        /// <summary>Sets a cell (null to empty it). Writes outside the grid are ignored.</summary>
        public void Set(int x, int y, CellTypeDefinition type) {
            if (InBounds(x, y)) { _cells[y * Width + x] = type; }
        }

        public void Set(Vector2Int cell, CellTypeDefinition type) {
            Set(cell.x, cell.y, type);
        }

        public void Fill(RectInt area, CellTypeDefinition type) {
            for (int y = area.yMin; y < area.yMax; y++) {
                for (int x = area.xMin; x < area.xMax; x++) { Set(x, y, type); }
            }
        }

        public void Clear() {
            System.Array.Clear(_cells, 0, _cells.Length);
        }

        public bool IsEmpty(int x, int y) {
            return InBounds(x, y) && _cells[y * Width + x] == null;
        }

        /// <summary>True for a blocking cell, and for anywhere outside the grid.</summary>
        public bool BlocksMovement(int x, int y) {
            if (!InBounds(x, y)) { return true; }

            CellTypeDefinition type = _cells[y * Width + x];
            return type != null && type.BlocksMovement;
        }

        /// <summary>True for a cell something can stand on. Outside the grid counts as ground.</summary>
        public bool SupportsStanding(int x, int y) {
            if (!InBounds(x, y)) { return true; }

            CellTypeDefinition type = _cells[y * Width + x];
            return type != null && type.SupportsStanding;
        }

        public int Count(CellTypeDefinition type) {
            int count = 0;
            foreach (CellTypeDefinition cell in _cells) {
                if (cell == type) { count++; }
            }

            return count;
        }
    }
}
