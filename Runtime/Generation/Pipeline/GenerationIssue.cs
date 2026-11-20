using UnityEngine;

namespace BBUnity.Generation.Pipeline {

    public enum IssueSeverity {
        Info,
        Warning,
        Error
    }

    /// <summary>A problem or note raised during generation or validation, optionally pinned to a cell.</summary>
    public class GenerationIssue {

        public IssueSeverity Severity { get; }
        public string Message { get; }
        public Vector2Int? Cell { get; }

        public GenerationIssue(IssueSeverity severity, string message, Vector2Int? cell = null) {
            Severity = severity;
            Message = message;
            Cell = cell;
        }
    }
}
