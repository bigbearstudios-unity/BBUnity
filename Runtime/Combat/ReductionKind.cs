namespace BBUnity.Combat {

    /// <summary>
    /// Which bucket a Reduction's Resolve() value belongs to when StatisticCombatController
    /// combines a whole list of them — see StatisticCombatController.MitigateDamage() for the
    /// default "additive-then-flat" combination (all Percentage values summed and applied once,
    /// then all Flat values subtracted from the remainder).
    /// </summary>
    public enum ReductionKind {
        Flat,
        Percentage
    }
}
