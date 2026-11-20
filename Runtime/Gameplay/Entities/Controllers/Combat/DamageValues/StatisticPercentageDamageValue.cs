using UnityEngine;
using UnityEngine.Serialization;

namespace BBUnity.Gameplay.Entities.Controllers.Combat.DamageValues {

    /// <summary>
    /// A percentage of the attacker's current value of a chosen statistic — e.g. Strength 100 at
    /// 50% deals 50 damage.
    /// </summary>
    [System.Serializable, System.ComponentModel.DisplayName("Statistic Percentage Damage")]
    public class StatisticPercentageDamageValue : DamageValue {

        [SerializeField]
        [FormerlySerializedAs("_attribute")]
        private StatisticTypeReference _statistic;

        [SerializeField, Range(0f, 1f)]
        private float _percentage = 1f;

        public override int Resolve(StatisticController attacker) {
            int? value = _statistic?.Resolve(attacker)?.NumericValue;
            return value == null ? 0 : Mathf.RoundToInt(value.Value * _percentage);
        }
    }
}
