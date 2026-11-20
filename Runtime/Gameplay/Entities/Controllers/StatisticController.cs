
using UnityEngine;
using UnityEngine.Serialization;

using System.Collections.Generic;

using BBUnity.Gameplay.Entities.Controllers.Statistics;

namespace BBUnity.Gameplay.Entities.Controllers {

    /// <summary>
    /// Controller for all statistics. Statistics are added at edit time via the "Add Statistic"
    /// button in the inspector (see StatisticControllerEditor) rather than declared as
    /// individual [SerializeField] fields on a subclass — each entry is an EntityStatistic
    /// boxing one of the BBUnity.Gameplay.Statistics.Specialized types (Health, Mana, Poise, ...).
    ///
    /// See <see cref="BBUnity.Gameplay.Entities.Controllers.InputController"/> for the rationale behind
    /// this execution order value and an important caveat about it not being inherited by
    /// concrete subclasses (e.g. StatisticCombatController).
    /// </summary>
    [AddComponentMenu("BBUnity/Gameplay/Entities/Controllers/Statistic Controller")]
    [DefaultExecutionOrder(10)]
    public class StatisticController : EntityController {

        [SerializeReference]
        [FormerlySerializedAs("_entityAttributes")]
        private List<EntityStatistic> _entityStatistics = new List<EntityStatistic>();

        // Virtual so a subclass that layers more statistics in before anything reads them (see
        // StatisticCombatController's Definition) can do so in Awake, ahead of every other
        // component's Start.
        protected virtual void Awake() { }

        protected virtual void Start() {
            foreach (EntityStatistic statistic in _entityStatistics) {
                statistic.Start();
            }
        }

        protected virtual void Update() {
            float deltaTime = Time.deltaTime;

            foreach (EntityStatistic statistic in _entityStatistics) {
                statistic.Tick(deltaTime);
            }
        }

        /// <summary>
        /// Looks up the registered statistic of type T (e.g. HealthValue) — used by
        /// subclasses to expose a strongly-typed accessor (see BaseStatisticController.Health)
        /// backed by this controller's list instead of a dedicated [SerializeField].
        /// </summary>
        public T GetStatistic<T>() where T : EntityStatistic {
            foreach (EntityStatistic statistic in _entityStatistics) {
                if (statistic is T typed) { return typed; }
            }

            return null;
        }

        /// <summary>
        /// Looks up an statistic by its Name — "Strength" for a generic statistic the designer
        /// named in the Inspector, or "Health" etc. for a typed one. Null if there is none.
        /// </summary>
        public EntityStatistic Get(string name) {
            foreach (EntityStatistic statistic in _entityStatistics) {
                if (statistic.Name == name) { return statistic; }
            }

            return null;
        }

        /// <summary>
        /// Looks up the boxed value object by its own type — Get&lt;Mana&gt;() rather than
        /// GetStatistic&lt;ManaValue&gt;().Value, and works for any statistic added later
        /// without this controller needing a property for it. Null if there is none.
        /// </summary>
        public TValue Get<TValue>() where TValue : class {
            foreach (EntityStatistic statistic in _entityStatistics) {
                if (statistic.BoxedValue is TValue typed) { return typed; }
            }

            return null;
        }

        /// <summary>
        /// Non-generic counterpart to GetStatistic&lt;T&gt;() — used by combat data (DamageValue/
        /// Reduction) that only has a System.Type picked at edit time (see StatisticTypeReference),
        /// not a compile-time type parameter.
        /// </summary>
        public EntityStatistic GetStatistic(System.Type statisticType) {
            foreach (EntityStatistic statistic in _entityStatistics) {
                if (statisticType.IsInstanceOfType(statistic)) { return statistic; }
            }

            return null;
        }

        /// <summary>
        /// Registers an statistic at runtime — used by StatisticCombatController.Reset() to
        /// auto-seed a mandatory Health entry, mirroring UnityPlayerInputController.Reset()'s
        /// auto-seeded movement mapping.
        /// </summary>
        protected void AddStatistic(EntityStatistic statistic) {
            _entityStatistics.Add(statistic);
        }

        /// <summary>
        /// Registers the statistic unless one with the same Name is already present — so a
        /// controller's own statistic always wins over one supplied by something shared (a
        /// definition asset). Returns whether it was added.
        /// </summary>
        protected bool AddStatisticIfMissing(EntityStatistic statistic) {
            if (statistic == null || Get(statistic.Name) != null) { return false; }

            _entityStatistics.Add(statistic);
            return true;
        }

        /// <summary>
        /// Every registered statistic, in order — for editor tooling and tests.
        /// </summary>
        public IReadOnlyList<EntityStatistic> Statistics {
            get { return _entityStatistics; }
        }
    }
}