using UnityEngine;
using BBUnity.Statistics.Generic;


namespace BBUnity.Statistics.Effectors {

    /// <summary>
    /// Changes the statistic's current value by Amount every Interval milliseconds — mana regenerating
    /// (positive Amount), or poison/bleed draining health (negative Amount). A "tick" is simply a
    /// short Interval; "per second" is an Interval of 1000.
    ///
    /// DelayAfterChange pauses it for that long after the statistic was last reduced by anything
    /// other than an effector (spending mana, taking damage), so regeneration doesn't start
    /// refilling while it's still being drained. The first point is gained one full Interval
    /// after the delay ends.
    /// </summary>
    [System.Serializable]
    public class RegenerationEffector : StatisticEffector {

        // Floor for Interval, so a zero or negative value can't turn into an unbounded loop.
        private const int MinimumInterval = 10;

        [SerializeField, Tooltip("Added to the current value each interval. Negative drains instead.")]
        private int _amount = 1;

        [SerializeField, Tooltip("Milliseconds between each application of Amount.")]
        private int _interval = 1000;

        [SerializeField, Tooltip("Milliseconds after the statistic is last reduced (spent or damaged) before this resumes.")]
        private int _delayAfterChange = 0;

        [System.NonSerialized]
        private float _timer;

        public int Amount { get { return _amount; } }
        public int Interval { get { return _interval; } }
        public int DelayAfterChange { get { return _delayAfterChange; } }

        public RegenerationEffector() { }

        public RegenerationEffector(int amount, int interval, int delayAfterChange = 0) {
            _amount = amount;
            _interval = interval;
            _delayAfterChange = delayAfterChange;
        }

        public override void Tick(DynamicIntegerStatistic statistic, float deltaTime) {
            if (_amount == 0) { return; }

            bool nothingToDo = _amount > 0 ? statistic.Current >= statistic.Maximum : statistic.Current <= statistic.Minimum;
            if (nothingToDo || statistic.MillisecondsSinceDecrease < _delayAfterChange) {
                _timer = 0f;
                return;
            }

            int interval = Mathf.Max(_interval, MinimumInterval);

            _timer += deltaTime * 1000f;
            while (_timer >= interval) {
                _timer -= interval;
                statistic.ApplyEffectorDelta(_amount);
            }
        }
    }
}
