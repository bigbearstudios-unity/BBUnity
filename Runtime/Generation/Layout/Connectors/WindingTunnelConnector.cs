using System;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Layout.Connectors {

    /// <summary>
    /// A meandering tunnel: walks from one room's centre towards the other's, sometimes stepping
    /// sideways instead of onwards. It always finishes the journey, so rooms are never left unlinked.
    /// </summary>
    [Serializable]
    public class WindingTunnelConnector : RoomConnector {

        [SerializeField, Range(0f, 0.6f), Tooltip("Chance of each step wandering sideways instead of towards the target.")]
        private float _wobble = 0.25f;

        public override void Carve(GridLayer layer, GridRoom from, GridRoom to, int width, GenerationRandom random) {
            Vector2Int position = from.Center;
            Vector2Int target = to.Center;
            int budget = (Mathf.Abs(target.x - position.x) + Mathf.Abs(target.y - position.y)) * 4 + 16;

            Carving.Brush(layer, position, width);

            while (position != target && budget-- > 0) {
                int dx = target.x - position.x;
                int dy = target.y - position.y;
                bool moveHorizontally = dy == 0 || (dx != 0 && Mathf.Abs(dx) >= Mathf.Abs(dy));

                if (random.Chance(_wobble)) { moveHorizontally = !moveHorizontally; }

                if (moveHorizontally) {
                    position.x += dx != 0 && !random.Chance(_wobble) ? (int)Mathf.Sign(dx) : (random.Chance(0.5f) ? 1 : -1);
                } else {
                    position.y += dy != 0 && !random.Chance(_wobble) ? (int)Mathf.Sign(dy) : (random.Chance(0.5f) ? 1 : -1);
                }

                position.x = Mathf.Clamp(position.x, 1, layer.Width - 2);
                position.y = Mathf.Clamp(position.y, 1, layer.Height - 2);
                Carving.Brush(layer, position, width);
            }

            if (position != target) {
                Carving.LShape(layer, position, target, width, true);
            }
        }
    }
}
