using System.Collections.Generic;
using System.Reflection;

using NUnit.Framework;
using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Layout;
using BBUnity.Generation.Pipeline;
using BBUnity.Platformer.Generation.Layout;
using BBUnity.Platformer.Generation.Movement;
using BBUnity.Platformer.Generation.Reachability;
using BBUnity.Platformer.Generation.Steps;
using BBUnity.Generation.Rooms;

namespace BBUnity.Platformer.Generation.Tests {
    public class PlatformerTests {

        private readonly List<Object> _created = new List<Object>();

        private CellTypeDefinition _solid;
        private CellTypeDefinition _platform;
        private RoomTypeDefinition _startRoom;
        private RoomTypeDefinition _exitRoom;
        private BBUnity.Generation.Tests.TestPlaceable _spawn;
        private BBUnity.Generation.Tests.TestPlaceable _exit;

        private T Create<T>() where T : ScriptableObject {
            T instance = ScriptableObject.CreateInstance<T>();
            _created.Add(instance);
            return instance;
        }

        private static void SetField(object target, string name, object value) {
            target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
        }

        [SetUp]
        public void SetUp() {
            _solid = Create<CellTypeDefinition>();

            _platform = Create<CellTypeDefinition>();
            SetField(_platform, "_blocksMovement", false);

            _startRoom = Create<RoomTypeDefinition>();
            _exitRoom = Create<RoomTypeDefinition>();
            _spawn = Create<BBUnity.Generation.Tests.TestPlaceable>();
            _exit = Create<BBUnity.Generation.Tests.TestPlaceable>();
        }

        [TearDown]
        public void TearDown() {
            foreach (Object created in _created) { Object.DestroyImmediate(created); }
            _created.Clear();
        }

        private PlatformerGenerationParameters CreateParameters() {
            return new PlatformerGenerationParameters(PlatformerMovementProfile.Default, _solid, _platform, _startRoom, _exitRoom, _spawn, _exit);
        }

        [Test]
        public void Profile_DerivesJumpHeightAndDistance() {
            PlatformerMovementProfile profile = PlatformerMovementProfile.Default;

            Assert.AreEqual(2.84f, profile.MaxJumpHeight, 0.01f);
            Assert.AreEqual(4.09f, profile.MaxJumpDistance, 0.01f);
        }

        // A flat floor along the bottom row, open above.
        private GridLayer CreateFloor(int width = 40, int height = 14) {
            var layer = new GenerationGrid(width, height).Main;
            layer.Fill(new RectInt(0, 0, width, 1), _solid);
            return layer;
        }

        [Test]
        public void Reachability_PlayerCanJumpOntoALowLedge() {
            GridLayer layer = CreateFloor();
            layer.Fill(new RectInt(8, 1, 5, 2), _solid);
            var reachability = new PlatformerReachability(layer, PlatformerMovementProfile.Default);

            HashSet<Vector2Int> reached = reachability.ReachableFrom(new[] { new Vector2Int(2, 1) });

            Assert.IsTrue(reached.Contains(new Vector2Int(10, 3)));
            Assert.IsTrue(reached.Contains(new Vector2Int(20, 1)));
        }

        [Test]
        public void Reachability_PlayerCannotJumpOntoATallLedge() {
            GridLayer layer = CreateFloor();
            layer.Fill(new RectInt(8, 1, 5, 4), _solid);
            var reachability = new PlatformerReachability(layer, PlatformerMovementProfile.Default);

            HashSet<Vector2Int> reached = reachability.ReachableFrom(new[] { new Vector2Int(2, 1) });

            Assert.IsFalse(reached.Contains(new Vector2Int(10, 5)));
            Assert.IsFalse(reached.Contains(new Vector2Int(20, 1)));
        }

        [Test]
        public void Reachability_PlayerCanCrossASmallGapButNotALargeOne() {
            foreach ((int gap, bool crossable) in new[] { (3, true), (8, false) }) {
                var layer = new GenerationGrid(40, 14).Main;
                layer.Fill(new RectInt(0, 0, 10, 1), _solid);
                layer.Fill(new RectInt(10 + gap, 0, 40 - 10 - gap, 1), _solid);
                var reachability = new PlatformerReachability(layer, PlatformerMovementProfile.Default);

                HashSet<Vector2Int> reached = reachability.ReachableFrom(new[] { new Vector2Int(2, 1) });

                Assert.AreEqual(crossable, reached.Contains(new Vector2Int(10 + gap + 2, 1)), $"gap of {gap}");
            }
        }

        [Test]
        public void Reachability_OneWayPlatformCatchesThePlayerFromAbove() {
            GridLayer layer = CreateFloor();
            layer.Fill(new RectInt(10, 3, 4, 1), _platform);
            var reachability = new PlatformerReachability(layer, PlatformerMovementProfile.Default);

            HashSet<Vector2Int> reached = reachability.ReachableFrom(new[] { new Vector2Int(2, 1) });

            Assert.IsTrue(reached.Contains(new Vector2Int(11, 4)));
        }

        [Test]
        public void RoomAssignment_PicksTheLeftmostRoomAsStartAndSetsTypes() {
            var result = new GenerationResult { Grid = new GenerationGrid(60, 20) };
            result.Grid.Main.Fill(new RectInt(0, 0, 60, 20), _solid);
            GridRoom left = result.Rooms.AddRoom(new RectInt(2, 2, 10, 8));
            GridRoom middle = result.Rooms.AddRoom(new RectInt(20, 2, 10, 8));
            GridRoom right = result.Rooms.AddRoom(new RectInt(40, 2, 10, 8));
            foreach (GridRoom room in result.Rooms.Rooms) { Carving.Rect(result.Grid.Main, room.Bounds); }
            result.Rooms.Connect(left, middle, ConnectionKind.Horizontal, Vector2Int.zero, Vector2Int.zero);
            result.Rooms.Connect(middle, right, ConnectionKind.Horizontal, Vector2Int.zero, Vector2Int.zero);

            new PlatformerRoomAssignmentStep().Execute(new GenerationContext(new GenerationRandom(1), CreateParameters(), null, result));

            Assert.AreEqual(_startRoom, left.Type);
            Assert.AreEqual(_exitRoom, right.Type);
            Assert.AreEqual(2, result.Placements.Count);
        }

        private GenerationResult GenerateLevel(int seed) {
            var result = new GenerationResult { Seed = seed, Grid = new GenerationGrid(90, 50) };
            var context = new GenerationContext(new GenerationRandom(seed), CreateParameters(), null, result);

            var layout = new RoomLayoutStep();
            SetField(layout, "_connector", new PlatformerCorridorConnector());

            GenerationPipeline.Run(new GenerationStep[] {
                layout,
                new PlatformerRoomAssignmentStep(),
                new PlatformerPlatformStep(),
                new PlatformerValidationStep()
            }, context);

            return result;
        }

        [Test]
        public void FullPipeline_ProducesLevelsWhereTheExitCanBeReached() {
            for (int seed = 1; seed <= 8; seed++) {
                GenerationResult result = GenerateLevel(seed);

                var errors = new List<string>();
                foreach (GenerationIssue issue in result.Issues) {
                    if (issue.Severity == IssueSeverity.Error) { errors.Add(issue.Message); }
                }

                Assert.IsEmpty(errors, $"seed {seed}: {string.Join("; ", errors)}");
            }
        }

        [Test]
        public void FullPipeline_SameSeedGivesSameLevel() {
            GenerationResult a = GenerateLevel(21);
            GenerationResult b = GenerateLevel(21);

            for (int y = 0; y < 50; y++) {
                for (int x = 0; x < 90; x++) { Assert.AreEqual(a.Grid.Main.Get(x, y), b.Grid.Main.Get(x, y)); }
            }
        }
    }
}
