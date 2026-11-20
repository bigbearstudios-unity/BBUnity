using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

using BBUnity.Gameplay.Statistics.Generic;

namespace BBUnity.Gameplay.Entities.Controllers.Statistics {

    /// <summary>
    /// The type listed in StatisticController's "Add Statistic" dropdown and stored in its
    /// [SerializeReference] list — mirrors EntityState/StateController. A concrete statistic
    /// (e.g. HealthValue in the Specialized folder) never reimplements statistic logic
    /// itself: it boxes one instance of the corresponding BBUnity.Gameplay.Statistics.Specialized
    /// type via EntityStatistic&lt;TStatistic&gt; below and forwards to it.
    /// </summary>
    [System.Serializable]
    public abstract class EntityStatistic {

        [SerializeField, Tooltip("What StatisticController.Get(name) finds this statistic by. Leave empty to use the type's own name (HealthValue is \"Health\").")]
        private string _name;

        /// <summary>
        /// The name this statistic is looked up by. Falls back to the type name without its
        /// "Value" suffix, so typed statistics need no naming while generic ones (see
        /// NamedStaticIntegerValue) are told apart by whatever the designer typed.
        /// </summary>
        public string Name {
            get { return string.IsNullOrEmpty(_name) ? DefaultName : _name; }
        }

        private string DefaultName {
            get {
                string typeName = GetType().Name;
                const string suffix = "Value";

                return typeName.EndsWith(suffix) && typeName.Length > suffix.Length
                    ? typeName.Substring(0, typeName.Length - suffix.Length)
                    : typeName;
            }
        }

        /// <summary>
        /// True for a generic statistic type that can be added more than once to the same
        /// controller (told apart by Name). Typed statistics (Health, Mana, ...) are one per
        /// controller, since lookup by type would otherwise just find the first.
        /// </summary>
        public virtual bool AllowsMultiple {
            get { return false; }
        }

        /// <summary>
        /// The boxed value object (Health, Mana, ...) or null — what StatisticController.Get&lt;T&gt;()
        /// searches by type.
        /// </summary>
        public virtual object BoxedValue {
            get { return null; }
        }

        /// <summary>
        /// True if `amount` can be spent from this statistic without going below its minimum.
        /// False for an statistic that has no numeric value.
        /// </summary>
        public virtual bool CanAfford(int amount) {
            return false;
        }

        /// <summary>
        /// Spends `amount` if affordable, otherwise changes nothing and returns false.
        /// </summary>
        public virtual bool TrySpend(int amount) {
            return false;
        }

        // Generic numeric read used by combat data (DamageValue/Reduction picking "an statistic"
        // by type) without needing to know the concrete statistic type — null for a wrapper whose
        // boxed type doesn't expose one. See EntityStatistic&lt;TStatistic&gt; below.
        public virtual int? NumericValue {
            get { return null; }
        }

        // Range counterparts to NumericValue — null for the same reason, and used by anything
        // displaying the statistic generically (see StatisticBar).
        public virtual int? MinimumValue {
            get { return null; }
        }

        public virtual int? MaximumValue {
            get { return null; }
        }

        /// <summary>
        /// Fired with (current, minimum, maximum) whenever the boxed statistic's value changes —
        /// including its reset on Start(). Lets a view that can't hold a direct reference to this
        /// statistic (a HUD in another scene, bound to a prefab-spawned Player) subscribe by type
        /// at runtime, rather than needing a serialized Slider on the statistic itself.
        /// </summary>
        public event System.Action<int, int, int> ValueChanged;

        protected void RaiseValueChanged(int current, int minimum, int maximum) {
            ValueChanged?.Invoke(current, minimum, maximum);
        }

        // Generic damage application, used by StatisticCombatController.ApplyHit() without
        // needing to know the concrete statistic type — no-ops by default for a wrapper whose
        // boxed type doesn't support it. See EntityStatistic&lt;TStatistic&gt; below.
        public virtual void TakeDamage(int amount) { }

        public virtual void Start() { }

        public virtual void Tick(float deltaTime) { }
    }

    /// <summary>
    /// Boxes a single DynamicIntegerStatistic instance (Health, Mana, Poise, ...) instead of
    /// duplicating its logic, and wires up an optional Slider: its max value is set from the
    /// statistic on Start, and its value is kept in sync via the statistic's Changed event.
    /// </summary>
    public abstract class EntityStatistic<TStatistic> : EntityStatistic where TStatistic : DynamicIntegerStatistic, new() {

        [SerializeField]
        [FormerlySerializedAs("_attribute")]
        protected TStatistic _statistic = new TStatistic();

        [SerializeField, Tooltip("Optional — kept in sync with this statistic's value once Start() runs.")]
        private Slider _slider;

        public TStatistic Value {
            get { return _statistic; }
        }

        public override int? NumericValue {
            get { return _statistic.Current; }
        }

        public override object BoxedValue {
            get { return _statistic; }
        }

        public override bool CanAfford(int amount) {
            return _statistic.CanAfford(amount);
        }

        public override bool TrySpend(int amount) {
            return _statistic.TrySpend(amount);
        }

        public override void TakeDamage(int amount) {
            _statistic.TakeDamage(amount);
        }

        public override int? MinimumValue {
            get { return _statistic.Minimum; }
        }

        public override int? MaximumValue {
            get { return _statistic.Maximum; }
        }

        public override void Start() {
            // Forwarded before Reset() so the initial reset-to-maximum reaches subscribers too.
            _statistic.Changed += RaiseValueChanged;
            _statistic.Reset();

            if (_slider != null) {
                _slider.maxValue = _statistic.Maximum;
                _slider.value = _statistic.Current;
                _statistic.Changed += HandleStatisticChange;
            }
        }

        public override void Tick(float deltaTime) {
            _statistic.Update(deltaTime);
        }

        private void HandleStatisticChange(int current, int minimum, int maximum) {
            _slider.maxValue = maximum;
            _slider.value = current;
        }
    }
}
