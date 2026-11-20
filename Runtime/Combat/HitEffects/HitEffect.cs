namespace BBUnity.Combat.HitEffects {

    /// <summary>
    /// One thing that happens when a hit lands — a spark, a sound, a camera shake. Authored as a
    /// [SerializeReference] entry in a HitEffectList, so each concrete subclass brings its own
    /// fields and is picked from a dropdown; add a new kind of effect by subclassing this. Carries
    /// no state about any particular hit: everything it needs arrives in the HitContext.
    /// </summary>
    [System.Serializable]
    public abstract class HitEffect {

        public abstract void Play(HitContext context);
    }
}
