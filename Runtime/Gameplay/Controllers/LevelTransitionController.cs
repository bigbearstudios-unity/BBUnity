using UnityEngine;

using System.Collections;

namespace BBUnity.Gameplay.Controllers {

    /// <summary>
    /// Controls how the screen looks while SessionController moves between levels. Put one in
    /// the Gameplay scene (or anywhere under the SessionController's GameObject); if there isn't
    /// one, level transitions simply cut instantly.
    ///
    /// The type decides which half-steps run — see LevelTransitionType. Timing uses unscaled
    /// time, so a hit-stop or slow-motion in progress can't stall or stretch a transition.
    /// </summary>
    [AddComponentMenu("BBUnity/Gameplay/Controllers/Level Transition Controller")]
    public class LevelTransitionController : MonoBehaviour {

        [SerializeField]
        private LevelTransitionType _transitionType = LevelTransitionType.Fade;

        // Fade settings — only shown (see LevelTransitionControllerEditor) when the type is Fade.
        [SerializeField, Tooltip("The color the screen fades to. Its alpha is the opacity reached at the fully faded point.")]
        private Color _fadeColor = Color.black;

        [SerializeField, Min(0), Tooltip("Milliseconds taken to fade from the game to the Fade Color.")]
        private int _fadeOutTime = 500;

        [SerializeField, Min(0), Tooltip("Milliseconds taken to fade from the Fade Color back to the game.")]
        private int _fadeInTime = 500;

        [SerializeField, Min(0), Tooltip("Milliseconds the screen stays fully covered at minimum, counted from the end of Out(). If the next level is ready sooner, In() waits out the rest — stops a very fast load flashing past.")]
        private int _minimumTransitionTime = 0;

        [SerializeField, Tooltip("Stops game time (the Player, enemies, physics, ...) from the moment the transition starts until the fade-in begins, so the Player can start moving while the screen is still fading in. The fade itself and the minimum time still run in real time.")]
        private bool _stopTimeOnTransition = true;

        [SerializeField, Min(0), Tooltip("Milliseconds after a level transition finishes during which another one is ignored — stops arriving inside a transition trigger from bouncing straight back.")]
        private int _transitionCooldown = DefaultTransitionCooldown;

        private int _timeStopId = -1;
        private float _coveredAt = float.NegativeInfinity;

        /// <summary>
        /// The cooldown SessionController uses when a scene has no LevelTransitionController.
        /// </summary>
        public const int DefaultTransitionCooldown = 500;

        public LevelTransitionType TransitionType {
            get { return _transitionType; }
        }

        /// <summary>
        /// Milliseconds the screen stays fully covered at minimum.
        /// </summary>
        public int MinimumTransitionTime {
            get { return _minimumTransitionTime; }
        }

        /// <summary>
        /// Milliseconds after a transition finishes during which another one is ignored.
        /// </summary>
        public int TransitionCooldown {
            get { return _transitionCooldown; }
        }

        /// <summary>
        /// Whether game time is stopped for the duration of a transition.
        /// </summary>
        public bool StopTimeOnTransition {
            get { return _stopTimeOnTransition; }
        }

        /// <summary>
        /// First half: covers the screen. Yield on it before starting the swap.
        /// </summary>
        public IEnumerator Out() {
            StopTime();

            switch (_transitionType) {
                case LevelTransitionType.Fade:
                    yield return Fade(_fadeColor.a, _fadeOutTime);
                    break;
                case LevelTransitionType.LoadingScreen:
                    break;
            }

            _coveredAt = Time.unscaledTime;
        }

        /// <summary>
        /// Second half: uncovers the screen once the new level is in place — and once the
        /// Minimum Transition Time has passed since Out() finished.
        /// </summary>
        public IEnumerator In() {
            float uncoverAt = _coveredAt + _minimumTransitionTime / 1000f;
            while (Time.unscaledTime < uncoverAt) { yield return null; }

            _coveredAt = float.NegativeInfinity;

            // Time restarts as the fade-in begins, so the Player can already be running while the
            // screen is still uncovering.
            ReleaseTime();

            yield return Uncover();
        }

        private IEnumerator Uncover() {
            switch (_transitionType) {
                case LevelTransitionType.Fade:
                    yield return Fade(0f, _fadeInTime);
                    break;
                case LevelTransitionType.LoadingScreen:
                    break;
            }
        }

        // Held through the TimeController as a non-cumulative scale-0 request that never times
        // out by itself, so it overrides any hit-stop in progress and is released explicitly.
        private void StopTime() {
            if (!_stopTimeOnTransition || _timeStopId >= 0) { return; }

            SessionController gameplay = SessionController.Current;
            if (gameplay == null || gameplay.TimeController == null) { return; }

            _timeStopId = gameplay.TimeController.RequestTimeScale(0f, int.MaxValue, false);
        }

        private void ReleaseTime() {
            if (_timeStopId < 0) { return; }

            SessionController gameplay = SessionController.Current;
            if (gameplay != null && gameplay.TimeController != null) {
                gameplay.TimeController.Cancel(_timeStopId);
            }

            _timeStopId = -1;
        }

        // Never leave the game frozen if this goes away mid-transition.
        private void OnDisable() {
            ReleaseTime();
        }

        // The overlay is shared through SessionController, so a level transition and anything
        // else that fades the screen (a death screen, ...) never draw two of their own. Falls back
        // to one on this object if there is no SessionController.
        private IEnumerator Fade(float toAlpha, int milliseconds) {
            ScreenFader fader = SessionController.Current != null ? SessionController.Current.ScreenFader : null;
            if (fader == null) {
                fader = GetComponent<ScreenFader>();
                if (fader == null) { fader = gameObject.AddComponent<ScreenFader>(); }
            }

            yield return fader.Fade(_fadeColor, toAlpha, milliseconds);
        }
    }
}
