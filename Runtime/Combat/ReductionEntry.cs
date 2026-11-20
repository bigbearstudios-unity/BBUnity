using UnityEngine;
using UnityEngine.Serialization;
using BBUnity.Statistics;

namespace BBUnity.Combat {

    /// <summary>
    /// One "mitigate incoming damage to this statistic, using this reduction" entry on an
    /// StatisticCombatController's Reductions list — pick the target statistic, then a
    /// Reduction (Flat / Percentage). Two entries can legitimately target the same statistic
    /// (e.g. a Flat and a Percentage reduction both applying to Health), and
    /// StatisticCombatController.MitigateDamage() combines whichever entries match the
    /// statistic actually being damaged.
    /// </summary>
    [System.Serializable]
    public class ReductionEntry {

        [SerializeField]
        [FormerlySerializedAs("_targetAttribute")]
        private StatisticTypeReference _targetStatistic;

        [SerializeReference]
        private Reduction _reduction;

        public System.Type TargetStatisticType {
            get { return _targetStatistic?.ResolveType(); }
        }

        public Reduction Reduction {
            get { return _reduction; }
        }
    }
}
