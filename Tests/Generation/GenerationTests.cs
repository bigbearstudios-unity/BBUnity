using System.Collections.Generic;

using NUnit.Framework;
using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Parameters;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Placement;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Tests {
    public class GenerationTests {

        private readonly List<Object> _created = new List<Object>();

        private T Create<T>() where T : ScriptableObject {
            T instance = ScriptableObject.CreateInstance<T>();
            _created.Add(instance);
            return instance;
        }

        [TearDown]
        public void TearDown() {
            foreach (Object created in _created) { Object.DestroyImmediate(created); }
            _created.Clear();
        }

        [Test]
        public void Random_SameSeedGivesSameSequence() {
            var a = new GenerationRandom(42);
            var b = new GenerationRandom(42);

            for (int i = 0; i < 20; i++) { Assert.AreEqual(a.Range(0, 1000), b.Range(0, 1000)); }
        }

        [Test]
        public void Random_RangeStaysInsideBounds() {
            var random = new GenerationRandom(7);

            for (int i = 0; i < 500; i++) {
                int value = random.Range(3, 6);
                Assert.That(value, Is.InRange(3, 6));
            }
        }

        [Test]
        public void Random_ForkIgnoresHowMuchWasDrawn() {
            var drawn = new GenerationRandom(99);
            for (int i = 0; i < 10; i++) { drawn.Range(0, 100); }
            var fresh = new GenerationRandom(99);

            Assert.AreEqual(fresh.Fork("x").Range(0, 100000), drawn.Fork("x").Range(0, 100000));
        }

        [Test]
        public void Parameter_FixedAlwaysResolvesToItsValue() {
            Assert.AreEqual(4, IntParameter.Fixed(4).Resolve(new GenerationRandom(1)));
        }

        [Test]
        public void Parameter_RangeResolvesInsideItsBounds() {
            var random = new GenerationRandom(5);

            for (int i = 0; i < 100; i++) { Assert.That(IntParameter.Range(6, 10).Resolve(random), Is.InRange(6, 10)); }
        }

        [Test]
        public void Context_OverridePinsAParameter() {
            var overrides = new ParameterOverrideSet();
            overrides.SetInt("rooms", 4);
            var context = new GenerationContext(new GenerationRandom(1), null, overrides, new GenerationResult());

            Assert.AreEqual(4, context.Resolve("rooms", IntParameter.Range(6, 10)));
        }

        [Test]
        public void Layer_OutsideTheGridBlocksMovement() {
            var layer = new GridLayer("Main", 4, 4);

            Assert.IsTrue(layer.BlocksMovement(-1, 0));
            Assert.IsTrue(layer.BlocksMovement(4, 0));
            Assert.IsFalse(layer.BlocksMovement(1, 1));
        }

        [Test]
        public void RoomGraph_FindsTheShortestPath() {
            var graph = new RoomGraph();
            GridRoom a = graph.AddRoom(new RectInt(0, 0, 4, 4));
            GridRoom b = graph.AddRoom(new RectInt(5, 0, 4, 4));
            GridRoom c = graph.AddRoom(new RectInt(10, 0, 4, 4));
            GridRoom isolated = graph.AddRoom(new RectInt(20, 0, 4, 4));
            graph.Connect(a, b, ConnectionKind.Horizontal, Vector2Int.zero, Vector2Int.zero);
            graph.Connect(b, c, ConnectionKind.Horizontal, Vector2Int.zero, Vector2Int.zero);

            CollectionAssert.AreEqual(new[] { a, b, c }, graph.FindPath(a, c));
            Assert.IsFalse(graph.AreConnected(a, isolated));
        }

        private GenerationResult CreateGroundedRoom(out CellTypeDefinition solid) {
            solid = Create<CellTypeDefinition>();
            var result = new GenerationResult { Grid = new GenerationGrid(10, 6) };
            result.Grid.Main.Fill(new RectInt(0, 0, 10, 1), solid);
            result.Rooms.AddRoom(new RectInt(0, 0, 10, 6));
            return result;
        }

        [Test]
        public void Finder_FloorCandidatesSitOnTheGround() {
            GenerationResult result = CreateGroundedRoom(out _);
            var placeable = Create<TestPlaceable>();

            List<Vector2Int> candidates = PlacementCandidateFinder.Find(result.Grid.Main, result.Rooms.Rooms[0], placeable, new List<RectInt>());

            Assert.AreEqual(10, candidates.Count);
            foreach (Vector2Int cell in candidates) { Assert.AreEqual(1, cell.y); }
        }

        [Test]
        public void Population_PlacesTheRequestedCountWithoutOverlap() {
            GenerationResult result = CreateGroundedRoom(out _);
            var placeable = Create<TestPlaceable>();
            var step = new PopulationStep(new[] { new ElementBudget(placeable, IntParameter.Fixed(3)) });

            step.Execute(new GenerationContext(new GenerationRandom(3), null, null, result));

            Assert.AreEqual(3, result.Placements.Count);
            Assert.AreEqual(3, new HashSet<Vector2Int>(result.Placements.ConvertAll(p => p.Cell)).Count);
            Assert.AreEqual(0, result.Issues.Count);
        }

        [Test]
        public void Population_ReportsAShortfallAsAWarning() {
            GenerationResult result = CreateGroundedRoom(out _);
            var placeable = Create<TestPlaceable>();
            var step = new PopulationStep(new[] { new ElementBudget(placeable, IntParameter.Fixed(50)) });

            step.Execute(new GenerationContext(new GenerationRandom(3), null, null, result));

            Assert.AreEqual(10, result.Placements.Count);
            Assert.AreEqual(IssueSeverity.Warning, result.Issues[0].Severity);
        }
    }
}
