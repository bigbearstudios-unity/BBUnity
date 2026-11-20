using System;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Pipeline;

namespace BBUnity.Generation.Layout {

    /// <summary>
    /// Base for layout algorithms that shape the whole grid at once instead of placing rooms —
    /// a cellular-automata cave, a maze, a tower. They carve the main layer directly and are
    /// normally followed by a RoomDetectionStep, which turns the result into rooms so the steps
    /// after it (population, camera plans, validation) work the same as for a room layout.
    /// </summary>
    [Serializable]
    public abstract class LayoutStep : GenerationStep {

        protected static GridLayer MainLayer(GenerationContext context) {
            return context.Result.Grid.Main;
        }
    }
}
