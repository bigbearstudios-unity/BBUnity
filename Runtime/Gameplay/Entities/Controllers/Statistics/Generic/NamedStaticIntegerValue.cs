using UnityEngine;
using UnityEngine.Serialization;

using BBUnity.Gameplay.Statistics.Generic;

namespace BBUnity.Gameplay.Entities.Controllers.Statistics.Generic {

    /// <summary>
    /// A generic stat the designer names in the Inspector ("Strength", "Dexterity", ...) and
    /// reads back with StatisticController.Get("Strength"). Any number can be added to one
    /// controller. Has no effectors — see NamedDynamicIntegerValue for one that does.
    /// </summary>
    [System.Serializable]
    [System.ComponentModel.DisplayName("Static Integer Statistic")]
    public class NamedStaticIntegerValue : EntityStatistic {

        [SerializeField]
        [FormerlySerializedAs("_attribute")]
        private StaticIntegerStatistic _statistic = new StaticIntegerStatistic();

        public StaticIntegerStatistic Value {
            get { return _statistic; }
        }

        public override bool AllowsMultiple {
            get { return true; }
        }

        public override object BoxedValue {
            get { return _statistic; }
        }

        public override int? NumericValue {
            get { return _statistic.Current; }
        }

        public override int? MinimumValue {
            get { return _statistic.Minimum; }
        }

        public override int? MaximumValue {
            get { return _statistic.Maximum; }
        }

        public override bool CanAfford(int amount) {
            return amount >= 0 && _statistic.Current - amount >= _statistic.Minimum;
        }

        public override bool TrySpend(int amount) {
            if (!CanAfford(amount)) { return false; }

            _statistic.SetCurrent(_statistic.Current - amount);
            return true;
        }

        public override void TakeDamage(int amount) {
            _statistic.SetCurrent(_statistic.Current - amount);
        }

        public override void Start() {
            _statistic.Changed += RaiseValueChanged;
            _statistic.Reset();
        }
    }
}
