using UnityEngine;
using UnityEngine.Serialization;
using BBUnity.Magnitudes;
using BBUnity.Statistics;

namespace BBUnity.Combat {

    /// <summary>
    /// One "damage this statistic, using this damage type" entry on an Attack — pick the target
    /// statistic, then a DamageValue (Static Damage / Static Range Damage / Statistic Damage /
    /// Statistic Percentage Damage), then whatever values that type needs. An Attack can carry any
    /// number of these, so a single attack can damage Health, Poise, or any future statistic, all
    /// from the same list rather than a fixed field per known statistic.
    /// </summary>
    [System.Serializable]
    public class DamageEntry {

        [SerializeField]
        [FormerlySerializedAs("_targetAttribute")]
        private StatisticTypeReference _targetStatistic;

        [SerializeReference]
        private DamageValue _damageValue;

        public System.Type TargetStatisticType {
            get { return _targetStatistic?.ResolveType(); }
        }

        public int ResolveAmount(StatisticController attacker) {
            return _damageValue?.Resolve(attacker) ?? 0;
        }
    }
}
