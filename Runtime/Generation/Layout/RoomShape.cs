using System;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Pipeline;

namespace BBUnity.Generation.Layout {

    /// <summary>
    /// Decides what a room looks like inside its rectangle. The grid is solid when a shape is
    /// asked to carve, so it only has to clear the open space. A shape must stay within `bounds`
    /// and must leave the room's centre cell open and everything it opens connected to it.
    /// </summary>
    [Serializable]
    public abstract class RoomShape {

        public abstract void Carve(GridLayer layer, RectInt bounds, GenerationRandom random);
    }
}
