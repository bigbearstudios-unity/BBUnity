using System;

using BBUnity.Generation.Grid;

namespace BBUnity.Generation.Pipeline {

    /// <summary>
    /// Base for the shared settings of one family of generators (the platformer's room count, room
    /// sizes, corridor widths...). A template holds one of these and the steps that understand it
    /// read their settings from it; generic steps don't need it.
    /// </summary>
    [Serializable]
    public abstract class GenerationParameters {

        /// <summary>
        /// The cell type steps fill a level with when they have none of their own, so a family's
        /// template only has to name its solid once.
        /// </summary>
        public virtual CellTypeDefinition DefaultSolid {
            get { return null; }
        }
    }
}
