using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

using BBUnity.Generation.Grid;

namespace BBUnity.Tiles {

    /// <summary>
    /// Paints a GenerationGrid into Unity Tilemaps using a TilePaletteDefinition: one Tilemap per
    /// palette layer under a new Grid object, each with the renderer and colliders its settings
    /// ask for. It uses no editor APIs, so it works in the editor and at runtime.
    /// </summary>
    public static class TilemapPainter {

        public const string GridName = "Tile Grid";

        public static List<Tilemap> Paint(GenerationGrid grid, TilePaletteDefinition palette, Transform parent, Vector2 origin, float cellSize, int seed) {
            var gridObject = new GameObject(GridName);
            gridObject.transform.SetParent(parent, false);
            gridObject.transform.position = origin;
            gridObject.AddComponent<Grid>().cellSize = new Vector3(cellSize, cellSize, 1f);

            var tilemaps = new Dictionary<string, Tilemap>();

            foreach (GridLayer layer in grid.Layers) {
                for (int y = 0; y < layer.Height; y++) {
                    for (int x = 0; x < layer.Width; x++) {
                        CellTypeDefinition type = layer.Get(x, y);
                        if (type == null || !palette.TryGetEntry(type, out TilePaletteEntry entry)) { continue; }

                        TileBase tile = TileRuleResolver.Resolve(layer, x, y, entry.RuleSet, seed);
                        if (tile == null) { continue; }

                        GetOrCreate(tilemaps, gridObject.transform, palette, entry.LayerName).SetTile(new Vector3Int(x, y, 0), tile);
                    }
                }
            }

            return new List<Tilemap>(tilemaps.Values);
        }

        private static Tilemap GetOrCreate(Dictionary<string, Tilemap> tilemaps, Transform parent, TilePaletteDefinition palette, string layerName) {
            if (tilemaps.TryGetValue(layerName, out Tilemap existing)) { return existing; }

            TilemapLayerSettings settings = palette.FindLayer(layerName);

            var tilemapObject = new GameObject(layerName);
            tilemapObject.transform.SetParent(parent, false);
            tilemapObject.layer = settings.PhysicsLayer;

            var tilemap = tilemapObject.AddComponent<Tilemap>();
            var renderer = tilemapObject.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = settings.SortingOrder;
            if (!string.IsNullOrEmpty(settings.SortingLayerName)) { renderer.sortingLayerName = settings.SortingLayerName; }

            AddColliders(tilemapObject, settings.Collision);

            tilemaps[layerName] = tilemap;
            return tilemap;
        }

        // The tilemap collider is merged into one composite collider on a static body, which
        // keeps a large level cheap and removes seams between adjacent tiles.
        private static void AddColliders(GameObject tilemapObject, TilemapCollisionMode mode) {
            if (mode == TilemapCollisionMode.None) { return; }

            tilemapObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

            var composite = tilemapObject.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;

            var tilemapCollider = tilemapObject.AddComponent<TilemapCollider2D>();
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

            if (mode == TilemapCollisionMode.OneWay) {
                var effector = tilemapObject.AddComponent<PlatformEffector2D>();
                effector.useOneWay = true;
                effector.surfaceArc = 170f;
                composite.usedByEffector = true;
            }
        }
    }
}
