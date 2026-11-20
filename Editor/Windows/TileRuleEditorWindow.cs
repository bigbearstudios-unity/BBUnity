using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

using BBUnity.Editor.Tiles;
using BBUnity.Editor.Toolkit;
using BBUnity.Tiles;

namespace BBUnity.Editor.Windows {

    /// <summary>
    /// Builds and tests tile rule sets: every rule set in the project in a list, and for the
    /// selected one a wizard that generates its rules from a standard tile layout, a visual 3 x 3
    /// editor for each rule, and a live preview on a test shape.
    /// </summary>
    public class TileRuleEditorWindow : EditorWindow {

        [SerializeField] private string _selected;
        [SerializeField] private EdgeCornerTileSet _wizard = new EdgeCornerTileSet();
        [SerializeField] private TileBase _stripLeft;
        [SerializeField] private TileBase _stripMiddle;
        [SerializeField] private TileBase _stripRight;
        [SerializeField] private TileBase _stripSingle;

        private DefinitionBrowser<TileRuleSetDefinition> _browser;
        private SerializedObject _windowObject;

        [MenuItem("Window/BBUnity/Tiles/Tile Rule Editor")]
        private static void Open() {
            GetWindow<TileRuleEditorWindow>("Tile Rule Editor");
        }

        public void CreateGUI() {
            _windowObject = new SerializedObject(this);

            _browser = new DefinitionBrowser<TileRuleSetDefinition>(
                "Tile Rule Set",
                ruleSet => ruleSet.name,
                null,
                (ruleSet, serializedObject) => new TileRuleSetEditorView(ruleSet, serializedObject, _windowObject, () => _browser.Reload())
            );

            _browser.SelectByGuid(_selected);
            _browser.Changed += () => _selected = _browser.SelectedGuid;

            rootVisualElement.Add(_browser);
        }
    }
}
