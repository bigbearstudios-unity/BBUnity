using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using BBUnity.Generation.Chunks;
using BBUnity.Generation.Grid;
using BBUnity.Generation.Placement;
using BBUnity.World.Elements;

namespace BBUnity.Editor.Generation {

    internal enum ChunkTool {
        Cells,
        Markers,
        Sockets
    }

    /// <summary>
    /// The drawing surface of the Chunk Editor: a chunk's cells as coloured squares that can be
    /// painted (left button) or erased (right button), plus its hand-placed markers and edge
    /// sockets. Every stroke is one undo step.
    /// </summary>
    internal class ChunkCanvas : VisualElement {

        private const float CellPixels = 24f;

        private static readonly Color SocketColor = new Color(0.3f, 0.9f, 1f);

        private readonly ChunkDefinition _chunk;
        private readonly System.Action _edited;

        public ChunkTool Tool { get; set; } = ChunkTool.Cells;
        public string LayerName { get; set; } = GridLayerNames.Main;
        public CellTypeDefinition Brush { get; set; }
        public PlaceableDefinition MarkerPlaceable { get; set; }
        public string SocketTag { get; set; } = string.Empty;

        public ChunkCanvas(ChunkDefinition chunk, System.Action edited) {
            _chunk = chunk;
            _edited = edited;

            style.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
            style.marginTop = 4;
            generateVisualContent += Draw;

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<AttachToPanelEvent>(_ => Undo.undoRedoPerformed += MarkDirtyRepaint);
            RegisterCallback<DetachFromPanelEvent>(_ => Undo.undoRedoPerformed -= MarkDirtyRepaint);

            UpdateSize();
        }

        public void UpdateSize() {
            style.width = _chunk.Width * CellPixels;
            style.height = _chunk.Height * CellPixels;
            MarkDirtyRepaint();
        }

        // ---- input ----

        private void OnPointerDown(PointerDownEvent evt) {
            Undo.RecordObject(_chunk, "Edit Chunk");
            this.CapturePointer(evt.pointerId);
            Apply(evt.localPosition, evt.button == 1, isClick: true);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt) {
            if (!this.HasPointerCapture(evt.pointerId) || Tool != ChunkTool.Cells) { return; }

            Apply(evt.localPosition, (evt.pressedButtons & 2) != 0, isClick: false);
        }

        private void OnPointerUp(PointerUpEvent evt) {
            if (!this.HasPointerCapture(evt.pointerId)) { return; }

            this.ReleasePointer(evt.pointerId);
            EditorUtility.SetDirty(_chunk);
            AssetDatabase.SaveAssetIfDirty(_chunk);
            _edited?.Invoke();
        }

        private Vector2Int ToCell(Vector2 position) {
            return new Vector2Int(Mathf.FloorToInt(position.x / CellPixels), _chunk.Height - 1 - Mathf.FloorToInt(position.y / CellPixels));
        }

        private void Apply(Vector2 position, bool erase, bool isClick) {
            Vector2Int cell = ToCell(position);
            if (cell.x < 0 || cell.y < 0 || cell.x >= _chunk.Width || cell.y >= _chunk.Height) { return; }

            switch (Tool) {
                case ChunkTool.Cells:
                    _chunk.SetCell(LayerName, cell.x, cell.y, erase ? null : Brush);
                    break;

                case ChunkTool.Markers:
                    if (!isClick) { return; }
                    if (!_chunk.RemoveMarkerAt(cell) && MarkerPlaceable != null) { _chunk.AddMarker(MarkerPlaceable, cell); }
                    break;

                case ChunkTool.Sockets:
                    if (!isClick) { return; }
                    ToggleSocketAt(cell);
                    break;
            }

            MarkDirtyRepaint();
        }

        // A socket belongs to an edge: left and right edges win over top and bottom in the corners.
        private void ToggleSocketAt(Vector2Int cell) {
            if (cell.x == 0) { _chunk.ToggleSocket(ChunkEdge.Left, cell.y, SocketTag); }
            else if (cell.x == _chunk.Width - 1) { _chunk.ToggleSocket(ChunkEdge.Right, cell.y, SocketTag); }
            else if (cell.y == _chunk.Height - 1) { _chunk.ToggleSocket(ChunkEdge.Top, cell.x, SocketTag); }
            else if (cell.y == 0) { _chunk.ToggleSocket(ChunkEdge.Bottom, cell.x, SocketTag); }
        }

        // ---- drawing ----

        private Rect CellRect(int x, int y) {
            return new Rect(x * CellPixels, (_chunk.Height - 1 - y) * CellPixels, CellPixels, CellPixels);
        }

        private void Draw(MeshGenerationContext context) {
            Painter2D painter = context.painter2D;

            // Other layers first and faded, the one being edited on top at full strength.
            foreach (ChunkLayer layer in _chunk.Layers) {
                if (layer.Name != LayerName) { DrawLayer(painter, layer.Name, 0.35f); }
            }

            DrawLayer(painter, LayerName, 1f);
            DrawGrid(painter);
            DrawSockets(painter);
            DrawMarkers(painter);
        }

        private void DrawLayer(Painter2D painter, string layerName, float alpha) {
            for (int y = 0; y < _chunk.Height; y++) {
                for (int x = 0; x < _chunk.Width; x++) {
                    CellTypeDefinition type = _chunk.GetCell(layerName, x, y);
                    if (type == null) { continue; }

                    Color color = type.PreviewColor;
                    Fill(painter, CellRect(x, y), new Color(color.r, color.g, color.b, alpha));
                }
            }
        }

        private void DrawGrid(Painter2D painter) {
            painter.strokeColor = new Color(1f, 1f, 1f, 0.08f);
            painter.lineWidth = 1f;

            for (int x = 0; x <= _chunk.Width; x++) { Line(painter, new Vector2(x * CellPixels, 0), new Vector2(x * CellPixels, _chunk.Height * CellPixels)); }
            for (int y = 0; y <= _chunk.Height; y++) { Line(painter, new Vector2(0, y * CellPixels), new Vector2(_chunk.Width * CellPixels, y * CellPixels)); }
        }

        private void DrawSockets(Painter2D painter) {
            foreach (ChunkSocket socket in _chunk.Sockets) {
                Rect cell;
                switch (socket.Edge) {
                    case ChunkEdge.Left: cell = CellRect(0, socket.Offset); break;
                    case ChunkEdge.Right: cell = CellRect(_chunk.Width - 1, socket.Offset); break;
                    case ChunkEdge.Top: cell = CellRect(socket.Offset, _chunk.Height - 1); break;
                    default: cell = CellRect(socket.Offset, 0); break;
                }

                Stroke(painter, new Rect(cell.x + 2, cell.y + 2, cell.width - 4, cell.height - 4), SocketColor, 3f);
            }
        }

        private void DrawMarkers(Painter2D painter) {
            foreach (ChunkMarker marker in _chunk.Markers) {
                Color color = Color.magenta;
                var element = marker.Placeable as LevelElementDefinition;
                if (element != null && element.Category != null) { color = element.Category.PreviewColor; }

                Rect cell = CellRect(marker.Position.x, marker.Position.y);
                var inner = new Rect(cell.x + 5, cell.y + 5, cell.width - 10, cell.height - 10);
                Fill(painter, inner, color);
                Stroke(painter, inner, Color.black, 1f);
            }
        }

        private static void Fill(Painter2D painter, Rect rect, Color color) {
            painter.fillColor = color;
            Path(painter, rect);
            painter.Fill();
        }

        private static void Stroke(Painter2D painter, Rect rect, Color color, float width) {
            painter.strokeColor = color;
            painter.lineWidth = width;
            Path(painter, rect);
            painter.Stroke();
        }

        private static void Path(Painter2D painter, Rect rect) {
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMax));
            painter.LineTo(new Vector2(rect.xMin, rect.yMax));
            painter.ClosePath();
        }

        private static void Line(Painter2D painter, Vector2 from, Vector2 to) {
            painter.BeginPath();
            painter.MoveTo(from);
            painter.LineTo(to);
            painter.Stroke();
        }
    }
}
