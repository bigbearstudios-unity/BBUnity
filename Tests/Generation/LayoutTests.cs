using System.Collections.Generic;

using NUnit.Framework;
using UnityEngine;

using BBUnity.Generation.Chunks;
using BBUnity.Generation.Grid;
using BBUnity.Generation.Layout;
using BBUnity.Generation.Parameters;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Tests {
    public class LayoutTests {

        private CellTypeDefinition _solid;

        [SetUp]
        public void SetUp() {
            _solid = ScriptableObject.CreateInstance<CellTypeDefinition>();
        }

        [TearDown]
        public void TearDown() {
            Object.DestroyImmediate(_solid);
        }

        // The step's settings are private serialized fields, as they are for any step in a
        // template, so tests set them the way the editor does.
        private RoomLayoutStep CreateRoomLayout() {
            var step = new RoomLayoutStep();
            typeof(RoomLayoutStep).GetField("_solid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(step, _solid);
            return step;
        }

        private GenerationResult Run(GenerationStep step, int seed, int width = 80, int height = 50, params GenerationStep[] after) {
            var result = new GenerationResult { Seed = seed, Grid = new GenerationGrid(width, height) };
            var context = new GenerationContext(new GenerationRandom(seed), null, null, result);

            step.Execute(context);
            foreach (GenerationStep next in after) { next.Execute(context); }
            return result;
        }

        [Test]
        public void RoomLayout_EveryRoomIsReachableFromTheFirst() {
            for (int seed = 1; seed <= 30; seed++) {
                GenerationResult result = Run(CreateRoomLayout(), seed);
                GridLayer layer = result.Grid.Main;

                Assert.Greater(result.Rooms.Rooms.Count, 1, $"seed {seed}");
                HashSet<Vector2Int> reachable = GridAnalysis.Reachable(layer, result.Rooms.Rooms[0].Center);

                foreach (GridRoom room in result.Rooms.Rooms) {
                    Assert.IsTrue(reachable.Contains(room.Center), $"seed {seed}: room {room.Id} cannot be reached");
                }
            }
        }

        [Test]
        public void RoomLayout_GraphConnectsEveryRoom() {
            GenerationResult result = Run(CreateRoomLayout(), 11);

            foreach (GridRoom room in result.Rooms.Rooms) {
                Assert.IsTrue(result.Rooms.AreConnected(result.Rooms.Rooms[0], room));
            }
        }

        [Test]
        public void RoomLayout_SameSeedGivesSameLevel() {
            GenerationResult a = Run(CreateRoomLayout(), 77);
            GenerationResult b = Run(CreateRoomLayout(), 77);

            Assert.AreEqual(a.Rooms.Rooms.Count, b.Rooms.Rooms.Count);
            for (int y = 0; y < 50; y++) {
                for (int x = 0; x < 80; x++) { Assert.AreEqual(a.Grid.Main.Get(x, y), b.Grid.Main.Get(x, y)); }
            }
        }

        [Test]
        public void RoomLayout_WithoutASolidTypeReportsAnError() {
            GenerationResult result = Run(new RoomLayoutStep(), 1);

            Assert.IsTrue(result.HasErrors);
        }

        [Test]
        public void ConnectionPlanner_ConnectsEveryRoomWithAtLeastATree() {
            var graph = new RoomGraph();
            for (int i = 0; i < 6; i++) { graph.AddRoom(new RectInt(i * 10, (i % 2) * 8, 4, 4)); }

            var pairs = ConnectionPlanner.Plan(graph.Rooms, 0, new GenerationRandom(1));

            Assert.AreEqual(5, pairs.Count);
        }

        private CellularCaveLayoutStep CreateCave() {
            var step = new CellularCaveLayoutStep();
            typeof(CellularCaveLayoutStep).GetField("_solid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(step, _solid);
            return step;
        }

        [Test]
        public void Cave_KeepsASingleConnectedOpenRegion() {
            for (int seed = 1; seed <= 20; seed++) {
                GenerationResult result = Run(CreateCave(), seed, 60, 40);

                List<List<Vector2Int>> regions = GridAnalysis.OpenRegions(result.Grid.Main);
                Assert.AreEqual(1, regions.Count, $"seed {seed}");
            }
        }

        [Test]
        public void RoomDetection_TurnsACaveIntoConnectedRooms() {
            // Any block with open space becomes a room, so a thin passage through an almost solid
            // block can't leave a gap in the graph.
            var detection = new RoomDetectionStep();
            typeof(RoomDetectionStep).GetField("_minOpenCells", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(detection, 1);

            GenerationResult result = Run(CreateCave(), 5, 80, 48, detection);

            Assert.Greater(result.Rooms.Rooms.Count, 1);
            foreach (GridRoom room in result.Rooms.Rooms) {
                Assert.IsTrue(result.Rooms.AreConnected(result.Rooms.Rooms[0], room), $"room {room.Id}");
            }
        }

        [Test]
        public void Carving_LShapeJoinsTwoPoints() {
            var grid = new GenerationGrid(20, 20);
            grid.Main.Fill(new RectInt(0, 0, 20, 20), _solid);

            Carving.LShape(grid.Main, new Vector2Int(2, 2), new Vector2Int(15, 12), 3, true);

            Assert.IsTrue(GridAnalysis.Reachable(grid.Main, new Vector2Int(2, 2)).Contains(new Vector2Int(15, 12)));
        }

        private ChunkDefinition CreateChunk(int width, int height) {
            var chunk = ScriptableObject.CreateInstance<ChunkDefinition>();
            chunk.Resize(width, height);

            // A room: solid walls all round, open inside.
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    bool wall = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                    chunk.SetCell(GridLayerNames.Main, x, y, wall ? _solid : null);
                }
            }

            return chunk;
        }

        [Test]
        public void Chunk_ResizeKeepsCellsThatStillFit() {
            ChunkDefinition chunk = CreateChunk(6, 5);

            try {
                chunk.Resize(8, 4);

                Assert.AreEqual(_solid, chunk.GetCell(GridLayerNames.Main, 0, 0));
                Assert.IsNull(chunk.GetCell(GridLayerNames.Main, 2, 2));
                Assert.AreEqual(8, chunk.Width);
                Assert.AreEqual(4, chunk.Height);
            } finally {
                Object.DestroyImmediate(chunk);
            }
        }

        [Test]
        public void ChunkRoomLayout_PlacesConnectedRoomsFromTheLibrary() {
            ChunkDefinition small = CreateChunk(10, 7);
            ChunkDefinition large = CreateChunk(14, 9);
            var library = ScriptableObject.CreateInstance<ChunkLibraryDefinition>();
            library.SetChunks(new[] { small, large });

            try {
                for (int seed = 1; seed <= 15; seed++) {
                    var result = new GenerationResult { Seed = seed, Grid = new GenerationGrid(80, 50) };
                    var context = new GenerationContext(new GenerationRandom(seed), null, null, result, library);

                    var step = new ChunkRoomLayoutStep();
                    typeof(ChunkRoomLayoutStep).GetField("_solid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(step, _solid);
                    step.Execute(context);

                    Assert.IsFalse(result.HasErrors, $"seed {seed}");
                    Assert.Greater(result.Rooms.Rooms.Count, 1, $"seed {seed}");

                    HashSet<Vector2Int> reachable = GridAnalysis.Reachable(result.Grid.Main, result.Rooms.Rooms[0].Center);
                    foreach (GridRoom room in result.Rooms.Rooms) {
                        Assert.IsTrue(reachable.Contains(room.Center), $"seed {seed}: room {room.Id} cannot be reached");
                    }
                }
            } finally {
                Object.DestroyImmediate(library);
                Object.DestroyImmediate(small);
                Object.DestroyImmediate(large);
            }
        }

        [Test]
        public void ChunkRoomLayout_WithoutALibraryReportsAnError() {
            var result = new GenerationResult { Grid = new GenerationGrid(40, 30) };
            var step = new ChunkRoomLayoutStep();
            typeof(ChunkRoomLayoutStep).GetField("_solid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(step, _solid);

            step.Execute(new GenerationContext(new GenerationRandom(1), null, null, result));

            Assert.IsTrue(result.HasErrors);
        }
    }
}
