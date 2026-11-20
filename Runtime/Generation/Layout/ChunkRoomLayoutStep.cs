using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Chunks;
using BBUnity.Generation.Grid;
using BBUnity.Generation.Layout.Connectors;
using BBUnity.Generation.Parameters;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Placement;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Layout {

    /// <summary>
    /// Lays a level out from hand-authored chunks: fills the grid with solid, drops chunks from the
    /// template's chunk library at random non-overlapping spots (each becomes a room of the chunk's
    /// room type, and its hand-placed markers become placements), then links the rooms with the
    /// connector. The chunk is drawn including its own walls, so it should be authored as a
    /// complete room with its centre open.
    /// </summary>
    [Serializable]
    public class ChunkRoomLayoutStep : LayoutStep {

        [SerializeField, Tooltip("What the level is filled with around the chunks. Uses the template's default solid if left empty.")]
        private CellTypeDefinition _solid;

        [SerializeField]
        private IntParameter _roomCount = IntParameter.Range(4, 6);

        [SerializeField, Tooltip("Only chunks with this tag are used. Leave empty to use every chunk in the library.")]
        private string _requiredTag;

        [SerializeField, Min(1), Tooltip("Minimum solid cells between two chunks.")]
        private int _gap = 3;

        [SerializeField]
        private IntParameter _corridorWidth = IntParameter.Fixed(3);

        [SerializeField]
        private IntParameter _extraConnections = IntParameter.Range(0, 1);

        [SerializeReference]
        private RoomConnector _connector = new LShapedCorridorConnector();

        private const int Margin = 2;
        private const int AttemptsPerRoom = 60;

        public override string DisplayName {
            get { return "Chunk Room Layout"; }
        }

        public void Configure(RoomConnector connector, string requiredTag = null) {
            _connector = connector;
            _requiredTag = requiredTag;
        }

        public override void Execute(GenerationContext context) {
            CellTypeDefinition solid = _solid != null ? _solid : context.Parameters?.DefaultSolid;
            if (solid == null) {
                context.AddIssue(IssueSeverity.Error, "Chunk Room Layout needs a Solid cell type.");
                return;
            }

            if (_connector == null) {
                context.AddIssue(IssueSeverity.Error, "Chunk Room Layout needs a Room Connector.");
                return;
            }

            List<ChunkDefinition> chunks = UsableChunks(context);
            if (chunks.Count == 0) {
                context.AddIssue(IssueSeverity.Error, "Chunk Room Layout found no chunks to use — add chunks to the template's chunk library" + (string.IsNullOrEmpty(_requiredTag) ? "." : $" with the tag '{_requiredTag}'."));
                return;
            }

            GenerationGrid grid = context.Result.Grid;
            grid.Main.Fill(new RectInt(0, 0, grid.Width, grid.Height), solid);

            int wanted = context.Resolve("layout.roomCount", _roomCount);
            GenerationRandom random = context.Random.Fork("layout.chunks");
            var placed = new List<RectInt>();

            for (int i = 0; i < wanted; i++) {
                if (!TryPlaceChunk(context, grid, chunks, random, placed)) { break; }
            }

            if (placed.Count == 0) {
                context.AddIssue(IssueSeverity.Error, "Chunk Room Layout could not fit any chunk in the level.");
                return;
            }

            if (placed.Count < wanted) {
                context.AddIssue(IssueSeverity.Warning, $"Chunk Room Layout placed {placed.Count} of {wanted} chunks — there was no room for more.");
            }

            RoomLinking.ConnectAll(context, grid.Main, _connector, _corridorWidth, _extraConnections);
        }

        private List<ChunkDefinition> UsableChunks(GenerationContext context) {
            var usable = new List<ChunkDefinition>();
            if (context.ChunkLibrary == null) { return usable; }

            foreach (ChunkDefinition chunk in context.ChunkLibrary.Chunks) {
                if (chunk == null) { continue; }
                if (!string.IsNullOrEmpty(_requiredTag) && !chunk.HasTag(_requiredTag)) { continue; }

                usable.Add(chunk);
            }

            return usable;
        }

        private bool TryPlaceChunk(GenerationContext context, GenerationGrid grid, List<ChunkDefinition> chunks, GenerationRandom random, List<RectInt> placed) {
            for (int attempt = 0; attempt < AttemptsPerRoom; attempt++) {
                ChunkDefinition chunk = random.PickWeighted(chunks, candidate => candidate.Weight);

                int maxX = grid.Width - Margin - chunk.Width;
                int maxY = grid.Height - Margin - chunk.Height;
                if (maxX < Margin || maxY < Margin) { continue; }

                var area = new RectInt(random.Range(Margin, maxX), random.Range(Margin, maxY), chunk.Width, chunk.Height);
                if (Collides(area, placed)) { continue; }

                chunk.Stamp(grid, area.min);
                placed.Add(area);

                GridRoom room = context.Result.Rooms.AddRoom(area, chunk.RoomType);
                foreach (ChunkMarker marker in chunk.Markers) {
                    if (marker.Placeable != null) { context.Result.Placements.Add(new PlacementRecord(marker.Placeable, area.min + marker.Position, room.Id)); }
                }

                return true;
            }

            return false;
        }

        private bool Collides(RectInt area, List<RectInt> placed) {
            var padded = new RectInt(area.x - _gap, area.y - _gap, area.width + _gap * 2, area.height + _gap * 2);

            foreach (RectInt other in placed) {
                if (padded.Overlaps(other)) { return true; }
            }

            return false;
        }
    }
}
