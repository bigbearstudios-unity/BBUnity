using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Placement;
using BBUnity.Generation.Platformer.Reachability;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Platformer.Steps {

    /// <summary>
    /// Makes every room reachable. Starting from the player spawn, it works out where the player
    /// can get to; for the first room it can't reach, it adds one-way stepping-stone platforms
    /// at the tops of the jumps the player can already make, each one nearer the room, until it
    /// can. Whatever it still can't fix is reported (the validation step reports it again at the
    /// end, after population). Run it after the start/exit assignment and the layout.
    /// </summary>
    [Serializable]
    public class PlatformerPlatformStep : GenerationStep {

        [SerializeField, Min(1), Tooltip("The most platforms added to one level.")]
        private int _maxPlatforms = 80;

        public override string DisplayName {
            get { return "Add Platforms"; }
        }

        public override void Execute(GenerationContext context) {
            PlatformerGenerationParameters parameters = PlatformerGenerationParameters.From(context, DisplayName);
            if (parameters == null) { return; }

            if (parameters.Platform == null) {
                context.AddIssue(IssueSeverity.Error, "Add Platforms needs a Platform cell type in the template's parameters.");
                return;
            }

            GridLayer layer = context.Result.Grid.Main;
            var reachability = new PlatformerReachability(layer, parameters.Movement.Resolve());

            Vector2Int? start = FindStart(context, parameters, reachability);
            if (start == null) {
                context.AddIssue(IssueSeverity.Error, "Add Platforms found nowhere for the player to start.");
                return;
            }

            int added = 0;
            foreach (GridRoom room in RoomsInOrder(context.Result.Rooms, context.Result.Rooms.GetRoom(RoomIdOf(context, start.Value)))) {
                HashSet<Vector2Int> reached = reachability.ReachableFrom(new[] { start.Value });

                while (!PlatformerReachability.AnyIn(reached, room.Bounds)) {
                    if (added >= _maxPlatforms || !AddPlatformToward(layer, reachability, parameters, start.Value, room, ref reached)) { break; }
                    added++;
                }

                if (!PlatformerReachability.AnyIn(reached, room.Bounds)) {
                    context.AddIssue(IssueSeverity.Warning, $"Room {room.Id} could not be made reachable.", room.Center);
                }
            }
        }

        private static Vector2Int? FindStart(GenerationContext context, PlatformerGenerationParameters parameters, PlatformerReachability reachability) {
            foreach (PlacementRecord placement in context.Result.Placements) {
                if (placement.Placeable != parameters.PlayerSpawn) { continue; }

                if (reachability.IsStandable(placement.Cell)) { return placement.Cell; }

                GridRoom room = context.Result.Rooms.GetRoom(placement.RoomId);
                return room != null ? reachability.FindStandableNear(room.Bounds) : null;
            }

            GridRoom first = context.Result.Rooms.Rooms.Count > 0 ? context.Result.Rooms.Rooms[0] : null;
            return first != null ? reachability.FindStandableNear(first.Bounds) : null;
        }

        private static int RoomIdOf(GenerationContext context, Vector2Int cell) {
            GridRoom room = context.Result.Rooms.FindRoomAt(cell);
            return room != null ? room.Id : 0;
        }

        // Breadth-first through the room graph from the start room, so rooms nearest the start
        // are fixed first and each repair builds on the last.
        private static List<GridRoom> RoomsInOrder(RoomGraph graph, GridRoom startRoom) {
            var order = new List<GridRoom>();
            if (startRoom == null) { return order; }

            var seen = new HashSet<int> { startRoom.Id };
            var queue = new Queue<GridRoom>();
            queue.Enqueue(startRoom);

            while (queue.Count > 0) {
                GridRoom room = queue.Dequeue();
                order.Add(room);

                foreach (GridRoom next in graph.Neighbours(room)) {
                    if (seen.Add(next.Id)) { queue.Enqueue(next); }
                }
            }

            return order;
        }

        // Adds the platform at the top of a known jump that ends up closest to the room. If it
        // doesn't get any closer it is taken back out, so the grid is left as it was.
        private static bool AddPlatformToward(GridLayer layer, PlatformerReachability reachability, PlatformerGenerationParameters parameters, Vector2Int start, GridRoom room, ref HashSet<Vector2Int> reached) {
            var apexes = new List<Vector2>();
            reachability.ReachableFrom(new[] { start }, apexes);

            Vector2 target = room.Center;
            float currentBest = DistanceToNearest(reached, target);

            var candidates = new List<(Vector2Int standing, float distance)>();
            var seen = new HashSet<Vector2Int>();
            foreach (Vector2 apex in apexes) {
                var standing = new Vector2Int(Mathf.FloorToInt(apex.x), Mathf.FloorToInt(apex.y));
                if (!seen.Add(standing)) { continue; }

                candidates.Add((standing, Vector2.Distance(new Vector2(standing.x + 0.5f, standing.y), target)));
            }

            candidates.Sort((a, b) => a.distance.CompareTo(b.distance));

            int tries = 0;
            foreach ((Vector2Int standing, float distance) in candidates) {
                if (distance >= currentBest - 0.01f || tries++ >= 6) { break; }

                List<(Vector2Int cell, CellTypeDefinition previous)> changes = PlacePlatform(layer, parameters, standing);
                if (changes.Count == 0) { continue; }

                HashSet<Vector2Int> after = reachability.ReachableFrom(new[] { start });
                if (DistanceToNearest(after, target) < currentBest - 0.01f) {
                    reached = after;
                    return true;
                }

                foreach ((Vector2Int cell, CellTypeDefinition previous) in changes) { layer.Set(cell, previous); }
            }

            return false;
        }

        // A platform whose top is level with the body's feet at `standing`; empty if it cannot go there.
        private static List<(Vector2Int, CellTypeDefinition)> PlacePlatform(GridLayer layer, PlatformerGenerationParameters parameters, Vector2Int standing) {
            var changes = new List<(Vector2Int, CellTypeDefinition)>();
            int row = standing.y - 1;
            int left = standing.x - (parameters.PlatformWidth - 1) / 2;

            for (int x = left; x < left + parameters.PlatformWidth; x++) {
                if (!layer.InBounds(x, row) || layer.BlocksMovement(x, row)) { continue; }
                if (layer.SupportsStanding(x, row)) { continue; }

                changes.Add((new Vector2Int(x, row), layer.Get(x, row)));
            }

            foreach ((Vector2Int cell, CellTypeDefinition _) in changes) { layer.Set(cell, parameters.Platform); }
            return changes;
        }

        private static float DistanceToNearest(HashSet<Vector2Int> cells, Vector2 target) {
            float best = float.MaxValue;
            foreach (Vector2Int cell in cells) {
                best = Mathf.Min(best, Vector2.Distance(new Vector2(cell.x + 0.5f, cell.y), target));
            }

            return best;
        }
    }
}
