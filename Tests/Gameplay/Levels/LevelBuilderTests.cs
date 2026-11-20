using System.Reflection;

using NUnit.Framework;
using UnityEngine;

using BBUnity.Gameplay.Levels.Elements;
using BBUnity.Gameplay.Levels.Generation;
using BBUnity.Generation.Grid;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Placement;

namespace BBUnity.Gameplay.Levels.Tests {
    public class LevelBuilderTests {

        private GameObject _prefab;
        private LevelElementDefinition _element;
        private GameObject _built;

        [SetUp]
        public void SetUp() {
            _prefab = new GameObject("Spawn Marker");
            _element = ScriptableObject.CreateInstance<LevelElementDefinition>();
            typeof(LevelElementDefinition).GetField("_prefab", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_element, _prefab);
        }

        [TearDown]
        public void TearDown() {
            if (_built != null) { Object.DestroyImmediate(_built); }
            Object.DestroyImmediate(_prefab);
            Object.DestroyImmediate(_element);
        }

        private GenerationResult CreateResult() {
            var result = new GenerationResult { Seed = 42, Grid = new GenerationGrid(20, 10) };
            result.Rooms.AddRoom(new RectInt(2, 1, 10, 6));
            result.Placements.Add(new PlacementRecord(_element, new Vector2Int(4, 2), 0));
            return result;
        }

        [Test]
        public void GridToWorld_MapsCellsAndFootprints() {
            var mapping = new GridToWorld(new Vector2(10f, 5f), 2f);

            Assert.AreEqual(new Vector2(14f, 9f), mapping.CellCorner(new Vector2Int(2, 2)));
            Assert.AreEqual(new Vector2(15f, 9f), mapping.FootprintBottomCenter(new RectInt(2, 2, 1, 1)));
        }

        [Test]
        public void Build_CreatesRootRoomsAndPlacedElements() {
            var mapping = new GridToWorld(Vector2.zero, 1f);

            GeneratedLevelRoot root = LevelBuilder.Build(CreateResult(), mapping, "Test Template");
            _built = root.gameObject;

            Assert.AreEqual(42, root.Seed);
            Assert.AreEqual("Test Template", root.TemplateName);

            LevelRoom room = root.GetComponentInChildren<LevelRoom>();
            Assert.AreEqual(new Rect(2f, 1f, 10f, 6f), room.Bounds);

            LevelElement placed = root.GetComponentInChildren<LevelElement>();
            Assert.AreSame(_element, placed.Definition);
            Assert.AreEqual(new Vector3(4.5f, 2f, 0f), placed.transform.position);
        }
    }
}
