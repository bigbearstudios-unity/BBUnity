using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;

using BBUnity.Audio;

using BBUnity.GameFlow;
using BBUnity.Support.Scenes;
using BBUnity.UI;
using BBUnity.World;
namespace BBUnity.Controllers {

    /// <summary>
    /// The persistent "game session" tier — stays loaded for the whole play session, with Level
    /// scenes loading/unloading additively on top of it (see LevelTransitionNode). Owns the
    /// Player, instantiated immediately (see SpawnPlayer) rather than waiting on the first Level,
    /// and the shared UI root everything else (main HUD, pooled enemy health bars, ...) hangs off
    /// — that's what fixes needing a Canvas per enemy: a Level-scene object reaches this via
    /// Current the same way an enemy's AttackCollider reaches its attacker's controller. Also
    /// owns the TimeController — the one place anything slows down or speeds up game time from —
    /// the ScreenFader, the one overlay anything that fades the screen shares, and the
    /// SoundPlayer, the pool of voices every sound effect plays through.
    ///
    /// Override OnReady() to wire up whatever else a concrete game needs once the Player exists —
    /// inventory, quest tracking, audio, ... this framework has no opinion on what those are.
    /// </summary>
    public abstract class SessionController : MonoBehaviour {

        public static SessionController Current { get; private set; }

        [SerializeField, Tooltip("Instantiated immediately when this scene loads — must carry a PlayerController component.")]
        private GameObject _playerPrefab;

        [SerializeField, Tooltip("Where the Player is instantiated. Falls back to this GameObject's own position if unassigned. Level scenes are responsible for repositioning the Player to a PlayerSpawnNode once loaded — this is only the very first, pre-Level placement.")]
        private Transform _playerSpawnAnchor;

        [SerializeField, Tooltip("The Canvas shared UI (main HUD, pooled enemy health bars, ...) should be parented under.")]
        private Canvas _sharedUIRoot;

        private PlayerController _player;
        private TimeController _timeController;
        private ScreenFader _screenFader;
        private SoundPlayer _audioController;
        private float _transitionAllowedAt;

        public PlayerController Player {
            get { return _player; }
        }

        public TimeController TimeController {
            get { return _timeController; }
        }

        public ScreenFader ScreenFader {
            get { return _screenFader; }
        }

        public SoundPlayer SoundPlayer {
            get { return _audioController; }
        }

        public Canvas SharedUIRoot {
            get { return _sharedUIRoot; }
        }

        /// <summary>
        /// True while a level transition (see TransitionToLevel) is in progress.
        /// </summary>
        public bool IsTransitioning { get; private set; }

        protected virtual void Awake() {
            Current = this;

            EnsureTimeController();
            EnsureScreenFader();
            EnsureAudioController();
            SpawnPlayer();
            OnReady();
        }

        protected virtual void OnDestroy() {
            if (Current == this) { Current = null; }
        }

        /// <summary>
        /// Moves the Player into another level: loads `targetScene` additively if it isn't loaded
        /// yet, places the Player at the PlayerSpawnNode in it with id `spawnNodeId` (the scene's
        /// first spawn node if blank), makes it the active scene, then unloads `sourceScene`.
        /// If a LevelTransitionController is in the scene it covers the screen first and uncovers
        /// it afterwards. Ignored while another transition is running or just finished.
        /// </summary>
        public void TransitionToLevel(SceneReference targetScene, string spawnNodeId, Scene sourceScene) {
            if (IsTransitioning || Time.unscaledTime < _transitionAllowedAt) { return; }

            // Optional — without a LevelTransitionController in the scene, levels just cut.
            LevelTransitionController screen = FindTransitionController();

            if (_player == null) {
                Debug.LogWarning($"SessionController on '{gameObject.name}' can't transition levels without a Player.", this);
                return;
            }

            if (targetScene == null || !targetScene.IsSet) {
                Debug.LogWarning($"SessionController on '{gameObject.name}': level transition requested with no target scene picked.", this);
                return;
            }

            StartCoroutine(TransitionRoutine(targetScene.SceneName, spawnNodeId, sourceScene, screen));
        }

        private IEnumerator TransitionRoutine(string targetSceneName, string spawnNodeId, Scene sourceScene, LevelTransitionController screen) {
            IsTransitioning = true;

            if (screen != null) { yield return screen.Out(); }

            Scene target = SceneManager.GetSceneByName(targetSceneName);
            if (!target.isLoaded) {
                AsyncOperation load = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
                if (load == null) {
                    Debug.LogWarning($"SessionController: scene '{targetSceneName}' couldn't be loaded — is it in the Build Settings?", this);
                    if (screen != null) { yield return screen.In(); }
                    IsTransitioning = false;
                    yield break;
                }

                while (!load.isDone) { yield return null; }

                target = SceneManager.GetSceneByName(targetSceneName);
            }

            PlayerSpawnNode spawnNode = PlayerSpawnNode.Find(target, spawnNodeId);
            if (spawnNode != null) {
                _player.TeleportTo(spawnNode.transform.position);
            } else {
                Debug.LogWarning($"SessionController: scene '{targetSceneName}' has no PlayerSpawnNode with id '{spawnNodeId}' — the Player was left where it was.", this);
            }

            SceneManager.SetActiveScene(target);

            if (sourceScene.IsValid() && sourceScene != target) {
                SceneLoader.Unload(sourceScene);
            }

            if (screen != null) { yield return screen.In(); }

            _transitionAllowedAt = Time.unscaledTime + (screen != null ? screen.TransitionCooldown : LevelTransitionController.DefaultTransitionCooldown) / 1000f;
            IsTransitioning = false;
        }

        private LevelTransitionController FindTransitionController() {
            LevelTransitionController screen = GetComponentInChildren<LevelTransitionController>();
            return screen != null ? screen : FindFirstObjectByType<LevelTransitionController>();
        }

        /// <summary>
        /// Runs once, after SpawnPlayer() — extension point for a concrete game's own
        /// cross-cutting systems.
        /// </summary>
        protected virtual void OnReady() { }

        // Added silently if the scene's Gameplay object doesn't already carry one, so every game
        // gets time control without a scene edit — it's shared infrastructure, not per-game setup.
        private void EnsureTimeController() {
            _timeController = GetComponent<TimeController>();
            if (_timeController == null) {
                _timeController = gameObject.AddComponent<TimeController>();
            }
        }

        // Same idea as the TimeController: shared screen-fading infrastructure every game gets.
        private void EnsureScreenFader() {
            _screenFader = GetComponent<ScreenFader>();
            if (_screenFader == null) {
                _screenFader = gameObject.AddComponent<ScreenFader>();
            }
        }

        // Same again: every game plays sound effects, so every game gets the shared audio voices.
        private void EnsureAudioController() {
            _audioController = GetComponent<SoundPlayer>();
            if (_audioController == null) {
                _audioController = gameObject.AddComponent<SoundPlayer>();
            }
        }

        private void SpawnPlayer() {
            if (_playerPrefab == null) {
                Debug.LogWarning($"SessionController on '{gameObject.name}' has no Player Prefab assigned.", this);
                return;
            }

            Vector3 position = _playerSpawnAnchor != null ? _playerSpawnAnchor.position : transform.position;
            GameObject instance = Instantiate(_playerPrefab, position, Quaternion.identity);

            _player = instance.GetComponent<PlayerController>();
            if (_player == null) {
                Debug.LogWarning($"SessionController on '{gameObject.name}': Player Prefab has no PlayerController component.", this);
                return;
            }

            _player.LoadState();
        }
    }
}
