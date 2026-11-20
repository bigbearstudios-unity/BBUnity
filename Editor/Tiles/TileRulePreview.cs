using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Generation.Grid;
using BBUnity.Tiles;

namespace BBUnity.Editor.Tiles {

    /// <summary>
    /// A live test shape drawn with a rule set's tiles: click cells to add or remove solid, or pick
    /// a preset shape. Hover a tile to see which rule picked it — the quickest way to find a rule
    /// that is wrong or missing.
    /// </summary>
    internal class TileRulePreview : VisualElement {

        private const int Width = 16;
        private const int Height = 10;
        private const int CellSize = 30;

        private static readonly (string name, string[] rows)[] Shapes = {
            ("Ground and ledge", new[] {
                "................", "................", "................", "....######......", "................",
                "............##..", "............##..", "################", "################", "################" }),
            ("Block", new[] {
                "................", "................", "...########.....", "...########.....", "...########.....",
                "...########.....", "...########.....", "................", "................", "................" }),
            ("Notches", new[] {
                "................", "..##########....", "..##########....", "..####..####....", "..####..####....",
                "..##########....", "..###.....##....", "..###.....##....", "..##########....", "................" }),
            ("Platforms", new[] {
                "................", "................", "..#####.........", "................", ".........#......",
                "...........####.", "................", "..##............", "................", "................" })
        };

        private readonly TileRuleSetDefinition _ruleSet;
        private readonly CellTypeDefinition _cellType;
        private readonly GridLayer _layer = new GenerationGrid(Width, Height).Main;
        private readonly VisualElement _grid = new VisualElement();

        public TileRulePreview(TileRuleSetDefinition ruleSet) {
            _ruleSet = ruleSet;
            _cellType = ScriptableObject.CreateInstance<CellTypeDefinition>();
            _cellType.hideFlags = HideFlags.HideAndDontSave;

            Add(InspectorElements.CreateHeading("Preview"));

            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            foreach ((string name, string[] rows) shape in Shapes) {
                string[] rows = shape.rows;
                toolbar.Add(new Button(() => { Load(rows); Refresh(); }) { text = shape.name });
            }

            toolbar.Add(new Button(() => { _layer.Clear(); Refresh(); }) { text = "Clear" });
            Add(toolbar);

            var hint = new Label("Click a cell to add or remove ground. Hover a tile to see which rule it used.");
            hint.style.opacity = 0.7f;
            hint.style.marginTop = 2;
            Add(hint);

            _grid.style.marginTop = 4;
            Add(_grid);

            Load(Shapes[0].rows);
            Refresh();

            RegisterCallback<DetachFromPanelEvent>(_ => Object.DestroyImmediate(_cellType));
        }

        private void Load(string[] rows) {
            _layer.Clear();

            for (int row = 0; row < rows.Length && row < Height; row++) {
                int y = Height - 1 - row;
                for (int x = 0; x < rows[row].Length && x < Width; x++) {
                    if (rows[row][x] == '#') { _layer.Set(x, y, _cellType); }
                }
            }
        }

        public void Refresh() {
            _grid.Clear();

            for (int y = Height - 1; y >= 0; y--) {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;

                for (int x = 0; x < Width; x++) { row.Add(CreateCell(x, y)); }

                _grid.Add(row);
            }
        }

        private VisualElement CreateCell(int x, int y) {
            var cell = new VisualElement();
            cell.style.width = cell.style.height = CellSize;
            cell.style.backgroundColor = new Color(0.14f, 0.14f, 0.16f);
            cell.style.borderBottomWidth = cell.style.borderRightWidth = 1;
            cell.style.borderBottomColor = cell.style.borderRightColor = new Color(0f, 0f, 0f, 0.35f);

            int cellX = x, cellY = y;
            cell.RegisterCallback<PointerDownEvent>(_ => {
                _layer.Set(cellX, cellY, _layer.Get(cellX, cellY) == null ? _cellType : null);
                Refresh();
            });

            if (_layer.Get(x, y) == null) { return cell; }

            int mask = TileRuleResolver.ComputeSameMask(_layer, x, y, _cellType, _ruleSet.OutOfBoundsCountsAsSame);
            int ruleIndex = _ruleSet.FindRuleIndex(mask);
            TileBase tile = _ruleSet.Resolve(mask, TileRuleResolver.Roll(x, y, 1));

            cell.tooltip = ruleIndex >= 0 ? $"Rule {ruleIndex + 1}: {_ruleSet.Rules[ruleIndex].Name}" : tile != null ? "Fallback tile" : "No rule matches and there is no fallback tile";

            Sprite sprite = (tile as Tile)?.sprite;
            if (sprite != null) {
                var image = new Image { sprite = sprite, scaleMode = ScaleMode.StretchToFill };
                image.style.flexGrow = 1;
                image.pickingMode = PickingMode.Ignore;
                cell.Add(image);
            } else {
                // No sprite to show (no tile, or a kind of tile that has none).
                cell.style.backgroundColor = tile != null ? new Color(0.35f, 0.4f, 0.5f) : new Color(0.6f, 0.2f, 0.2f);
                var mark = new Label(tile != null ? "▦" : "!");
                mark.style.unityTextAlign = TextAnchor.MiddleCenter;
                mark.style.flexGrow = 1;
                mark.pickingMode = PickingMode.Ignore;
                cell.Add(mark);
            }

            return cell;
        }
    }
}
