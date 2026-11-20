using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Placement;
using BBUnity.Generation.Platformer.Reachability;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Platformer.Steps {

    /// <summary>
    /// Checks the finished level without changing it: the level exit must be reachable from the
    /// player spawn (an error if not), and unreachable rooms and floor placements are reported as
    /// warnings. Run it last.
    /// </summary>
    [Serializable]
    public class PlatformerValidationStep : GenerationStep {

        public override string DisplayName {
            get { return "Validate Reachability"; }
        }

        public override void Execute(GenerationContext context) {
            PlatformerGenerationParameters parameters = PlatformerGenerationParameters.From(context, DisplayName);
            if (parameters == null) { return; }

            var reachability = new PlatformerReachability(context.Result.Grid.Main, parameters.Movement.Resolve());

            PlacementRecord spawn = Find(context, parameters.PlayerSpawn);
            if (spawn == null) {
                context.AddIssue(IssueSeverity.Error, "The level has no player spawn.");
                return;
            }

            HashSet<Vector2Int> reached = reachability.ReachableFrom(new[] { spawn.Cell });
            if (reached.Count == 0) {
                context.AddIssue(IssueSeverity.Error, "The player spawn is not somewhere the player can stand.", spawn.Cell);
                return;
            }

            PlacementRecord exit = Find(context, parameters.LevelExit);
            if (exit == null) {
                context.AddIssue(IssueSeverity.Error, "The level has no exit.");
            } else if (!IsReached(reached, exit)) {
                context.AddIssue(IssueSeverity.Error, "The level exit cannot be reached from the player spawn.", exit.Cell);
            }

            foreach (GridRoom room in context.Result.Rooms.Rooms) {
                if (!PlatformerReachability.AnyIn(reached, room.Bounds)) {
                    context.AddIssue(IssueSeverity.Warning, $"Room {room.Id} cannot be reached from the player spawn.", room.Center);
                }
            }

            foreach (PlacementRecord placement in context.Result.Placements) {
                if (placement == spawn || placement == exit || placement.Placeable.Surface != PlacementSurface.Floor) { continue; }

                if (!IsReached(reached, placement)) {
                    context.AddIssue(IssueSeverity.Warning, $"'{placement.Placeable.DisplayName}' sits somewhere the player cannot reach.", placement.Cell);
                }
            }
        }

        private static PlacementRecord Find(GenerationContext context, PlaceableDefinition placeable) {
            foreach (PlacementRecord placement in context.Result.Placements) {
                if (placement.Placeable == placeable) { return placement; }
            }

            return null;
        }

        private static bool IsReached(HashSet<Vector2Int> reached, PlacementRecord placement) {
            RectInt footprint = placement.Footprint;

            for (int x = footprint.xMin; x < footprint.xMax; x++) {
                if (reached.Contains(new Vector2Int(x, footprint.yMin))) { return true; }
            }

            return false;
        }
    }
}
