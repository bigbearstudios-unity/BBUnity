using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Parameters;
using BBUnity.Generation.Pipeline;

namespace BBUnity.Generation.Layout {

    /// <summary>
    /// Carves a cave with cellular automata: the level starts as random noise and is smoothed a
    /// few times, each cell becoming solid if enough of its neighbours are. Optionally everything
    /// but the largest cavern is filled back in, which guarantees the open space is one connected
    /// piece. Follow it with a RoomDetectionStep to turn the cave into rooms.
    /// </summary>
    [Serializable]
    public class CellularCaveLayoutStep : LayoutStep {

        [SerializeField, Tooltip("What the cave walls are made of. Uses the template's default solid if left empty.")]
        private CellTypeDefinition _solid;

        [SerializeField, Tooltip("Chance of a cell starting as wall. Around 0.45 gives open, connected caves.")]
        private FloatParameter _fillChance = FloatParameter.Fixed(0.45f);

        [SerializeField, Tooltip("How many times the noise is smoothed. More passes give rounder caverns.")]
        private IntParameter _smoothingPasses = IntParameter.Fixed(4);

        [SerializeField, Tooltip("Fill in every cavern except the largest, so the cave is one connected space.")]
        private bool _keepLargestRegion = true;

        public override string DisplayName {
            get { return "Cellular Cave Layout"; }
        }

        public override void Execute(GenerationContext context) {
            CellTypeDefinition solid = _solid != null ? _solid : context.Parameters?.DefaultSolid;
            if (solid == null) {
                context.AddIssue(IssueSeverity.Error, "Cellular Cave Layout needs a Solid cell type.");
                return;
            }

            GridLayer layer = MainLayer(context);
            GenerationRandom random = context.Random.Fork("cave");
            float fill = context.Resolve("cave.fillChance", _fillChance);
            int passes = context.Resolve("cave.smoothingPasses", _smoothingPasses);

            bool[,] walls = new bool[layer.Width, layer.Height];
            for (int y = 0; y < layer.Height; y++) {
                for (int x = 0; x < layer.Width; x++) {
                    bool edge = x == 0 || y == 0 || x == layer.Width - 1 || y == layer.Height - 1;
                    walls[x, y] = edge || random.Chance(fill);
                }
            }

            for (int pass = 0; pass < passes; pass++) { walls = Smooth(walls, layer.Width, layer.Height); }

            for (int y = 0; y < layer.Height; y++) {
                for (int x = 0; x < layer.Width; x++) { layer.Set(x, y, walls[x, y] ? solid : null); }
            }

            if (_keepLargestRegion) { FillSmallerRegions(layer, solid); }

            if (GridAnalysis.CountOpen(layer) == 0) {
                context.AddIssue(IssueSeverity.Error, "The cave came out completely solid — lower the Fill Chance.");
            }
        }

        // A cell becomes a wall when five or more of its eight neighbours are walls (the edge counts as wall).
        private static bool[,] Smooth(bool[,] walls, int width, int height) {
            var next = new bool[width, height];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int neighbours = 0;

                    for (int ny = y - 1; ny <= y + 1; ny++) {
                        for (int nx = x - 1; nx <= x + 1; nx++) {
                            if (nx == x && ny == y) { continue; }
                            if (nx < 0 || ny < 0 || nx >= width || ny >= height || walls[nx, ny]) { neighbours++; }
                        }
                    }

                    next[x, y] = neighbours >= 5 || (neighbours == 4 && walls[x, y]);
                }
            }

            return next;
        }

        private static void FillSmallerRegions(GridLayer layer, CellTypeDefinition solid) {
            List<List<Vector2Int>> regions = GridAnalysis.OpenRegions(layer);

            for (int i = 1; i < regions.Count; i++) {
                foreach (Vector2Int cell in regions[i]) { layer.Set(cell, solid); }
            }
        }
    }
}
