using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Layout {

    /// <summary>
    /// Chooses which pairs of rooms get a passage: a minimum spanning tree over the room centres
    /// (so every room is reachable with the shortest total corridor) plus a few extra links
    /// between nearby rooms so the level has loops instead of being one long chain.
    /// </summary>
    public static class ConnectionPlanner {

        public static List<(GridRoom a, GridRoom b)> Plan(IReadOnlyList<GridRoom> rooms, int extraLinks, GenerationRandom random) {
            var pairs = new List<(GridRoom, GridRoom)>();
            if (rooms.Count < 2) { return pairs; }

            var connected = new List<GridRoom> { rooms[0] };
            var remaining = new List<GridRoom>(rooms);
            remaining.RemoveAt(0);

            while (remaining.Count > 0) {
                GridRoom bestFrom = null, bestTo = null;
                float bestDistance = float.MaxValue;

                foreach (GridRoom inside in connected) {
                    foreach (GridRoom outside in remaining) {
                        float distance = Vector2Int.Distance(inside.Center, outside.Center);
                        if (distance < bestDistance) { bestDistance = distance; bestFrom = inside; bestTo = outside; }
                    }
                }

                pairs.Add((bestFrom, bestTo));
                connected.Add(bestTo);
                remaining.Remove(bestTo);
            }

            AddExtraLinks(rooms, pairs, extraLinks, random);
            return pairs;
        }

        private static void AddExtraLinks(IReadOnlyList<GridRoom> rooms, List<(GridRoom a, GridRoom b)> pairs, int extraLinks, GenerationRandom random) {
            var candidates = new List<(GridRoom a, GridRoom b, float distance)>();

            for (int i = 0; i < rooms.Count; i++) {
                for (int j = i + 1; j < rooms.Count; j++) {
                    if (HasPair(pairs, rooms[i], rooms[j])) { continue; }
                    candidates.Add((rooms[i], rooms[j], Vector2Int.Distance(rooms[i].Center, rooms[j].Center)));
                }
            }

            candidates.Sort((x, y) => x.distance.CompareTo(y.distance));

            // Pick from the nearest handful so loops are short, but not always the same ones.
            int pool = Mathf.Min(candidates.Count, Mathf.Max(extraLinks * 2, 1));
            var nearest = candidates.GetRange(0, pool);
            random.Shuffle(nearest);

            for (int i = 0; i < Mathf.Min(extraLinks, nearest.Count); i++) { pairs.Add((nearest[i].a, nearest[i].b)); }
        }

        private static bool HasPair(List<(GridRoom a, GridRoom b)> pairs, GridRoom x, GridRoom y) {
            foreach ((GridRoom a, GridRoom b) in pairs) {
                if ((a == x && b == y) || (a == y && b == x)) { return true; }
            }

            return false;
        }
    }
}
