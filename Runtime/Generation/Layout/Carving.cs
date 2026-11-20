using UnityEngine;

using BBUnity.Generation.Grid;

namespace BBUnity.Generation.Layout {

    /// <summary>Helpers for carving open space out of a solid grid. Open space is a null cell.</summary>
    public static class Carving {

        public static void Rect(GridLayer layer, RectInt area) {
            layer.Fill(area, null);
        }

        /// <summary>Carves a square of `width` cells centred on a point (a corridor's cross-section).</summary>
        public static void Brush(GridLayer layer, Vector2Int center, int width) {
            width = Mathf.Max(1, width);
            int half = (width - 1) / 2;
            layer.Fill(new RectInt(center.x - half, center.y - half, width, width), null);
        }

        /// <summary>Carves a straight horizontal or vertical corridor between two points that share a row or a column.</summary>
        public static void Line(GridLayer layer, Vector2Int from, Vector2Int to, int width) {
            width = Mathf.Max(1, width);
            int half = (width - 1) / 2;

            if (from.y == to.y) {
                int minX = Mathf.Min(from.x, to.x);
                layer.Fill(new RectInt(minX, from.y - half, Mathf.Abs(to.x - from.x) + 1, width), null);
            } else if (from.x == to.x) {
                int minY = Mathf.Min(from.y, to.y);
                layer.Fill(new RectInt(from.x - half, minY, width, Mathf.Abs(to.y - from.y) + 1), null);
            } else {
                throw new System.ArgumentException("Carving.Line needs two points on the same row or column; use LShape for others.");
            }
        }

        /// <summary>Carves a corridor with one bend between any two points.</summary>
        public static void LShape(GridLayer layer, Vector2Int from, Vector2Int to, int width, bool horizontalFirst) {
            Vector2Int corner = horizontalFirst ? new Vector2Int(to.x, from.y) : new Vector2Int(from.x, to.y);

            Line(layer, from, corner, width);
            Line(layer, corner, to, width);
        }
    }
}
