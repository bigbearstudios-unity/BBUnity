using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Placement;
using BBUnity.Generation.Rooms;
using BBUnity.World.Elements;

namespace BBUnity.Editor.Generation {

    /// <summary>
    /// Draws a GenerationResult as a flat schematic — walls and ground, rooms, connections, what
    /// was placed and any problems — with no tiles or prefabs, so a level can be judged the moment
    /// it is generated. Scroll to zoom, drag to pan.
    /// </summary>
    internal class SchematicView : VisualElement {

        private static readonly Color Background = new Color(0.12f, 0.12f, 0.14f);
        private static readonly Color ReachableTint = new Color(0.3f, 1f, 0.4f, 0.28f);
        private static readonly Color ConnectionColor = new Color(1f, 0.95f, 0.4f, 0.9f);

        private GenerationResult _result;
        private HashSet<Vector2Int> _reachable;
        private float _zoom = 8f;
        private Vector2 _pan;
        private bool _fitPending = true;

        public bool ShowRooms { get; set; } = true;
        public bool ShowConnections { get; set; } = true;
        public bool ShowPlacements { get; set; } = true;
        public bool ShowReachable { get; set; } = true;

        public SchematicView() {
            style.flexGrow = 1;
            style.overflow = Overflow.Hidden;
            style.backgroundColor = Background;
            focusable = true;

            generateVisualContent += Draw;
            RegisterCallback<GeometryChangedEvent>(_ => {
                if (_fitPending) { Fit(); }
            });
            RegisterCallback<WheelEvent>(OnWheel);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        public void SetResult(GenerationResult result, HashSet<Vector2Int> reachable, bool refit) {
            _result = result;
            _reachable = reachable;
            if (refit) { _fitPending = true; Fit(); }
            MarkDirtyRepaint();
        }

        public void Fit() {
            if (_result == null || _result.Grid == null || contentRect.width < 1f || contentRect.height < 1f) { return; }

            _fitPending = false;
            _zoom = Mathf.Max(1f, Mathf.Min((contentRect.width - 20f) / _result.Grid.Width, (contentRect.height - 20f) / _result.Grid.Height));
            _pan = new Vector2((contentRect.width - _result.Grid.Width * _zoom) / 2f, (contentRect.height - _result.Grid.Height * _zoom) / 2f);
            MarkDirtyRepaint();
        }

        private void OnWheel(WheelEvent evt) {
            Vector2 mouse = evt.localMousePosition;
            float height = contentRect.height;
            float cellX = (mouse.x - _pan.x) / _zoom;
            float cellY = (height - mouse.y - _pan.y) / _zoom;

            _zoom = Mathf.Clamp(_zoom * Mathf.Exp(-evt.delta.y * 0.05f), 1f, 80f);
            _pan = new Vector2(mouse.x - cellX * _zoom, height - mouse.y - cellY * _zoom);

            MarkDirtyRepaint();
            evt.StopPropagation();
        }

        private void OnPointerDown(PointerDownEvent evt) {
            this.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt) {
            if (!this.HasPointerCapture(evt.pointerId)) { return; }

            _pan += new Vector2(evt.deltaPosition.x, -evt.deltaPosition.y);
            MarkDirtyRepaint();
        }

        private void OnPointerUp(PointerUpEvent evt) {
            if (this.HasPointerCapture(evt.pointerId)) { this.ReleasePointer(evt.pointerId); }
        }

        // A rectangle in cell coordinates (bottom-left origin, y up) as a screen-space rectangle.
        private Rect ToScreen(float x, float y, float width, float height) {
            return new Rect(_pan.x + x * _zoom, contentRect.height - (_pan.y + (y + height) * _zoom), width * _zoom, height * _zoom);
        }

        private void Draw(MeshGenerationContext context) {
            if (_result == null || _result.Grid == null) { return; }

            Painter2D painter = context.painter2D;
            GenerationGrid grid = _result.Grid;

            FillRect(painter, ToScreen(0, 0, grid.Width, grid.Height), new Color(0.18f, 0.18f, 0.21f));

            foreach (GridLayer layer in grid.Layers) { DrawCells(painter, layer); }

            if (ShowReachable && _reachable != null) {
                foreach (Vector2Int cell in _reachable) { FillRect(painter, ToScreen(cell.x, cell.y, 1, 1), ReachableTint); }
            }

            if (ShowRooms) { DrawRooms(painter); }
            if (ShowConnections) { DrawConnections(painter); }
            if (ShowPlacements) { DrawPlacements(painter); }
            DrawIssues(painter);
        }

        // Neighbouring cells of one type are drawn as a single rectangle, so a big level stays cheap.
        private void DrawCells(Painter2D painter, GridLayer layer) {
            for (int y = 0; y < layer.Height; y++) {
                int x = 0;
                while (x < layer.Width) {
                    CellTypeDefinition type = layer.Get(x, y);
                    if (type == null) { x++; continue; }

                    int start = x;
                    while (x < layer.Width && layer.Get(x, y) == type) { x++; }

                    FillRect(painter, ToScreen(start, y, x - start, 1), type.PreviewColor);
                }
            }
        }

        private void DrawRooms(Painter2D painter) {
            foreach (GridRoom room in _result.Rooms.Rooms) {
                Rect area = ToScreen(room.Bounds.x, room.Bounds.y, room.Bounds.width, room.Bounds.height);
                if (room.Type != null) { FillRect(painter, area, room.Type.PreviewColor); }

                StrokeRect(painter, area, new Color(1f, 1f, 1f, 0.35f), 1f);
            }
        }

        private void DrawConnections(Painter2D painter) {
            painter.strokeColor = ConnectionColor;
            painter.lineWidth = Mathf.Max(1f, _zoom * 0.2f);

            foreach (RoomConnection connection in _result.Rooms.Connections) {
                Rect a = ToScreen(connection.PointA.x, connection.PointA.y, 1, 1);
                Rect b = ToScreen(connection.PointB.x, connection.PointB.y, 1, 1);

                painter.BeginPath();
                painter.MoveTo(a.center);
                painter.LineTo(b.center);
                painter.Stroke();
            }
        }

        private void DrawPlacements(Painter2D painter) {
            foreach (PlacementRecord placement in _result.Placements) {
                RectInt footprint = placement.Footprint;
                Rect area = ToScreen(footprint.x, footprint.y, footprint.width, footprint.height);

                Color color = Color.magenta;
                var element = placement.Placeable as LevelElementDefinition;
                if (element != null && element.Category != null) { color = element.Category.PreviewColor; }

                FillRect(painter, area, color);
                StrokeRect(painter, area, Color.black, 1f);
            }
        }

        private void DrawIssues(Painter2D painter) {
            foreach (GenerationIssue issue in _result.Issues) {
                if (!issue.Cell.HasValue) { continue; }

                Rect area = ToScreen(issue.Cell.Value.x - 1, issue.Cell.Value.y - 1, 3, 3);
                StrokeRect(painter, area, issue.Severity == IssueSeverity.Error ? Color.red : new Color(1f, 0.7f, 0.1f), 2f);
            }
        }

        private static void FillRect(Painter2D painter, Rect rect, Color color) {
            painter.fillColor = color;
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMax));
            painter.LineTo(new Vector2(rect.xMin, rect.yMax));
            painter.ClosePath();
            painter.Fill();
        }

        private static void StrokeRect(Painter2D painter, Rect rect, Color color, float width) {
            painter.strokeColor = color;
            painter.lineWidth = width;
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMax));
            painter.LineTo(new Vector2(rect.xMin, rect.yMax));
            painter.ClosePath();
            painter.Stroke();
        }
    }
}
