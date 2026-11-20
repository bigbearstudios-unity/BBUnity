using UnityEngine;
using BBUnity.Statistics;

namespace BBUnity.Magnitudes {

    /// <summary>
    /// Rolls a random value between two flat numbers.
    /// </summary>
    [System.Serializable, System.ComponentModel.DisplayName("Static Range Damage")]
    public class StaticRangeDamageValue : DamageValue {

        [SerializeField]
        private int _minimum;

        [SerializeField]
        private int _maximum;

        public override int Resolve(StatisticController attacker) {
            return _minimum >= _maximum ? _minimum : Random.Range(_minimum, _maximum + 1);
        }
    }
}
