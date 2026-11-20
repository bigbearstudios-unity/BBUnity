using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Placement;
using BBUnity.Generation.Rooms;

namespace BBUnity.Platformer.Generation.Steps {

    /// <summary>
    /// Chooses the start and exit rooms — the leftmost room, and the room furthest from it through
    /// the room graph — gives them their room types, and places the player spawn and the level
    /// exit on their floors. Run it after the layout and before the platform step.
    /// </summary>
    [Serializable]
    public class PlatformerRoomAssignmentStep : GenerationStep {

        public override string DisplayName {
            get { return "Assign Start and Exit"; }
        }

        public override void Execute(GenerationContext context) {
            PlatformerGenerationParameters parameters = PlatformerGenerationParameters.From(context, DisplayName);
            if (parameters == null) { return; }

            RoomGraph graph = context.Result.Rooms;
            if (graph.Rooms.Count < 2) {
                context.AddIssue(IssueSeverity.Error, "A level needs at least two rooms for a start and an exit.");
                return;
            }

            GridRoom start = graph.Rooms[0];
            foreach (GridRoom room in graph.Rooms) {
                if (room.Center.x < start.Center.x) { start = room; }
            }

            GridRoom exit = null;
            int longest = -1;
            foreach (GridRoom room in graph.Rooms) {
                int length = graph.FindPath(start, room).Count;
                if (room != start && length > longest) { longest = length; exit = room; }
            }

            if (parameters.StartRoomType != null) { start.Type = parameters.StartRoomType; }
            if (parameters.ExitRoomType != null) { exit.Type = parameters.ExitRoomType; }

            PlaceAtFloor(context, parameters.PlayerSpawn, start);
            PlaceAtFloor(context, parameters.LevelExit, exit);
        }

        private static void PlaceAtFloor(GenerationContext context, PlaceableDefinition placeable, GridRoom room) {
            if (placeable == null) { return; }

            var occupied = new List<RectInt>();
            foreach (PlacementRecord existing in context.Result.Placements) { occupied.Add(existing.Footprint); }

            List<Vector2Int> candidates = PlacementCandidateFinder.Find(context.Result.Grid.Main, room, placeable, occupied);
            if (candidates.Count == 0) {
                context.AddIssue(IssueSeverity.Error, $"No room on the floor of the {room.Type?.DisplayName ?? "chosen"} room for '{placeable.DisplayName}'.");
                return;
            }

            Vector2Int best = candidates[0];
            foreach (Vector2Int candidate in candidates) {
                if ((candidate - room.Center).sqrMagnitude < (best - room.Center).sqrMagnitude) { best = candidate; }
            }

            context.Result.Placements.Add(new PlacementRecord(placeable, best, room.Id));
        }
    }
}
