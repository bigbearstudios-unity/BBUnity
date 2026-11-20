using UnityEngine;
using UnityEngine.Serialization;

namespace BBUnity.Gameplay.Entities.Controllers.Combat.Reductions {

    /// <summary>
    /// Reduces incoming damage by a percentage read from the defender's chosen statistic — the
    /// statistic's raw current value is treated as a whole percentage (e.g. Resistance 25 means
    /// 25%), not a 0..1 fraction, since every statistic today is a plain int.
    /// </summary>
    [System.Serializable]
    public class PercentageReduction : Reduction {

        [SerializeField]
        [FormerlySerializedAs("_attribute")]
        private StatisticTypeReference _statistic;

        public override ReductionKind Kind {
            get { return ReductionKind.Percentage; }
        }

        public override float Resolve(StatisticController defender) {
            int? value = _statistic?.Resolve(defender)?.NumericValue;
            return value == null ? 0f : value.Value / 100f;
        }
    }
}
