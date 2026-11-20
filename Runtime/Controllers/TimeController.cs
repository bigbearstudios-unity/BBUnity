using UnityEngine;

using System.Collections.Generic;

namespace BBUnity.Controllers {

    /// <summary>
    /// The single owner of Time.timeScale — anything that wants time to run faster or slower (a
    /// hit-stop on an enemy, a slow-motion finisher, a game-wide speed-up, ...) asks this instead
    /// of writing Time.timeScale itself, so two systems can never fight over it or leave it stuck
    /// at a stray value. Lives next to SessionController and is reached through
    /// SessionController.Current.TimeController (added automatically if the scene doesn't have
    /// one — see SessionController.Awake).
    ///
    /// Every request is a *timed modifier*: a scale and how long it lasts, measured in real
    /// (unscaled) milliseconds — a 100ms hit-stop lasts 100ms of wall-clock time, not 1s of its own
    /// slowed-down time. When a modifier times out it is simply dropped, so once everything has
    /// expired the effective scale falls back to BaseTimeScale (1 unless a game changed it) with
    /// nothing to remember or undo. ResetToNormal() does the same immediately.
    ///
    /// Each modifier is either:
    /// - Cumulative: multiplies with every other cumulative modifier and BaseTimeScale. Two
    ///   cumulative 0.5x slow-downs running together give 0.25x.
    /// - Not cumulative: sets the time scale to exactly its own value for as long as it lasts,
    ///   ignoring BaseTimeScale and every cumulative modifier. If several of these overlap, the
    ///   most recently requested one wins, and an earlier one takes over again if it outlasts it.
    ///
    /// Time.fixedDeltaTime is deliberately left alone — physics simply steps less often per real
    /// second while slowed, which is what a hit-stop wants.
    /// </summary>
    [AddComponentMenu("BBUnity/Controllers/Time Controller")]
    public class TimeController : MonoBehaviour {

        // Unity's own documented upper limit for Time.timeScale.
        private const float MaxTimeScale = 100f;

        private struct Modifier {
            public int Id;
            public float Scale;
            public float Remaining;
            public bool Cumulative;
        }

        private readonly List<Modifier> _modifiers = new List<Modifier>();

        private float _baseTimeScale = 1f;
        private int _nextId = 1;

        /// <summary>
        /// The speed the game runs at when no modifier is active — 1 for normal speed. Cumulative
        /// modifiers multiply on top of this; a non-cumulative one ignores it. Takes effect
        /// immediately.
        /// </summary>
        public float BaseTimeScale {
            get { return _baseTimeScale; }
            set {
                _baseTimeScale = Mathf.Clamp(value, 0f, MaxTimeScale);
                Apply();
            }
        }

        /// <summary>
        /// How many modifiers are currently running.
        /// </summary>
        public int ActiveModifierCount {
            get { return _modifiers.Count; }
        }

        /// <summary>
        /// The scale this controller is currently asking Time.timeScale to be.
        /// </summary>
        public float EffectiveTimeScale {
            get { return Resolve(); }
        }

        /// <summary>
        /// Starts a timed modifier — see the class comment for what cumulative means.
        /// </summary>
        /// <param name="scale">0 stops time, below 1 slows it down, above 1 speeds it up.</param>
        /// <param name="duration">How long it lasts, in real (unscaled) milliseconds. Zero or less
        /// is ignored.</param>
        /// <param name="cumulative">True to multiply with the other cumulative modifiers and
        /// BaseTimeScale; false to override them all for its duration.</param>
        /// <returns>An id to pass to Cancel(), or -1 if nothing was started.</returns>
        public int RequestTimeScale(float scale, int duration, bool cumulative) {
            if (duration <= 0f) { return -1; }

            var modifier = new Modifier {
                Id = _nextId++,
                Scale = Mathf.Clamp(scale, 0f, MaxTimeScale),
                Remaining = duration / 1000f,
                Cumulative = cumulative
            };

            _modifiers.Add(modifier);
            Apply();

            return modifier.Id;
        }

        /// <summary>
        /// Ends one modifier early. False if it had already expired or never existed.
        /// </summary>
        public bool Cancel(int id) {
            int index = _modifiers.FindIndex(m => m.Id == id);
            if (index < 0) { return false; }

            _modifiers.RemoveAt(index);
            Apply();

            return true;
        }

        /// <summary>
        /// Drops every modifier and puts BaseTimeScale back to 1 — straight back to normal speed.
        /// </summary>
        public void ResetToNormal() {
            _modifiers.Clear();
            _baseTimeScale = 1f;
            Apply();
        }

        /// <summary>
        /// Ages every modifier by the given real-time seconds, dropping the ones that have timed
        /// out. Called every frame from Update() with Time.unscaledDeltaTime; public so a test (or
        /// anything else driving time by hand) can step it deterministically.
        /// </summary>
        public void Advance(float unscaledDeltaTime) {
            if (_modifiers.Count == 0) { return; }

            for (int i = _modifiers.Count - 1; i >= 0; i--) {
                Modifier modifier = _modifiers[i];
                modifier.Remaining -= unscaledDeltaTime;

                if (modifier.Remaining <= 0f) {
                    _modifiers.RemoveAt(i);
                } else {
                    _modifiers[i] = modifier;
                }
            }

            Apply();
        }

        private void Update() {
            Advance(Time.unscaledDeltaTime);
        }

        // Never leave the game stuck slowed down if this goes away mid-modifier (scene unload,
        // the Gameplay object being destroyed, leaving play mode).
        private void OnDisable() {
            _modifiers.Clear();
            Time.timeScale = 1f;
        }

        private float Resolve() {
            for (int i = _modifiers.Count - 1; i >= 0; i--) {
                if (!_modifiers[i].Cumulative) { return _modifiers[i].Scale; }
            }

            float scale = _baseTimeScale;
            foreach (Modifier modifier in _modifiers) {
                scale *= modifier.Scale;
            }

            return Mathf.Clamp(scale, 0f, MaxTimeScale);
        }

        private void Apply() {
            Time.timeScale = Resolve();
        }
    }
}
