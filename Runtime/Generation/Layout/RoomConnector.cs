using System;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Layout {

    /// <summary>
    /// Decides how two rooms are joined: carve open space between them (from one room's centre to
    /// the other's) that is `width` cells wide. Subclass it for a new kind of passage; the layout
    /// step records the connection in the room graph.
    /// </summary>
    [Serializable]
    public abstract class RoomConnector {

        public abstract void Carve(GridLayer layer, GridRoom from, GridRoom to, int width, GenerationRandom random);
    }
}
