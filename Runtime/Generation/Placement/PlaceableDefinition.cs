using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Placement {

    /// <summary>
    /// Anything generation can put into a level — an enemy, item, trap, spawn point. This is the
    /// generic, game-agnostic part: where it may sit and how much room it needs. What it actually
    /// becomes (a prefab, an entity) is added by a game-side subclass such as
    /// LevelElementDefinition.
    /// </summary>
    public abstract class PlaceableDefinition : ScriptableObject {

        [SerializeField, Tooltip("Shown in editors. The asset name is used if left blank.")]
        private string _displayName;

        [SerializeField]
        private PlacementSurface _surface = PlacementSurface.Floor;

        [SerializeField, Tooltip("The free space needed, in cells (width, height).")]
        private Vector2Int _clearance = Vector2Int.one;

        [SerializeField, Min(0), Tooltip("Cells of empty space to keep between this and other placed things.")]
        private int _minSpacing = 0;

        [SerializeField, Min(0), Tooltip("What this costs against a room's or level's budget (for example difficulty).")]
        private int _cost = 1;

        [SerializeField, Min(0f), Tooltip("How likely this is chosen over alternatives of the same kind.")]
        private float _weight = 1f;

        [SerializeField, Tooltip("Room types this may be placed in. Empty means any room.")]
        private List<RoomTypeDefinition> _allowedRoomTypes = new List<RoomTypeDefinition>();

        public string DisplayName {
            get { return string.IsNullOrEmpty(_displayName) ? name : _displayName; }
        }

        public PlacementSurface Surface {
            get { return _surface; }
        }

        public Vector2Int Clearance {
            get { return new Vector2Int(Mathf.Max(1, _clearance.x), Mathf.Max(1, _clearance.y)); }
        }

        public int MinSpacing {
            get { return _minSpacing; }
        }

        public int Cost {
            get { return _cost; }
        }

        public float Weight {
            get { return _weight; }
        }

        public IReadOnlyList<RoomTypeDefinition> AllowedRoomTypes {
            get { return _allowedRoomTypes; }
        }

        public bool AllowsRoom(GridRoom room) {
            return _allowedRoomTypes.Count == 0 || (room.Type != null && _allowedRoomTypes.Contains(room.Type));
        }
    }
}
