using UnityEngine;

namespace BBUnity.Gameplay.Levels.Elements {

    /// <summary>A room of a generated level: its area in the world and what kind of room it is.</summary>
    [AddComponentMenu("BBUnity/Gameplay/Levels/Elements/Level Room")]
    public class LevelRoom : MonoBehaviour {

        [SerializeField]
        private int _roomId;

        [SerializeField]
        private string _roomType;

        [SerializeField]
        private Rect _bounds;

        public int RoomId {
            get { return _roomId; }
        }

        public string RoomType {
            get { return _roomType; }
        }

        /// <summary>The room's area in world units.</summary>
        public Rect Bounds {
            get { return _bounds; }
        }

        public void Initialize(int roomId, string roomType, Rect bounds) {
            _roomId = roomId;
            _roomType = roomType;
            _bounds = bounds;
        }
    }
}
