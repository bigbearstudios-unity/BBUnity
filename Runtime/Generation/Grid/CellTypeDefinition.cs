using UnityEngine;

namespace BBUnity.Generation.Grid {

    /// <summary>
    /// What one cell of a GenerationGrid can be — Solid, One-Way Platform, Hazard and so on. An
    /// asset rather than an enum so a project can add its own. Generation only cares about the
    /// two movement flags (they drive room carving and placement); how a cell type looks in the
    /// level is decided later by a tile palette, and how it looks in the schematic preview by
    /// PreviewColor. An empty cell is simply null, not a type.
    /// </summary>
    [CreateAssetMenu(fileName = "New Cell Type", menuName = "BBUnity/Generation/Grid/Cell Type Definition")]
    public class CellTypeDefinition : ScriptableObject {

        [SerializeField, Tooltip("Shown in editors. The asset name is used if left blank.")]
        private string _displayName;

        [SerializeField, Tooltip("How this cell type is drawn in the schematic preview.")]
        private Color _previewColor = Color.gray;

        [SerializeField, Tooltip("Characters cannot walk or fall through this cell (walls, ground).")]
        private bool _blocksMovement = true;

        [SerializeField, Tooltip("Something can stand on top of this cell (ground, one-way platforms).")]
        private bool _supportsStanding = true;

        public string DisplayName {
            get { return string.IsNullOrEmpty(_displayName) ? name : _displayName; }
        }

        public Color PreviewColor {
            get { return _previewColor; }
        }

        public bool BlocksMovement {
            get { return _blocksMovement; }
        }

        public bool SupportsStanding {
            get { return _supportsStanding; }
        }
    }
}
