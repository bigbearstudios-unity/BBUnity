using UnityEngine;

using System.Collections.Generic;

using BBUnity.Gameplay.Statistics.Base;
using BBUnity.Gameplay.Statistics.Effectors;

namespace BBUnity.Gameplay.Statistics.Generic {

    /// <summary>
    /// An integer value that lives between a Minimum and a Maximum and can be acted on over time
    /// by effectors (see StatisticEffector).
    ///
    /// There are two maximums: BaseMaximum is the authored value, and Maximum is that after every
    /// maximum-modifying effector has been applied — Maximum is the one the value is clamped to
    /// and the one a HUD should show. Current never leaves [Minimum, Maximum].
    ///
    /// Changed fires whenever Current or Maximum changes.
    /// </summary>
    [System.Serializable]
    public class DynamicIntegerStatistic : DynamicStatistic {

        public delegate void ChangedEventHandler(int current, int minimum, int maximum);

        public enum OnResetBehavior {
            ResetToMaximum = 0,
            ResetToMinimum = 100,
            ResetToZero = 200
        }

        // The authored (base) maximum. Kept under its original serialized name so existing
        // prefabs and scenes keep their values.
        [SerializeField]
        protected int _maximum = 0;

        [SerializeField]
        protected int _minimum = 0;

        [SerializeField]
        private OnResetBehavior _onResetBehavior;

        [SerializeField, EditorAttributes.ReadOnly]
        protected int _current = 0;

        [SerializeField, Tooltip("Always-on effects: regeneration, maximum modifiers, ...")]
        private StatisticEffectorList _effectors = new StatisticEffectorList();

        // Effectors added at runtime (buffs, debuffs) — never serialized.
        private readonly List<StatisticEffector> _runtimeEffectors = new List<StatisticEffector>();

        // Cached result of applying every effector's MaximumBonus to _maximum; resolved lazily so
        // it is correct however the statistic came to exist (deserialized, new, in a test).
        private int _effectiveMaximum;
        private bool _maximumResolved;

        private float _millisecondsSinceDecrease = float.PositiveInfinity;

        public int Minimum { get { return _minimum; } }
        public int BaseMaximum { get { return _maximum; } }
        public int Current { get { return _current; } }
        public bool IsZero { get { return _current == 0; } }

        /// <summary>
        /// The maximum after effectors — what Current is clamped to.
        /// </summary>
        public int Maximum {
            get {
                if (!_maximumResolved) {
                    _effectiveMaximum = ComputeMaximum();
                    _maximumResolved = true;
                }

                return _effectiveMaximum;
            }
        }

        public bool IsFull { get { return _current >= Maximum; } }

        /// <summary>
        /// Seconds since Current was last reduced by anything other than an effector (spending,
        /// damage). Infinite until that first happens. RegenerationEffector uses it for its delay.
        /// </summary>
        public float MillisecondsSinceDecrease { get { return _millisecondsSinceDecrease; } }

        public event ChangedEventHandler Changed;

        public override void Reset() {
            if(_onResetBehavior == OnResetBehavior.ResetToMaximum) {
                SetCurrent(Maximum); return;
            }

            if(_onResetBehavior == OnResetBehavior.ResetToZero) {
                SetCurrent(0); return;
            }

            if(_onResetBehavior == OnResetBehavior.ResetToMinimum) {
                SetCurrent(_minimum); return;
            }
        }

        public virtual void IncreaseBy(int amount) {
            SetClamped(_current + amount);
        }

        public virtual void DecreaseBy(int amount) {
            if (amount > 0) { _millisecondsSinceDecrease = 0f; }

            SetClamped(_current - amount);
        }

        public virtual void SetCurrent(int value) {
            if (value < _current) { _millisecondsSinceDecrease = 0f; }

            SetClamped(value);
        }

        // Default "take damage" behavior, used generically by StatisticCombatController.
        // Overridden by a subclass whose damage has extra side effects — see Poise.TakeDamage.
        public virtual void TakeDamage(int amount) {
            DecreaseBy(amount);
        }

        /// <summary>
        /// True if `amount` could be spent without dropping below Minimum.
        /// </summary>
        public bool CanAfford(int amount) {
            return amount >= 0 && _current - amount >= _minimum;
        }

        /// <summary>
        /// Spends `amount` if it can be afforded; otherwise changes nothing and returns false —
        /// never a partial spend.
        /// </summary>
        public bool TrySpend(int amount) {
            if (!CanAfford(amount)) { return false; }

            DecreaseBy(amount);
            return true;
        }

        /// <summary>
        /// Changes the authored maximum at runtime (a level-up, say). Current is clamped if it
        /// now exceeds the new effective maximum.
        /// </summary>
        public void SetBaseMaximum(int value) {
            int previous = Maximum;

            _maximum = value;
            RefreshMaximum(previous);
        }

        /// <summary>
        /// Moves Current by `delta` on behalf of an effector. Unlike DecreaseBy it does not count
        /// as "the statistic was just reduced", so a draining effect doesn't keep restarting a
        /// regeneration delay, and it only notifies if the value actually changed.
        /// </summary>
        public void ApplyEffectorDelta(int delta) {
            int target = Mathf.Clamp(_current + delta, _minimum, Maximum);
            if (target == _current) { return; }

            _current = target;
            Notify();
        }

        /// <summary>
        /// Adds a runtime effector. `duration` is in game milliseconds; zero or less means it lasts
        /// until removed. `source` tags it for RemoveEffectorsFrom. Returns the effector, which
        /// is also the handle for RemoveEffector.
        /// </summary>
        public StatisticEffector AddEffector(StatisticEffector effector, int duration = 0, object source = null) {
            if (effector == null || _runtimeEffectors.Contains(effector)) { return effector; }

            int previous = Maximum;

            effector.IsTimed = duration > 0;
            effector.RemainingDuration = duration;
            effector.Source = source;
            _runtimeEffectors.Add(effector);

            RefreshMaximum(previous);
            return effector;
        }

        public bool RemoveEffector(StatisticEffector effector) {
            int previous = Maximum;

            if (!_runtimeEffectors.Remove(effector)) { return false; }

            RefreshMaximum(previous);
            return true;
        }

        /// <summary>
        /// Removes every runtime effector added with this source. Returns how many were removed.
        /// </summary>
        public int RemoveEffectorsFrom(object source) {
            int previous = Maximum;
            int removed = _runtimeEffectors.RemoveAll(e => ReferenceEquals(e.Source, source));

            if (removed > 0) { RefreshMaximum(previous); }
            return removed;
        }

        public override void Update(float deltaTime) {
            _millisecondsSinceDecrease += deltaTime * 1000f;

            for (int i = 0; i < _effectors.Count; i++) {
                _effectors[i].Tick(this, deltaTime);
            }

            if (_runtimeEffectors.Count == 0) { return; }

            int previous = Maximum;
            bool expired = false;

            for (int i = _runtimeEffectors.Count - 1; i >= 0; i--) {
                StatisticEffector effector = _runtimeEffectors[i];
                effector.Tick(this, deltaTime);

                if (!effector.IsTimed) { continue; }

                effector.RemainingDuration -= deltaTime * 1000f;
                if (effector.RemainingDuration <= 0f) {
                    _runtimeEffectors.RemoveAt(i);
                    expired = true;
                }
            }

            if (expired) { RefreshMaximum(previous); }
        }

        private int ComputeMaximum() {
            int bonus = 0;

            for (int i = 0; i < _effectors.Count; i++) {
                bonus += _effectors[i].MaximumBonus(_maximum);
            }

            for (int i = 0; i < _runtimeEffectors.Count; i++) {
                bonus += _runtimeEffectors[i].MaximumBonus(_maximum);
            }

            return Mathf.Max(_minimum, _maximum + bonus);
        }

        private void RefreshMaximum(int previous) {
            _effectiveMaximum = ComputeMaximum();
            _maximumResolved = true;

            if (_effectiveMaximum == previous) { return; }

            if (_current > _effectiveMaximum) { _current = _effectiveMaximum; }
            Notify();
        }

        private void SetClamped(int value) {
            _current = Mathf.Clamp(value, _minimum, Maximum);
            Notify();
        }

        private void Notify() {
            Changed?.Invoke(_current, _minimum, Maximum);
        }
    }
}
