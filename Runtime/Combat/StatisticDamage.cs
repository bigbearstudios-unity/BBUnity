using UnityEngine;
using System.Collections.Generic;

namespace BBUnity.Combat {

    /// <summary>
    /// One raw, pre-mitigation damage amount targeting a specific statistic type — the type
    /// travels with the amount since it's resolved by the attacker but applied by the defender,
    /// two different StatisticControllers.
    /// </summary>
    public readonly struct StatisticDamage {

        public readonly System.Type StatisticType;
        public readonly int Amount;

        public StatisticDamage(System.Type statisticType, int amount) {
            StatisticType = statisticType;
            Amount = amount;
        }
    }
}
