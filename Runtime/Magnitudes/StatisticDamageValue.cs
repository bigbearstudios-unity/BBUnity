using UnityEngine;
using UnityEngine.Serialization;
using BBUnity.Statistics.Base;
using BBUnity.Statistics;

namespace BBUnity.Magnitudes {

    /// <summary>
    /// The attacker's raw current value of a chosen statistic — e.g. "Strength" dealing damage
    /// equal to its current value.
    /// </summary>
    [System.Serializable, System.ComponentModel.DisplayName("Statistic Damage")]
    public class StatisticDamageValue : DamageValue {

        [SerializeField]
        [FormerlySerializedAs("_attribute")]
        private StatisticTypeReference _statistic;

        public override int Resolve(StatisticController attacker) {
            return _statistic?.Resolve(attacker)?.NumericValue ?? 0;
        }
    }
}
