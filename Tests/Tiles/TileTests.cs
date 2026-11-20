using System.Collections.Generic;
using System.Reflection;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

using BBUnity.Generation.Grid;
using BBUnity.Tiles;

namespace BBUnity.Tiles.Tests {
    public class TileTests {

        private readonly List<Object> _created = new List<Object>();
        private CellTypeDefinition _solid;

        private T Create<T>() where T : ScriptableObject {
            T instance = ScriptableObject.CreateInstance<T>();
            _created.Add(instance);
            return instance;
        }

        [SetUp]
        public void SetUp() {
            _solid = Create<CellTypeDefinition>();
        }

        [TearDown]
        public void TearDown() {
            foreach (Object created in _created) { Object.DestroyImmediate(created); }
            _created.Clear();
        }

        private GridLayer CreateLayer(int width, int height, params RectInt[] solidAreas) {
            GridLayer layer = new GenerationGrid(width, height).Main;
            foreach (RectInt area in solidAreas) { layer.Fill(area, _solid); }
            return layer;
        }

        private Tile NewTile(string name) {
            Tile tile = Create<Tile>();
            tile.name = name;
            return tile;
        }

        private TileRuleSetDefinition CreateEdgeCornerRuleSet() {
            var tiles = new EdgeCornerTileSet {
                Center = NewTile("Center"), Top = NewTile("Top"), Bottom = NewTile("Bottom"), Left = NewTile("Left"), Right = NewTile("Right"),
                TopLeft = NewTile("TopLeft"), TopRight = NewTile("TopRight"), BottomLeft = NewTile("BottomLeft"), BottomRight = NewTile("BottomRight"),
                InnerTopLeft = NewTile("InnerTopLeft"), InnerTopRight = NewTile("InnerTopRight"), InnerBottomLeft = NewTile("InnerBottomLeft"), InnerBottomRight = NewTile("InnerBottomRight"),
                Single = NewTile("Single")
            };

            var ruleSet = Create<TileRuleSetDefinition>();
            ruleSet.SetRules(TileRuleGenerator.FromEdgeCornerSet(tiles), tiles.Single);
            return ruleSet;
        }

        [Test]
        public void SameMask_MarksOnlyTheSameNeighbours() {
            GridLayer layer = CreateLayer(7, 7, new RectInt(2, 2, 3, 3));

            int topLeft = TileRuleResolver.ComputeSameMask(layer, 2, 4, _solid, true);

            // East, south-east and south are the only neighbours inside the block.
            Assert.AreEqual(TileNeighbors.Bit(TileNeighbor.East) | TileNeighbors.Bit(TileNeighbor.SouthEast) | TileNeighbors.Bit(TileNeighbor.South), topLeft);
        }

        [Test]
        public void EdgeCornerSet_PicksTheRightTileAcrossABlock() {
            TileRuleSetDefinition ruleSet = CreateEdgeCornerRuleSet();
            GridLayer layer = CreateLayer(7, 7, new RectInt(2, 2, 3, 3));

            Assert.AreEqual("Center", TileRuleResolver.Resolve(layer, 3, 3, ruleSet, 1).name);
            Assert.AreEqual("TopLeft", TileRuleResolver.Resolve(layer, 2, 4, ruleSet, 1).name);
            Assert.AreEqual("Top", TileRuleResolver.Resolve(layer, 3, 4, ruleSet, 1).name);
            Assert.AreEqual("TopRight", TileRuleResolver.Resolve(layer, 4, 4, ruleSet, 1).name);
            Assert.AreEqual("Right", TileRuleResolver.Resolve(layer, 4, 3, ruleSet, 1).name);
            Assert.AreEqual("BottomRight", TileRuleResolver.Resolve(layer, 4, 2, ruleSet, 1).name);
            Assert.AreEqual("Bottom", TileRuleResolver.Resolve(layer, 3, 2, ruleSet, 1).name);
            Assert.AreEqual("BottomLeft", TileRuleResolver.Resolve(layer, 2, 2, ruleSet, 1).name);
            Assert.AreEqual("Left", TileRuleResolver.Resolve(layer, 2, 3, ruleSet, 1).name);
        }

        [Test]
        public void EdgeCornerSet_UsesAnInnerCornerWhereANotchIsCutOut() {
            TileRuleSetDefinition ruleSet = CreateEdgeCornerRuleSet();
            GridLayer layer = CreateLayer(9, 9, new RectInt(1, 1, 5, 5));
            layer.Set(5, 5, null);

            Assert.AreEqual("InnerTopRight", TileRuleResolver.Resolve(layer, 4, 4, ruleSet, 1).name);
        }

        [Test]
        public void EdgeCornerSet_FallsBackToTheSingleTile() {
            TileRuleSetDefinition ruleSet = CreateEdgeCornerRuleSet();
            GridLayer layer = CreateLayer(5, 5, new RectInt(2, 2, 1, 1));

            Assert.AreEqual("Single", TileRuleResolver.Resolve(layer, 2, 2, ruleSet, 1).name);
        }

        [Test]
        public void EdgeCornerSet_TreatsTheLevelBorderAsContinuingSolid() {
            TileRuleSetDefinition ruleSet = CreateEdgeCornerRuleSet();
            GridLayer layer = CreateLayer(6, 6, new RectInt(0, 0, 3, 3));

            Assert.AreEqual("Center", TileRuleResolver.Resolve(layer, 0, 0, ruleSet, 1).name);
        }

        [Test]
        public void HorizontalStrip_PicksCapsMiddleAndSingle() {
            var ruleSet = Create<TileRuleSetDefinition>();
            ruleSet.SetRules(TileRuleGenerator.FromHorizontalStrip(NewTile("Left"), NewTile("Middle"), NewTile("Right"), NewTile("Single")), null);
            GridLayer layer = CreateLayer(12, 4, new RectInt(2, 1, 4, 1), new RectInt(8, 1, 1, 1));

            Assert.AreEqual("Left", TileRuleResolver.Resolve(layer, 2, 1, ruleSet, 1).name);
            Assert.AreEqual("Middle", TileRuleResolver.Resolve(layer, 3, 1, ruleSet, 1).name);
            Assert.AreEqual("Right", TileRuleResolver.Resolve(layer, 5, 1, ruleSet, 1).name);
            Assert.AreEqual("Single", TileRuleResolver.Resolve(layer, 8, 1, ruleSet, 1).name);
        }

        [Test]
        public void Rule_PicksVariantsByWeightAndRoll() {
            Tile first = NewTile("First");
            Tile second = NewTile("Second");
            var rule = new TileRule("Variants", first).AddOutput(second, 1f);

            Assert.AreEqual(first, rule.Pick(0.1f));
            Assert.AreEqual(second, rule.Pick(0.9f));
        }

        [Test]
        public void Roll_IsRepeatableAndInRange() {
            Assert.AreEqual(TileRuleResolver.Roll(5, 9, 3), TileRuleResolver.Roll(5, 9, 3));
            Assert.That(TileRuleResolver.Roll(5, 9, 3), Is.InRange(0f, 1f));
        }

        [Test]
        public void Painter_PaintsTilesAndAddsCollidersToSolidLayers() {
            TileRuleSetDefinition ruleSet = CreateEdgeCornerRuleSet();
            var palette = Create<TilePaletteDefinition>();
            typeof(TilePaletteDefinition).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(palette, new List<TilePaletteEntry> { new TilePaletteEntry(_solid, ruleSet, "Ground") });

            var grid = new GenerationGrid(7, 7);
            grid.Main.Fill(new RectInt(2, 2, 3, 3), _solid);
            var parent = new GameObject("Painter Test");

            try {
                List<Tilemap> tilemaps = TilemapPainter.Paint(grid, palette, parent.transform, new Vector2(10f, 20f), 1f, 1);

                Assert.AreEqual(1, tilemaps.Count);
                Assert.AreEqual("Center", tilemaps[0].GetTile(new Vector3Int(3, 3, 0)).name);
                Assert.IsNull(tilemaps[0].GetTile(new Vector3Int(0, 0, 0)));
                Assert.IsNotNull(tilemaps[0].GetComponent<TilemapCollider2D>());
                Assert.IsNotNull(tilemaps[0].GetComponent<CompositeCollider2D>());
                Assert.AreEqual(new Vector3(10f, 20f, 0f), parent.transform.Find(TilemapPainter.GridName).position);
            } finally {
                Object.DestroyImmediate(parent);
            }
        }
    }
}
