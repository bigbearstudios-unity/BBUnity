using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Generation.Chunks;
using BBUnity.Generation.Grid;
using BBUnity.Generation.Placement;

namespace BBUnity.Editor.Generation {

    /// <summary>
    /// The editor for one chunk: its size, room type, weight and tags, and a canvas to paint cells
    /// (left button paints, right button erases), place hand-picked enemies, items and traps, and
    /// mark the edge sockets other chunks attach to.
    /// </summary>
    internal class ChunkEditorView : VisualElement {

        private const string Erase = "(Erase)";
        private const string NoPlaceable = "(None)";

        private readonly ChunkDefinition _chunk;
        private readonly SerializedObject _serializedObject;
        private readonly ChunkCanvas _canvas;
        private readonly Label _info = new Label();

        public ChunkEditorView(ChunkDefinition chunk, SerializedObject serializedObject) {
            _chunk = chunk;
            _serializedObject = serializedObject;
            _canvas = new ChunkCanvas(chunk, OnEdited);

            Add(new PropertyField(serializedObject.FindProperty("_roomType")));
            Add(new PropertyField(serializedObject.FindProperty("_weight")));
            Add(new PropertyField(serializedObject.FindProperty("_tags")));

            Add(CreateSizeRow());

            Add(InspectorElements.CreateHeading("Draw"));
            Add(CreateToolbar());
            Add(CreateToolOptions());

            _info.style.opacity = 0.7f;
            _info.style.marginTop = 4;
            Add(_info);

            var scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scroll.style.maxHeight = 520;
            scroll.Add(_canvas);
            Add(scroll);

            Add(new Label("Left click paints, right click erases. Cells you leave empty are open space. Author the chunk as a complete room, walls included, with its centre open."));

            OnEdited();
        }

        private VisualElement CreateSizeRow() {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginTop = 4;

            var width = new IntegerField("Width") { value = _chunk.Width };
            var height = new IntegerField("Height") { value = _chunk.Height };
            width.style.flexGrow = height.style.flexGrow = 1;
            row.Add(width);
            row.Add(height);

            row.Add(new Button(() => {
                Undo.RecordObject(_chunk, "Resize Chunk");
                _chunk.Resize(width.value, height.value);
                EditorUtility.SetDirty(_chunk);
                width.SetValueWithoutNotify(_chunk.Width);
                height.SetValueWithoutNotify(_chunk.Height);
                _canvas.UpdateSize();
                OnEdited();
            }) { text = "Apply Size" });

            return row;
        }

        private VisualElement CreateToolbar() {
            var toolbar = new Toolbar();

            var cells = new ToolbarToggle { text = "Cells", value = true };
            var markers = new ToolbarToggle { text = "Markers" };
            var sockets = new ToolbarToggle { text = "Sockets" };
            var toggles = new[] { cells, markers, sockets };

            void Choose(ChunkTool tool) {
                _canvas.Tool = tool;
                for (int i = 0; i < toggles.Length; i++) { toggles[i].SetValueWithoutNotify(i == (int)tool); }
                UpdateOptions();
            }

            cells.RegisterValueChangedCallback(_ => Choose(ChunkTool.Cells));
            markers.RegisterValueChangedCallback(_ => Choose(ChunkTool.Markers));
            sockets.RegisterValueChangedCallback(_ => Choose(ChunkTool.Sockets));

            toolbar.Add(cells);
            toolbar.Add(markers);
            toolbar.Add(sockets);
            return toolbar;
        }

        private VisualElement _cellOptions;
        private VisualElement _markerOptions;
        private VisualElement _socketOptions;

        private VisualElement CreateToolOptions() {
            var options = new VisualElement();

            _cellOptions = new VisualElement();
            var layer = new DropdownField("Layer", new List<string> { GridLayerNames.Main, GridLayerNames.Background, GridLayerNames.Decoration }, 0);
            layer.RegisterValueChangedCallback(change => { _canvas.LayerName = change.newValue; _canvas.MarkDirtyRepaint(); });
            _cellOptions.Add(layer);

            List<CellTypeDefinition> cellTypes = Find<CellTypeDefinition>().OrderBy(type => type.DisplayName).ToList();
            var brush = new DropdownField("Brush", new[] { Erase }.Concat(cellTypes.Select(type => type.DisplayName)).ToList(), cellTypes.Count > 0 ? 1 : 0);
            brush.RegisterValueChangedCallback(change => _canvas.Brush = cellTypes.FirstOrDefault(type => type.DisplayName == change.newValue));
            _canvas.Brush = cellTypes.FirstOrDefault();
            _cellOptions.Add(brush);
            options.Add(_cellOptions);

            _markerOptions = new VisualElement();
            List<PlaceableDefinition> placeables = Find<PlaceableDefinition>().OrderBy(placeable => placeable.DisplayName).ToList();
            var placeable = new DropdownField("Place", new[] { NoPlaceable }.Concat(placeables.Select(p => p.DisplayName)).ToList(), 0);
            placeable.RegisterValueChangedCallback(change => _canvas.MarkerPlaceable = placeables.FirstOrDefault(p => p.DisplayName == change.newValue));
            _markerOptions.Add(placeable);
            _markerOptions.Add(new Label("Click a cell to place the chosen element there; click it again to remove it. These are placed exactly where you put them."));
            options.Add(_markerOptions);

            _socketOptions = new VisualElement();
            var tag = new TextField("Socket Tag");
            tag.RegisterValueChangedCallback(change => _canvas.SocketTag = change.newValue);
            _socketOptions.Add(tag);
            _socketOptions.Add(new Label("Click a cell on the chunk's edge to add or remove a socket. Sockets join other chunks' sockets on the opposite edge with the same tag."));
            options.Add(_socketOptions);

            UpdateOptions();
            return options;
        }

        private void UpdateOptions() {
            InspectorElements.SetVisible(_cellOptions, _canvas.Tool == ChunkTool.Cells);
            InspectorElements.SetVisible(_markerOptions, _canvas.Tool == ChunkTool.Markers);
            InspectorElements.SetVisible(_socketOptions, _canvas.Tool == ChunkTool.Sockets);
        }

        // The canvas edits the asset directly, so the serialized copy is refreshed to match.
        private void OnEdited() {
            _serializedObject.Update();

            int open = 0;
            for (int y = 0; y < _chunk.Height; y++) {
                for (int x = 0; x < _chunk.Width; x++) {
                    if (_chunk.GetCell(GridLayerNames.Main, x, y) == null) { open++; }
                }
            }

            _info.text = $"{_chunk.Width} × {_chunk.Height} cells  |  {open} open  |  {_chunk.Markers.Count} markers  |  {_chunk.Sockets.Count} sockets";
        }

        private static List<T> Find<T>() where T : Object {
            return AssetDatabase.FindAssets("t:" + typeof(T).Name)
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null)
                .ToList();
        }
    }
}
