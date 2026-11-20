using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

using BBUnity.Editor.Tiles;
using BBUnity.Editor.Toolkit;
using BBUnity.Tiles;

namespace BBUnity.Editor.Windows {

    /// <summary>
    /// Manages tile palettes: which tile rule set draws each cell type, and how each tilemap layer
    /// of a built level is set up (sorting, physics layer, collision). Give a palette to the
    /// Level Generator's Build In Scene to see a generated level drawn with real tiles.
    /// </summary>
    public class TilePalettesWindow : EditorWindow {

        [SerializeField] private string _selected;

        private DefinitionBrowser<TilePaletteDefinition> _browser;

        [MenuItem("Window/BBUnity/Tiles/Tile Palettes")]
        private static void Open() {
            GetWindow<TilePalettesWindow>("Tile Palettes");
        }

        public void CreateGUI() {
            _browser = new DefinitionBrowser<TilePaletteDefinition>(
                "Tile Palette",
                palette => palette.name,
                null,
                (palette, serializedObject) => new TilePaletteEditorView(serializedObject)
            );

            _browser.SelectByGuid(_selected);
            _browser.Changed += () => _selected = _browser.SelectedGuid;

            rootVisualElement.Add(_browser);
        }
    }
}
