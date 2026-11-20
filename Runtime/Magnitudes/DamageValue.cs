using BBUnity.Statistics;

namespace BBUnity.Magnitudes {

    /// <summary>
    /// The type listed in an Attack's damage-type picker and stored in its [SerializeReference]
    /// fields (HealthDamage/PoiseDamage) — mirrors EntityState/EntityStatistic's "add a concrete
    /// type from a dropdown" pattern, but for a single polymorphic field rather than a list (see
    /// DamageValueDrawer). Resolved against the *attacker's* StatisticController at the
    /// moment a hit lands (see AttackCollider), before the defender's Reductions are applied.
    /// </summary>
    [System.Serializable]
    public abstract class DamageValue {

        public abstract int Resolve(StatisticController attacker);
    }
}
