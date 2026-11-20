using BBUnity.Statistics;

namespace BBUnity.Combat {

    /// <summary>
    /// The type picked for a ReductionEntry.Reduction — unlike EntityStatistic/EntityState,
    /// duplicates of the same concrete type are allowed across an StatisticCombatController's
    /// Reductions list (e.g. two ReductionEntry entries, both FlatReduction, targeting the same
    /// statistic from two different resistance statistics), so there's no one-per-type constraint
    /// here.
    /// </summary>
    [System.Serializable]
    public abstract class Reduction {

        public abstract ReductionKind Kind { get; }

        /// <summary>
        /// Flat: the raw amount to subtract. Percentage: a 0..1 fraction of incoming damage to
        /// remove.
        /// </summary>
        public abstract float Resolve(StatisticController defender);
    }
}
