using System;
using System.Collections.Generic;

namespace BBUnity.Generation.Pipeline {

    /// <summary>Runs generation steps in order. A step that throws is reported as an error and the run stops there.</summary>
    public static class GenerationPipeline {

        public static void Run(IEnumerable<GenerationStep> steps, GenerationContext context) {
            foreach (GenerationStep step in steps) {
                if (step == null || !step.Enabled) { continue; }

                try {
                    step.Execute(context);
                } catch (Exception exception) {
                    context.AddIssue(IssueSeverity.Error, $"{step.DisplayName} failed: {exception.Message}");
                    UnityEngine.Debug.LogException(exception);
                    return;
                }
            }
        }
    }
}
