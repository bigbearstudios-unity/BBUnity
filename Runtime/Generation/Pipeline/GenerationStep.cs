using System;

using UnityEngine;

namespace BBUnity.Generation.Pipeline {

    /// <summary>
    /// One stage of a generation pipeline — laying out rooms, carving corridors, adding
    /// platforms, placing enemies. Steps are plain serializable classes held by a template
    /// (so each carries its own settings) and run in order, each reading and adding to the
    /// shared GenerationContext.
    /// </summary>
    [Serializable]
    public abstract class GenerationStep {

        [SerializeField]
        private bool _enabled = true;

        public bool Enabled {
            get { return _enabled; }
        }

        public virtual string DisplayName {
            get { return GetType().Name; }
        }

        public abstract void Execute(GenerationContext context);
    }
}
