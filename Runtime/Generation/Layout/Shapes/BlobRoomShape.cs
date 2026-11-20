using System;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Pipeline;

namespace BBUnity.Generation.Layout.Shapes {

    /// <summary>A rounded, ragged-edged cave chamber: an ellipse filling the bounds with a roughened rim.</summary>
    [Serializable]
    public class BlobRoomShape : RoomShape {

        [SerializeField, Range(0f, 0.8f), Tooltip("How ragged the edge is. 0 is a clean ellipse.")]
        private float _roughness = 0.35f;

        public override void Carve(GridLayer layer, RectInt bounds, GenerationRandom random) {
            float radiusX = Mathf.Max(1f, bounds.width / 2f);
            float radiusY = Mathf.Max(1f, bounds.height / 2f);
            Vector2 center = new Vector2(bounds.xMin + radiusX, bounds.yMin + radiusY);

            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                for (int x = bounds.xMin; x < bounds.xMax; x++) {
                    float dx = (x + 0.5f - center.x) / radiusX;
                    float dy = (y + 0.5f - center.y) / radiusY;
                    float distance = dx * dx + dy * dy;

                    // Everything inside (1 - roughness) is always open, so the chamber stays in one
                    // piece; only the rim band is randomly eaten away.
                    float limit = 1f - _roughness * random.Value();
                    if (distance <= limit) { layer.Set(x, y, null); }
                }
            }

            layer.Set(bounds.xMin + bounds.width / 2, bounds.yMin + bounds.height / 2, null);
        }
    }
}
