using UnityEngine;

using BBUnity.Gameplay.Statistics.Base;

namespace BBUnity.Gameplay.Statistics.Generic {

    /// <summary>
    /// A plain named stat (Strength, Dexterity, ...) — an integer between a Minimum and a Maximum
    /// that only changes when game code sets it. Deliberately has no effectors and no
    /// regeneration; reach for DynamicIntegerStatistic when it needs either.
    /// </summary>
    [System.Serializable]
    public class StaticIntegerStatistic : StaticStatistic {

        public delegate void ChangedEventHandler(int current, int minimum, int maximum);

        [SerializeField]
        private int _minimum = 0;

        [SerializeField]
        private int _maximum = 100;

        [SerializeField]
        private int _current = 0;

        public int Minimum { get { return _minimum; } }
        public int Maximum { get { return _maximum; } }
        public int Current { get { return _current; } }

        public event ChangedEventHandler Changed;

        public void SetCurrent(int value) {
            _current = Mathf.Clamp(value, _minimum, _maximum);

            Changed?.Invoke(_current, _minimum, _maximum);
        }

        // Re-applies the authored current value through the clamp, so a Current outside
        // [Minimum, Maximum] in the Inspector is corrected on start.
        public override void Reset() {
            SetCurrent(_current);
        }
    }
}
