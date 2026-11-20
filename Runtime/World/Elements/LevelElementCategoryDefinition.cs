using UnityEngine;

namespace BBUnity.World.Elements {

    /// <summary>
    /// A kind of thing that goes in a level — Player Spawn, Exit, Enemy, Item, Trap, Checkpoint...
    /// An asset rather than an enum so a project can add its own. Used to group and filter
    /// elements in the editors and to colour them in the schematic preview.
    /// </summary>
    [CreateAssetMenu(fileName = "New Level Element Category", menuName = "BBUnity/World/Elements/Level Element Category Definition")]
    public class LevelElementCategoryDefinition : ScriptableObject {

        [SerializeField, Tooltip("Shown in editors. The asset name is used if left blank.")]
        private string _displayName;

        [SerializeField, Tooltip("How elements of this category are drawn in the schematic preview.")]
        private Color _previewColor = Color.white;

        public string DisplayName {
            get { return string.IsNullOrEmpty(_displayName) ? name : _displayName; }
        }

        public Color PreviewColor {
            get { return _previewColor; }
        }
    }
}
