using UnityEngine;

using BBUnity.Gameplay.Controllers;

namespace BBUnity.Gameplay.Entities.Controllers.Combat.HitEffects {

    /// <summary>
    /// Slows (or freezes) game time briefly when a hit lands — the "hit-stop" that makes a blow
    /// feel heavy. Goes through the SessionController's TimeController like every other change to
    /// time, so it can't fight another system over Time.timeScale.
    /// </summary>
    [System.Serializable]
    public class TimeScaleEffect : HitEffect {

        [SerializeField, Min(0f), Tooltip("The time scale during the effect: 0 freezes time, 1 does nothing.")]
        private float _scale = 0.1f;

        [SerializeField, Min(0), Tooltip("How long it lasts, in real milliseconds — not slowed-down milliseconds.")]
        private int _duration = 80;

        [SerializeField, Tooltip("When on, multiplies with any other slowdown already running; when off it overrides them for its duration.")]
        private bool _cumulative = false;

        public override void Play(HitContext context) {
            if (_duration <= 0) { return; }

            SessionController gameplay = SessionController.Current;
            if (gameplay == null || gameplay.TimeController == null) { return; }

            gameplay.TimeController.RequestTimeScale(_scale, _duration, _cumulative);
        }
    }
}
