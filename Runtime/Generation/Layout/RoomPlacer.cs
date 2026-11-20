using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Pipeline;

namespace BBUnity.Generation.Layout {

    /// <summary>
    /// Decides where a level's rooms go. Subclass it to add a placement idea (a spiral, a tower, a
    /// ring...): return the room rectangles, which must be inside the grid and must not overlap.
    /// Returning fewer rooms than requested is allowed; the layout step reports the shortfall.
    /// </summary>
    [Serializable]
    public abstract class RoomPlacer {

        public abstract List<RectInt> Place(RoomPlacementRequest request, GenerationRandom random);
    }
}
