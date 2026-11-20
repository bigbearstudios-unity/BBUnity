using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Placement {

    /// <summary>
    /// Fills a laid-out level: for each budget, picks rooms the placeable allows and places the
    /// requested number of them where they fit (see PlacementCandidateFinder), never overlapping
    /// anything already placed. A shortfall is reported as a warning, not an error. Run it after
    /// the steps that create rooms and carve the grid.
    /// </summary>
    [Serializable]
    public class PopulationStep : GenerationStep {

        [SerializeField]
        private List<ElementBudget> _budgets = new List<ElementBudget>();

        public PopulationStep() { }

        public PopulationStep(IEnumerable<ElementBudget> budgets) {
            _budgets = new List<ElementBudget>(budgets);
        }

        public IReadOnlyList<ElementBudget> Budgets {
            get { return _budgets; }
        }

        public override string DisplayName {
            get { return "Populate"; }
        }

        public override void Execute(GenerationContext context) {
            GenerationResult result = context.Result;
            var occupied = new List<RectInt>();
            foreach (PlacementRecord existing in result.Placements) { occupied.Add(existing.Footprint); }

            foreach (ElementBudget budget in _budgets) {
                PlaceableDefinition placeable = budget?.Placeable;
                if (placeable == null) { continue; }

                int wanted = context.Resolve("population." + placeable.name, budget.Count);
                GenerationRandom random = context.Random.Fork("place:" + placeable.name);

                var rooms = new List<GridRoom>();
                foreach (GridRoom room in result.Rooms.Rooms) {
                    if (placeable.AllowsRoom(room)) { rooms.Add(room); }
                }

                int placed = 0;
                for (int i = 0; i < wanted; i++) {
                    if (!TryPlaceOne(context, placeable, rooms, random, occupied)) { break; }
                    placed++;
                }

                if (placed < wanted) {
                    context.AddIssue(IssueSeverity.Warning, $"Only placed {placed} of {wanted} '{placeable.DisplayName}' — there was no room left that fits.");
                }
            }
        }

        private static bool TryPlaceOne(GenerationContext context, PlaceableDefinition placeable, List<GridRoom> rooms, GenerationRandom random, List<RectInt> occupied) {
            var order = new List<GridRoom>(rooms);
            random.Shuffle(order);

            foreach (GridRoom room in order) {
                List<Vector2Int> candidates = PlacementCandidateFinder.Find(context.Result.Grid.Main, room, placeable, occupied);
                if (candidates.Count == 0) { continue; }

                var record = new PlacementRecord(placeable, random.Pick(candidates), room.Id);
                context.Result.Placements.Add(record);
                occupied.Add(record.Footprint);
                return true;
            }

            return false;
        }
    }
}
