using UnityEngine;

namespace BBUnity.Gameplay.Entities.Controllers.Combat.DamageValues {

    [System.Serializable, System.ComponentModel.DisplayName("Static Damage")]
    public class StaticDamageValue : DamageValue {

        [SerializeField]
        private int _amount;

        public override int Resolve(StatisticController attacker) {
            return _amount;
        }
    }
}
