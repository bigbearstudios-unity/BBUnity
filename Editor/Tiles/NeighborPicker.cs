using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using BBUnity.Tiles;

namespace BBUnity.Editor.Tiles {

    /// <summary>
    /// The 3 x 3 grid for one tile rule: the tile in the middle, its eight neighbours around it.
    /// Click a neighbour to cycle it through Any (dot), Same (green: the same cell type) and
    /// Different (red cross: empty or another cell type).
    /// </summary>
    internal class NeighborPicker : VisualElement {

        // Screen layout, top row first, as TileNeighbor values (-1 is the tile itself).
        private static readonly int[] Layout = {
            (int)TileNeighbor.NorthWest, (int)TileNeighbor.North, (int)TileNeighbor.NorthEast,
            (int)TileNeighbor.West, -1, (int)TileNeighbor.East,
            (int)TileNeighbor.SouthWest, (int)TileNeighbor.South, (int)TileNeighbor.SouthEast
        };

        private readonly SerializedObject _serializedObject;
        private readonly string _neighborsPath;
        private readonly Button[] _cells = new Button[TileNeighbors.Count];

        public NeighborPicker(SerializedObject serializedObject, string rulePath) {
            _serializedObject = serializedObject;
            _neighborsPath = rulePath + "._neighbors";

            // A rule added from the list starts without its eight slots.
            serializedObject.Update();
            SerializedProperty neighbors = serializedObject.FindProperty(_neighborsPath);
            if (neighbors.arraySize != TileNeighbors.Count) {
                neighbors.arraySize = TileNeighbors.Count;
                serializedObject.ApplyModifiedProperties();
            }

            style.width = 96;
            style.flexShrink = 0;
            style.marginRight = 8;

            for (int row = 0; row < 3; row++) {
                var line = new VisualElement();
                line.style.flexDirection = FlexDirection.Row;

                for (int column = 0; column < 3; column++) {
                    int neighbor = Layout[row * 3 + column];
                    line.Add(neighbor < 0 ? CreateCenter() : CreateCell(neighbor));
                }

                Add(line);
            }

            Refresh();
            schedule.Execute(Refresh).Every(300);
        }

        private static VisualElement CreateCenter() {
            var center = new Label("tile");
            Style(center);
            center.style.unityTextAlign = TextAnchor.MiddleCenter;
            center.style.backgroundColor = new Color(0.25f, 0.4f, 0.7f);
            return center;
        }

        private Button CreateCell(int neighbor) {
            var button = new Button(() => Cycle(neighbor));
            Style(button);
            _cells[neighbor] = button;
            return button;
        }

        private static void Style(VisualElement element) {
            element.style.width = 30;
            element.style.height = 30;
            element.style.marginLeft = element.style.marginRight = element.style.marginTop = element.style.marginBottom = 1;
            element.style.paddingLeft = element.style.paddingRight = 0;
        }

        private void Cycle(int neighbor) {
            _serializedObject.Update();
            SerializedProperty slot = _serializedObject.FindProperty(_neighborsPath).GetArrayElementAtIndex(neighbor);
            slot.enumValueIndex = (slot.enumValueIndex + 1) % 3;
            _serializedObject.ApplyModifiedProperties();

            Refresh();
        }

        private void Refresh() {
            _serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty neighbors = _serializedObject.FindProperty(_neighborsPath);
            if (neighbors == null || neighbors.arraySize != TileNeighbors.Count) { return; }

            for (int i = 0; i < TileNeighbors.Count; i++) {
                var requirement = (NeighborRequirement)neighbors.GetArrayElementAtIndex(i).enumValueIndex;
                Button cell = _cells[i];

                switch (requirement) {
                    case NeighborRequirement.Same:
                        cell.text = "■";
                        cell.style.backgroundColor = new Color(0.25f, 0.6f, 0.3f);
                        cell.tooltip = "Must be the same cell type as this tile";
                        break;
                    case NeighborRequirement.Different:
                        cell.text = "✕";
                        cell.style.backgroundColor = new Color(0.65f, 0.25f, 0.25f);
                        cell.tooltip = "Must be empty or a different cell type";
                        break;
                    default:
                        cell.text = "·";
                        cell.style.backgroundColor = new Color(0.22f, 0.22f, 0.22f);
                        cell.tooltip = "Doesn't matter";
                        break;
                }
            }
        }
    }
}
