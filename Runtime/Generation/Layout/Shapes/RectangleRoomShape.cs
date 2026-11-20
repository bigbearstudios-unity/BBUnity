using System;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Pipeline;

namespace BBUnity.Generation.Layout.Shapes {

    /// <summary>A plain rectangular room.</summary>
    [Serializable]
    public class RectangleRoomShape : RoomShape {

        public override void Carve(GridLayer layer, RectInt bounds, GenerationRandom random) {
            Carving.Rect(layer, bounds);
        }
    }
}
