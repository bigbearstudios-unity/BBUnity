using UnityEngine;

namespace BBUnity.Generation.Rooms {

    /// <summary>
    /// A kind of room — Start, Exit, Combat, Treasure, Boss, Corridor, Shaft, Secret... An asset so
    /// a project can add its own. A room type carries no rules itself: what may appear in a room
    /// is decided by each placeable's allowed room types, and how rooms are made by the
    /// generation steps.
    /// </summary>
    [CreateAssetMenu(fileName = "New Room Type", menuName = "BBUnity/Generation/Rooms/Room Type Definition")]
    public class RoomTypeDefinition : ScriptableObject {

        [SerializeField, Tooltip("Shown in editors. The asset name is used if left blank.")]
        private string _displayName;

        [SerializeField, Tooltip("How rooms of this type are tinted in the schematic preview.")]
        private Color _previewColor = new Color(1f, 1f, 1f, 0.15f);

        public string DisplayName {
            get { return string.IsNullOrEmpty(_displayName) ? name : _displayName; }
        }

        public Color PreviewColor {
            get { return _previewColor; }
        }
    }
}
