using UnityEngine;

using BBUnity.Generation.Pipeline;
using BBUnity.World.Elements;
using BBUnity.World.Generation;

namespace BBUnity.Tiles {

    /// <summary>
    /// The tilemap layer of a generated level: paints the generation grid with a
    /// TilePaletteDefinition. Pass one to LevelBuilder.Build to get tilemaps.
    /// </summary>
    public class TilemapLevelPainter : ILevelLayerBuilder {
        private readonly TilePaletteDefinition _palette;

        public TilemapLevelPainter(TilePaletteDefinition palette) {
            _palette = palette;
        }

        public string ContainerName => GeneratedLevelRoot.TilesContainer;

        public void Build(GenerationResult result, GridToWorld mapping, Transform container) {
            TilemapPainter.Paint(result.Grid, _palette, container, mapping.Origin, mapping.CellSize, result.Seed);
        }
    }
}
