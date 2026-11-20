using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

using BBUnity.Editor.Generation;
using BBUnity.Editor.Toolkit;
using BBUnity.Generation.Chunks;

namespace BBUnity.Editor.Windows {

    /// <summary>
    /// Builds the hand-authored chunks — rooms, shafts, set pieces — that chunk-based layouts draw
    /// from: paint the cells, place specific enemies, items and traps, and mark where other chunks
    /// can attach. Add finished chunks to a chunk library and give the library to a template.
    /// </summary>
    public class ChunkEditorWindow : EditorWindow {

        [SerializeField] private string _selected;

        private DefinitionBrowser<ChunkDefinition> _browser;

        [MenuItem("Window/BBUnity/Generation/Chunk Editor")]
        private static void Open() {
            GetWindow<ChunkEditorWindow>("Chunk Editor");
        }

        public void CreateGUI() {
            _browser = new DefinitionBrowser<ChunkDefinition>(
                "Chunk",
                chunk => chunk.name,
                null,
                (chunk, serializedObject) => new ChunkEditorView(chunk, serializedObject),
                chunk => chunk.RoomType != null ? chunk.RoomType.PreviewColor : (Color?)null
            );

            _browser.SelectByGuid(_selected);
            _browser.Changed += () => _selected = _browser.SelectedGuid;

            rootVisualElement.Add(_browser);
        }
    }
}
