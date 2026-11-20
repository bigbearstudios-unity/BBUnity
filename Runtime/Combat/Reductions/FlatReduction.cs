using UnityEngine;
using UnityEngine.Serialization;
using BBUnity.Statistics;

namespace BBUnity.Combat.Reductions {

    /// <summary>
    /// Subtracts the defender's raw current value of a chosen statistic — e.g. Defense 15
    /// reducing an incoming hit by 15.
    /// </summary>
    [System.Serializable]
    public class FlatReduction : Reduction {

        [SerializeField]
        [FormerlySerializedAs("_attribute")]
        private StatisticTypeReference _statistic;

        public override ReductionKind Kind {
            get { return ReductionKind.Flat; }
        }

        public override float Resolve(StatisticController defender) {
            return _statistic?.Resolve(defender)?.NumericValue ?? 0;
        }
    }
}
