using BBUnity.Statistics.Generic;

namespace BBUnity.Statistics.Effectors {

    /// <summary>
    /// Something that continuously acts on a DynamicIntegerStatistic — regenerating or draining
    /// its value over time, or raising/lowering its maximum. Effectors are authored on the
    /// statistic in the Inspector (always on) or added at runtime with
    /// DynamicIntegerStatistic.AddEffector (optionally timed, optionally tagged with a source so
    /// everything an item applied can be removed together).
    ///
    /// An effector only ever talks to the statistic it is attached to, so it carries no
    /// knowledge of Health, Mana etc. — a new kind of effect is a new subclass overriding
    /// MaximumBonus and/or Tick.
    /// </summary>
    [System.Serializable]
    public abstract class StatisticEffector {

        // Runtime-only bookkeeping, set by DynamicIntegerStatistic.AddEffector. An authored
        // effector never has a duration, so these stay at their defaults for it.
        [System.NonSerialized]
        internal bool IsTimed;

        [System.NonSerialized]
        internal float RemainingDuration; // milliseconds

        /// <summary>
        /// Whatever was passed to AddEffector as the source, or null.
        /// </summary>
        public object Source { get; internal set; }

        /// <summary>
        /// How much this effector adds to the statistic's maximum (negative to reduce it), given
        /// the statistic's authored base maximum. All effectors' bonuses are summed.
        /// </summary>
        public virtual int MaximumBonus(int baseMaximum) {
            return 0;
        }

        /// <summary>
        /// Called every frame the statistic is updated, in game time (so it follows
        /// Time.timeScale).
        /// </summary>
        public virtual void Tick(BBUnity.Statistics.Generic.DynamicIntegerStatistic statistic, float deltaTime) {

        }
    }
}
