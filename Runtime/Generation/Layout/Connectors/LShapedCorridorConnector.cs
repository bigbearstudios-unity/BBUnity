using System;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Layout.Connectors {

    /// <summary>A corridor with a single bend between the two room centres, bending either way at random.</summary>
    [Serializable]
    public class LShapedCorridorConnector : RoomConnector {

        public override void Carve(GridLayer layer, GridRoom from, GridRoom to, int width, GenerationRandom random) {
            Carving.LShape(layer, from.Center, to.Center, width, random.Chance(0.5f));
        }
    }
}
