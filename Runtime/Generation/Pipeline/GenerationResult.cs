using System.Collections.Generic;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Placement;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Pipeline {

    /// <summary>A parameter as it was resolved for one run — what editors show so a value can be pinned.</summary>
    public class ResolvedParameter {
        public string Key { get; }
        public float Value { get; }
        public bool IsInt { get; }

        public ResolvedParameter(string key, float value, bool isInt) {
            Key = key;
            Value = value;
            IsInt = isInt;
        }
    }

    /// <summary>
    /// Everything a generation run produces: the grid, the rooms and how they connect, what was
    /// placed where, and any issues raised. This — not tiles or GameObjects — is the generated
    /// level; the schematic preview, the tile painter and the element spawner all read from it.
    /// </summary>
    public class GenerationResult {

        public int Seed { get; set; }
        public GenerationGrid Grid { get; set; }
        public RoomGraph Rooms { get; } = new RoomGraph();
        public List<PlacementRecord> Placements { get; } = new List<PlacementRecord>();
        public List<GenerationIssue> Issues { get; } = new List<GenerationIssue>();
        public List<ResolvedParameter> Parameters { get; } = new List<ResolvedParameter>();

        public bool HasErrors {
            get {
                foreach (GenerationIssue issue in Issues) {
                    if (issue.Severity == IssueSeverity.Error) { return true; }
                }

                return false;
            }
        }
    }
}
