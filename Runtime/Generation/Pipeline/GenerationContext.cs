using UnityEngine;

using BBUnity.Generation.Chunks;
using BBUnity.Generation.Parameters;

namespace BBUnity.Generation.Pipeline {

    /// <summary>
    /// What a step works with: the result built so far, the template's parameters, and the seeded
    /// random. Steps resolve their Fixed/Range parameters through Resolve so a test run can pin
    /// any of them with a ParameterOverrideSet.
    /// </summary>
    public class GenerationContext {

        public GenerationRandom Random { get; }
        public GenerationParameters Parameters { get; }
        public ParameterOverrideSet Overrides { get; }
        public GenerationResult Result { get; }

        /// <summary>The chunks the template offers (may be null).</summary>
        public ChunkLibraryDefinition ChunkLibrary { get; }

        public GenerationContext(GenerationRandom random, GenerationParameters parameters, ParameterOverrideSet overrides, GenerationResult result, ChunkLibraryDefinition chunkLibrary = null) {
            Random = random;
            Parameters = parameters;
            Overrides = overrides ?? new ParameterOverrideSet();
            Result = result;
            ChunkLibrary = chunkLibrary;
        }

        /// <summary>The parameter's value for this run: the override for `key` if there is one, else drawn from its own stream.</summary>
        public int Resolve(string key, IntParameter parameter) {
            int value = Overrides.TryGetInt(key, out int pinned) ? pinned : parameter.Resolve(Random.Fork("param:" + key));
            Record(key, value, true);
            return value;
        }

        public float Resolve(string key, FloatParameter parameter) {
            float value = Overrides.TryGetFloat(key, out float pinned) ? pinned : parameter.Resolve(Random.Fork("param:" + key));
            Record(key, value, false);
            return value;
        }

        private void Record(string key, float value, bool isInt) {
            Result.Parameters.RemoveAll(existing => existing.Key == key);
            Result.Parameters.Add(new ResolvedParameter(key, value, isInt));
        }

        public void AddIssue(IssueSeverity severity, string message, Vector2Int? cell = null) {
            Result.Issues.Add(new GenerationIssue(severity, message, cell));
        }
    }
}
