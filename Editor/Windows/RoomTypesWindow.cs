using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

using BBUnity.Editor.Generation;
using BBUnity.Editor.Toolkit;
using BBUnity.Generation.Rooms;

namespace BBUnity.Editor.Windows {

    /// <summary>Manages the kinds of room a generated level can contain (Start, Exit, Combat, Treasure...).</summary>
    public class RoomTypesWindow : EditorWindow {

        [SerializeField] private string _selected;

        private DefinitionBrowser<RoomTypeDefinition> _roomTypes;

        [MenuItem("Window/BBUnity/Generation/Room Types")]
        private static void Open() {
            GetWindow<RoomTypesWindow>("Room Types");
        }

        public void CreateGUI() {
            _roomTypes = new DefinitionBrowser<RoomTypeDefinition>(
                "Room Type",
                roomType => roomType.DisplayName,
                null,
                DefinitionDetails.CreateRoomType,
                roomType => roomType.PreviewColor
            );

            _roomTypes.SelectByGuid(_selected);
            _roomTypes.Changed += () => _selected = _roomTypes.SelectedGuid;

            rootVisualElement.Add(_roomTypes);
        }
    }
}
