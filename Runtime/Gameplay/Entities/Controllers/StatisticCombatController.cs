using UnityEngine;

using System;
using System.Collections.Generic;

using BBUnity.Gameplay.Entities.Controllers.Statistics;
using BBUnity.Gameplay.Entities.Controllers.Statistics.Specialized;
using BBUnity.Gameplay.Entities.Controllers.Combat;
using BBUnity.Gameplay.Statistics.Specialized;

using BBUnity.Gameplay.Entities.Controllers.Combat.HitEffects;
namespace BBUnity.Gameplay.Entities.Controllers {

    /// <summary>
    /// A fully-fledged combat controller built on top of StatisticController/PooledEffectController —
    /// those stay low-level, reusable on their own; this is the "just works" option for anything
    /// that fights. Still an StatisticController underneath (see StateController-facing code
    /// like BaseState.StatisticController), so statistic access works exactly the same way.
    ///
    /// Auto-seeds a Health statistic (see Reset()) so every combat entity has one without needing
    /// to click "Add Statistic" for it; Poise, Mana etc. are still added per-entity like any other
    /// statistic.
    ///
    /// Owns both halves of the hit pipeline as data, generic over which statistic is involved —
    /// neither Attacks nor Reductions are hardcoded to Health/Poise, they're lists of "pick an
    /// statistic, then how much" entries (see DamageEntry/ReductionEntry), so a single Attack can
    /// damage any number of statistics, and any statistic can have its own mitigation:
    /// - Attacks: each one fully self-contained (its own AttackCollider prefab, spawn position,
    ///   and DamageEntries) — see PerformAttack().
    /// - Reductions: what this entity mitigates, per statistic, when it takes a hit — see
    ///   ApplyHit().
    /// </summary>
    [AddComponentMenu("BBUnity/Gameplay/Entities/Controllers/Statistic Combat Controller")]
    public class StatisticCombatController : StatisticController {

        [SerializeField, Tooltip("Shared combat data (statistics, reductions) for this entity. Anything set directly on this controller overrides it — see CombatantDefinition.")]
        private CombatantDefinition _definition;

        [SerializeField]
        private List<Attack> _attacks = new List<Attack>();

        [SerializeField]
        private List<ReductionEntry> _reductions = new List<ReductionEntry>();

        [SerializeField, Tooltip("What this entity is made of (Flesh, Bone, Foliage, ...), used to choose how it looks when hit. Added to the Definition's Surface Tags.")]
        private List<CombatTagDefinition> _surfaceTags = new List<CombatTagDefinition>();

        [SerializeField, Tooltip("How this entity looks when hit, by the hit's tags and this entity's Surface Tags. The most specific matching row plays; where this and the Definition tie, this one wins.")]
        private HitEffectTable _receiverHitEffects = new HitEffectTable();

        [SerializeField, Tooltip("However much a hit to a given statistic is mitigated, at least this much always gets through — unless the hit dealt none of that statistic's damage to begin with.")]
        private int _minimumDamage = 1;

        // Set by e.g. a hyper-armor attack (GroundedChargedAttackState) — poise damage is
        // ignored entirely while true; every other statistic's damage still applies.
        private bool _isPoiseImmune = false;

        private PooledEffectController _pooledEffectController;

        // The controller's own copy of the Definition asset, so runtime changes never write back
        // into the asset or leak to another entity using it.
        private CombatantDefinition _runtimeDefinition;

        // Controller Reductions plus whichever Definition ones weren't overridden — what
        // ApplyDamage actually reads.
        private readonly List<ReductionEntry> _effectiveReductions = new List<ReductionEntry>();

        // Controller surface tags plus the Definition's.
        private readonly List<CombatTagDefinition> _effectiveSurfaceTags = new List<CombatTagDefinition>();

        /// <summary>
        /// Fired once accumulated poise damage crosses the flinch threshold. The game layer
        /// (e.g. PlayerStateController) listens for this to force a reaction state — this class
        /// deliberately knows nothing about states itself.
        /// </summary>
        public event System.Action Flinched;

        private bool _flinchSignaled;

        /// <summary>
        /// Fired once per ApplyHit() call, before any mitigation/statistic changes — "this entity
        /// was hit at all", independent of how much damage actually landed. The game layer uses
        /// this for reactions that should happen on any hit rather than only a poise break — e.g.
        /// EnemyAIController's knockback, which needs HitInfo.Source to know which way to push.
        /// </summary>
        public event System.Action<HitInfo> Hit;

        /// <summary>
        /// Lets the game decide, per incoming hit, to take over from the normal pipeline: return
        /// true and ApplyHit() stops there — no Hit, no Reductions, no statistic changes — so
        /// whatever set this is fully responsible for the hit (blocking it, redirecting it,
        /// answering it with a hit of its own, ...). Null means every hit is applied normally.
        /// Typically set by a state when it is entered and cleared when it exits; this class has no
        /// opinion on what an interception means.
        /// </summary>
        public System.Func<HitInfo, bool> HitInterceptor { get; set; }

        /// <summary>
        /// Fired when HitInterceptor took a hit instead of the normal pipeline — for feedback
        /// (effects, floating text) that doesn't care why it was intercepted.
        /// </summary>
        public event System.Action<HitInfo> HitIntercepted;

        // Time.time at which each CombatActionDefinition may next be used — see TryUse().
        private readonly Dictionary<CombatActionDefinition, float> _actionReadyAt = new Dictionary<CombatActionDefinition, float>();

        public Health Health {
            get { return GetStatistic<HealthValue>()?.Value; }
        }

        public Poise Poise {
            get { return GetStatistic<PoiseValue>()?.Value; }
        }

        public Mana Mana {
            get { return GetStatistic<ManaValue>()?.Value; }
        }

        protected override void Awake() {
            base.Awake();

            _effectiveReductions.Clear();
            _effectiveReductions.AddRange(_reductions);

            _effectiveSurfaceTags.Clear();
            _effectiveSurfaceTags.AddRange(_surfaceTags);

            if (_definition == null) { return; }

            _runtimeDefinition = Instantiate(_definition);

            foreach (CombatTagDefinition tag in _runtimeDefinition.SurfaceTags) {
                if (tag != null && !_effectiveSurfaceTags.Contains(tag)) { _effectiveSurfaceTags.Add(tag); }
            }

            for (int i = 0; i < _runtimeDefinition.Statistics.Count; i++) {
                AddStatisticIfMissing(_runtimeDefinition.Statistics[i]);
            }

            foreach (ReductionEntry entry in _runtimeDefinition.Reductions) {
                if (entry == null || OverridesReduction(entry.TargetStatisticType)) { continue; }

                _effectiveReductions.Add(entry);
            }
        }

        private void OnDestroy() {
            if (_runtimeDefinition != null) {
                Destroy(_runtimeDefinition);
            }
        }

        // True if this controller sets a reduction for the statistic itself, in which case the
        // Definition's reductions for it are ignored.
        private bool OverridesReduction(Type statisticType) {
            foreach (ReductionEntry entry in _reductions) {
                if (entry.TargetStatisticType == statisticType) { return true; }
            }

            return false;
        }

        private void Reset() {
            if (GetStatistic<HealthValue>() == null) {
                AddStatistic(new HealthValue());
            }
        }

        public void SetPoiseImmune(bool immune) {
            _isPoiseImmune = immune;
        }

        /// <summary>
        /// Spawns the named Attack's AttackCollider prefab (pooled, via PooledEffectController — added
        /// silently to this GameObject if it doesn't already have one; see GetOrAddPooledEffectController)
        /// at its configured spawn position, and stamps it with this entity as the source and the
        /// attack's id, so it can resolve this same Attack's damage again at the moment it lands
        /// a hit (see CreateHit). Logs a warning and no-ops on any missing setup — a missing
        /// AttackCollider prefab, spawn position, or unregistered id.
        /// </summary>
        public GameObject PerformAttack(string id) {
            Attack attack = _attacks.Find(a => a.Id == id);
            if (attack == null) {
                Debug.LogWarning($"StatisticCombatController on '{gameObject.name}' has no Attack registered with id '{id}'.", this);
                return null;
            }

            if (attack.AttackColliderPrefab == null) {
                Debug.LogWarning($"StatisticCombatController on '{gameObject.name}': Attack '{id}' has no AttackCollider prefab assigned.", this);
                return null;
            }

            if (attack.SpawnPosition == null) {
                Debug.LogWarning($"StatisticCombatController on '{gameObject.name}': Attack '{id}' has no spawn position assigned.", this);
                return null;
            }

            GameObject instance = GetOrAddPooledEffectController().Spawn(id, attack.AttackColliderPrefab.gameObject, attack.SpawnPosition.position);

            AttackCollider attackCollider = instance.GetComponent<AttackCollider>();
            attackCollider.SetAttackId(id);
            attackCollider.SetSource(gameObject);

            return instance;
        }

        /// <summary>
        /// Resolves the named Attack's DamageEntries against this (attacking) controller's own
        /// statistics into a hit's raw, pre-mitigation numbers — called by AttackCollider at the
        /// moment of impact, not when PerformAttack() spawns it, so stat-scaled damage reflects
        /// the attacker's current state rather than a value snapshotted earlier.
        /// </summary>
        public HitInfo CreateHit(string attackId) {
            Attack attack = _attacks.Find(a => a.Id == attackId);

            if (attack == null) {
                Debug.LogWarning($"StatisticCombatController on '{gameObject.name}' has no Attack registered with id '{attackId}'.", this);
                return new HitInfo(gameObject, Array.Empty<StatisticDamage>());
            }

            return BuildHit($"Attack '{attackId}'", attack.DamageEntries, attack.Tags, attack.HitEffects);
        }

        /// <summary>
        /// The same as CreateHit(string), for a CombatActionDefinition: its DamageEntries resolved against
        /// this (acting) controller's own statistics, carrying the action's tags.
        /// </summary>
        public HitInfo CreateHit(CombatActionDefinition action) {
            if (action == null) { return new HitInfo(gameObject, Array.Empty<StatisticDamage>()); }

            return BuildHit($"Action '{action.name}'", action.DamageEntries, action.Tags, action.HitEffects);
        }

        private HitInfo BuildHit(string label, IReadOnlyList<DamageEntry> entries, IReadOnlyList<CombatTagDefinition> tags, HitEffectList effects) {
            var damages = new List<StatisticDamage>(entries.Count);
            foreach (DamageEntry entry in entries) {
                Type statisticType = entry.TargetStatisticType;
                if (statisticType == null) {
                    Debug.LogWarning($"StatisticCombatController on '{gameObject.name}': {label} has a Damage Entry with no Target Statistic set — it will be skipped.", this);
                    continue;
                }

                damages.Add(new StatisticDamage(statisticType, entry.ResolveAmount(this)));
            }

            return new HitInfo(gameObject, damages, tags, effects);
        }

        /// <summary>
        /// True if the action's cooldown has passed (or it has none, or it was never used).
        /// </summary>
        public bool IsReady(CombatActionDefinition action) {
            if (action == null) { return false; }

            return !_actionReadyAt.TryGetValue(action, out float readyAt) || Time.time >= readyAt;
        }

        /// <summary>
        /// Starts the action's cooldown and returns true, or returns false (changing nothing) if it
        /// is still cooling down. Call it at the moment the action is committed to.
        /// </summary>
        public bool TryUse(CombatActionDefinition action) {
            if (!IsReady(action)) { return false; }

            if (action.Cooldown > 0) {
                _actionReadyAt[action] = Time.time + action.Cooldown / 1000f;
            }

            return true;
        }

        /// <summary>
        /// Applies this (defending) controller's own Reductions to each of an incoming hit's
        /// per-statistic amounts, then reduces that statistic. Poise damage is skipped entirely
        /// while poise-immune; a successful poise break still fires Flinched exactly as before.
        /// A hit taken by HitInterceptor skips all of this, including its effects.
        /// </summary>
        public void ApplyHit(HitInfo hit) {
            if (HitInterceptor != null && HitInterceptor(hit)) {
                HitIntercepted?.Invoke(hit);
                return;
            }

            Hit?.Invoke(hit);

            foreach (StatisticDamage damage in hit.Damages) {
                ApplyDamage(damage.StatisticType, damage.Amount);
            }

            PlayHitEffects(hit);
        }

        /// <summary>
        /// A pooled spawn through this entity's PooledEffectController (added if missing) — what
        /// SpawnEffect uses. The spawned object takes this entity's scale; the caller adjusts it.
        /// </summary>
        public GameObject SpawnPooled(string name, GameObject prefab, Vector3 position) {
            return GetOrAddPooledEffectController().Spawn(name, prefab, position);
        }

        // The hit's own effects (the blade's spark) and then this entity's response to it, the
        // most specific matching row of its own table and the Definition's.
        private void PlayHitEffects(HitInfo hit) {
            var context = new HitContext(hit, this);

            hit.ActionEffects?.Play(context);

            HitEffectRule own = _receiverHitEffects.Resolve(hit.Tags, _effectiveSurfaceTags);
            HitEffectRule shared = _runtimeDefinition != null ? _runtimeDefinition.ReceiverHitEffects.Resolve(hit.Tags, _effectiveSurfaceTags) : null;

            HitEffectRule rule = own;
            if (shared != null && (own == null || shared.Specificity > own.Specificity)) { rule = shared; }

            rule?.Effects.Play(context);
        }

        private void ApplyDamage(Type statisticType, int rawAmount) {
            if (statisticType == null) { return; }
            if (statisticType == typeof(PoiseValue) && _isPoiseImmune) { return; }

            EntityStatistic targetStatistic = GetStatistic(statisticType);
            if (targetStatistic == null) {
                Debug.LogWarning($"StatisticCombatController on '{gameObject.name}' took damage targeting {statisticType.Name}, but has no such statistic — it was ignored.", this);
                return;
            }

            int mitigated = MitigateDamage(rawAmount, GetReductionsFor(statisticType));
            if (mitigated <= 0) { return; }

            targetStatistic.TakeDamage(mitigated);

            if (targetStatistic is PoiseValue poiseStatistic) {
                // The flinch counter is deliberately left set: a state machine polls
                // Poise.Flinched to play its flinch animation, and the state consuming it
                // (ResetFlinchCounter on Enter) is what clears it. Resetting here, in the same
                // call that raises the event, meant the state never saw it. Flinched fires once
                // per flinch, however many hits land before the state picks it up.
                if (!poiseStatistic.Value.Flinched) {
                    _flinchSignaled = false;
                } else if (!_flinchSignaled) {
                    _flinchSignaled = true;
                    Flinched?.Invoke();
                }
            }
        }

        private List<Reduction> GetReductionsFor(Type statisticType) {
            var matching = new List<Reduction>();

            foreach (ReductionEntry entry in _effectiveReductions) {
                if (entry.Reduction != null && entry.TargetStatisticType == statisticType) {
                    matching.Add(entry.Reduction);
                }
            }

            return matching;
        }

        /// <summary>
        /// Default combination for a list of Reductions targeting one statistic:
        /// "additive-then-flat" — every Percentage reduction is summed into one combined
        /// percentage and applied once (so stacking two 25% resistances gives 50% off, not 25%
        /// off twice, compounding), then every Flat reduction is subtracted from what's left.
        /// Overridable by a subclass that wants a different combination rule.
        /// </summary>
        protected virtual int MitigateDamage(int rawDamage, List<Reduction> reductions) {
            if (rawDamage <= 0) { return 0; }

            float totalPercentage = 0f;
            float totalFlat = 0f;

            foreach (Reduction reduction in reductions) {
                float amount = reduction.Resolve(this);

                if (reduction.Kind == ReductionKind.Percentage) {
                    totalPercentage += amount;
                } else {
                    totalFlat += amount;
                }
            }

            float afterPercentage = rawDamage * (1f - Mathf.Clamp01(totalPercentage));
            int mitigated = Mathf.RoundToInt(afterPercentage - totalFlat);

            return Mathf.Max(_minimumDamage, mitigated);
        }

        // Silent by design (see PerformAttack) — a fully-fledged combat controller shouldn't
        // require its user to remember a second, unrelated component just to spawn attacks.
        // We'll likely want this configurable later (e.g. warn instead of auto-add).
        private PooledEffectController GetOrAddPooledEffectController() {
            if (_pooledEffectController != null) { return _pooledEffectController; }

            _pooledEffectController = GetComponent<PooledEffectController>();
            if (_pooledEffectController == null) {
                _pooledEffectController = gameObject.AddComponent<PooledEffectController>();
            }

            return _pooledEffectController;
        }
    }
}
