using System;
using System.Collections.Generic;
using UnityEngine;

using BBUnity.Animation2D.Exceptions;
namespace BBUnity.Animation2D {

    [AddComponentMenu("BBUnity/Animation2D/Sprite Animator")]
    public class SpriteAnimator : MonoBehaviour {

        public enum OnLoopType {
            Loop,
            Stop,
            DisableMonoBehaviour,
            DeactivateGameObject
        }

        public delegate void AnimationCompletedEventHandler(SpriteAnimator spriteAnimator);
        public delegate void AnimationFrameChangedEventHandler(SpriteAnimator spriteAnimator, int currentFrame);

        [SerializeField, Tooltip("The frames per second at which the animation will run (must be greater than 0)")]
        private int _framesPerSecond = 60;

        [SerializeField, Tooltip("Multiplier applied to playback speed at runtime (must be greater than 0)")]
        private float _speedMultiplier = 1.0f;

        [SerializeField, Tooltip("Sets if the animation should start when Start is called")]
        private bool _playOnStart = true;

        [SerializeField, Tooltip("Sets if the animation should start when OnEnable is called")]
        private bool _playOnEnable = true;

        [SerializeField, Tooltip("Sets if the animation should restart from frame 0 when OnEnable is called")]
        private bool _restartOnEnable = true;

        [SerializeField, Tooltip("Sets the action which should take place upon the animation completing")]
        private OnLoopType _onLoop = OnLoopType.Loop;

        [SerializeField, Tooltip("The frames which will make up the animation")]
        private Sprite[] _frames = null;

        private SpriteRenderer _spriteRenderer;
        private bool _isPlaying = false;
        private int _currentFrame = 0;
        private int _segmentStart = 0;
        private int _segmentEnd = -1;  // -1 means use full array
        private float _timePerFrame, _lastFrameChange = 0.0f;

        // Shared buffer avoids per-Awake allocation when discovering ISpriteAnimator components.
        private static readonly List<ISpriteAnimator> _callbackBuffer = new List<ISpriteAnimator>();

        public event AnimationCompletedEventHandler AnimationCompleted;
        public event AnimationCompletedEventHandler AnimationLooped;
        public event AnimationFrameChangedEventHandler AnimationFrameChanged;

        public bool IsPlaying { get { return _isPlaying; } }
        public bool PlayOnEnable { get { return _playOnEnable; } }
        public bool RestartOnEnable { get { return _restartOnEnable; } set { _restartOnEnable = value; } }
        public OnLoopType OnLoop { get { return _onLoop; } }

        public float SpeedMultiplier {
            get { return _speedMultiplier; }
            set {
                if(value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "BBUnity SpriteAnimator: SpeedMultiplier must be greater than 0.");
                _speedMultiplier = value;
            }
        }

        public int CurrentFrame { get { return _currentFrame; } }
        public bool IsComplete { get { return _frames != null && _currentFrame == ActiveEndFrame; } }

        public Sprite[] Frames {
            get { return (Sprite[])_frames?.Clone(); }
        }

        private int ActiveEndFrame {
            get { return (_segmentEnd >= 0 && _segmentEnd < _frames.Length) ? _segmentEnd : _frames.Length - 1; }
        }

        /*
         * Unity Overrides
         */

        protected virtual void Awake() {
            FindSpriteRenderer();
            AssignCallbackInterfaceEvents();
            CalculateTimePerFrame();
        }

        protected virtual void Start() {
            SetPlaying(_playOnStart);
        }

        protected virtual void OnEnable() {
            if(_frames == null || _frames.Length == 0) {
                if(_playOnEnable || _restartOnEnable) {
                    Debug.LogWarning("BBUnity SpriteAnimator: 'Play On Enable' or 'Restart On Enable' is set but no frames are assigned. Assign sprites in the Inspector before enabling.", this);
                }
                return;
            }

            if(_restartOnEnable) { ResetAnimation(); }
            SetPlaying(_playOnEnable);
        }

        protected virtual void Update() {
            if(IsPlaying) {
                _lastFrameChange += Time.deltaTime * _speedMultiplier;
                if(_lastFrameChange >= _timePerFrame) {
                    _lastFrameChange %= _timePerFrame;
                    IncrementCurrentFrame();
                }
            }
        }

        /*
         * Private Methods
         */

        private void FindSpriteRenderer() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if(_spriteRenderer == null) {
                throw new SpriteAnimatorRendererNotFoundException();
            }
        }

        private void ValidateFrames() {
            if(_frames == null || _frames.Length == 0) {
                throw new SpriteAnimatorNoFramesException();
            }
        }

        private void ValidateSpriteRenderer() {
            if(_spriteRenderer == null) {
                Debug.LogError("BBUnity SpriteAnimator: SpriteRenderer has been destroyed. Re-attach a SpriteRenderer component.", this);
            }
        }

        private void AssignCallbackInterfaceEvents() {
            _callbackBuffer.Clear();
            GetComponents<ISpriteAnimator>(_callbackBuffer);
            foreach(ISpriteAnimator behavior in _callbackBuffer) {
                AnimationCompleted += behavior.OnAnimationComplete;
                AnimationLooped += behavior.OnAnimationLoop;
                AnimationFrameChanged += behavior.OnAnimationChangedFrame;
            }
            _callbackBuffer.Clear();
        }

        private void ChangeFrame(int frame) {
            _currentFrame = frame;
            if(_spriteRenderer == null) {
                ValidateSpriteRenderer();
                return;
            }
            _spriteRenderer.sprite = _frames[_currentFrame];
            AnimationFrameChanged?.Invoke(this, _currentFrame);
        }

        private void CalculateTimePerFrame() {
            if(_framesPerSecond <= 0) {
                Debug.LogError("BBUnity SpriteAnimator: _framesPerSecond must be greater than 0. Defaulting to 1.", this);
                _framesPerSecond = 1;
            }
            _timePerFrame = 1.0f / _framesPerSecond;
        }

        private void ResetAnimation() {
            ValidateFrames();
            if(_spriteRenderer == null) {
                ValidateSpriteRenderer();
                return;
            }
            _currentFrame = _segmentStart;
            _spriteRenderer.sprite = _frames[_currentFrame];
        }

        private void HandleAnimationCompleted() {
            // Every completion path resets the frame back to the start of the active segment,
            // set directly (not via ChangeFrame()) so AnimationFrameChanged never fires
            // at a completion boundary.
            _currentFrame = _segmentStart;
            if(_spriteRenderer != null) { _spriteRenderer.sprite = _frames[_currentFrame]; }

            if(_onLoop == OnLoopType.Loop) {
                AnimationLooped?.Invoke(this);
            } else {
                AnimationCompleted?.Invoke(this);
                if(_onLoop == OnLoopType.Stop) {
                    StopAnimation();
                } else if(_onLoop == OnLoopType.DisableMonoBehaviour) {
                    enabled = false;
                } else if(_onLoop == OnLoopType.DeactivateGameObject) {
                    gameObject.SetActive(false);
                }
            }
        }

        /*
         * Public Methods
         */

        public void SetPlaying(bool playing) {
            if(playing) { ValidateFrames(); }
            _isPlaying = playing;
        }

        /// <summary>
        /// Starts the animation from the beginning of the full frame array.
        /// </summary>
        public void Play() {
            ValidateFrames();
            _segmentStart = 0;
            _segmentEnd = -1;
            _currentFrame = 0;
            if(_spriteRenderer != null) { _spriteRenderer.sprite = _frames[0]; }
            _isPlaying = true;
        }

        /// <summary>
        /// Starts the animation playing a subset of frames from startFrame to endFrame (inclusive).
        /// </summary>
        public void Play(int startFrame, int endFrame) {
            ValidateFrames();
            if(startFrame < 0 || startFrame >= _frames.Length) throw new SpriteAnimatorInvalidFrameException();
            if(endFrame < startFrame || endFrame >= _frames.Length) throw new SpriteAnimatorInvalidFrameException();
            _segmentStart = startFrame;
            _segmentEnd = endFrame;
            _currentFrame = startFrame;
            if(_spriteRenderer != null) { _spriteRenderer.sprite = _frames[_currentFrame]; }
            _isPlaying = true;
        }

        /// <summary>
        /// Stops the animation. Use ResumeAnimation() to continue from the current frame.
        /// </summary>
        public void StopAnimation() {
            _isPlaying = false;
        }

        /// <summary>
        /// Pauses the animation on the current frame. Use ResumeAnimation() to continue.
        /// </summary>
        public void PauseAnimation() {
            _isPlaying = false;
        }

        /// <summary>
        /// Resumes playback from the current frame without resetting position.
        /// </summary>
        public void ResumeAnimation() {
            if(_frames == null || _frames.Length == 0) {
                Debug.LogWarning("BBUnity SpriteAnimator: Cannot resume — no frames have been set.", this);
                return;
            }
            _isPlaying = true;
        }

        /// <summary>
        /// Advances the animation by one frame. Can be called manually to drive the animator externally.
        /// </summary>
        public void IncrementCurrentFrame() {
            if(_frames == null || _frames.Length == 0) { return; }
            int endFrame = ActiveEndFrame;
            if(_currentFrame > endFrame) { _currentFrame = endFrame; }
            if(IsComplete) {
                HandleAnimationCompleted();
            } else {
                ChangeFrame(_currentFrame + 1);
            }
        }

        public void SetOnLoop(OnLoopType onLoop) {
            _onLoop = onLoop;
        }

        /// <summary>
        /// Sets the animation to a specific frame index.
        /// </summary>
        public void SetCurrentFrame(int frame) {
            ValidateFrames();
            if(frame < 0 || frame > _frames.Length - 1) {
                throw new SpriteAnimatorInvalidFrameException();
            }
            ChangeFrame(frame);
        }

        /// <summary>
        /// Replaces the entire frames array. Throws if called while the animation is playing.
        /// </summary>
        public void SetFrames(Sprite[] sprites, bool resetAnimation = true) {
            if(_isPlaying) { throw new SpriteAnimatorImmutableWhilePlayingException(); }
            if(sprites == null) throw new ArgumentNullException(nameof(sprites), "BBUnity SpriteAnimator: 'sprites' must not be null.");
            if(sprites.Length == 0) throw new SpriteAnimatorNoFramesException();
            _frames = (Sprite[])sprites.Clone();
            if(resetAnimation) { ResetAnimation(); }
        }

        /// <summary>
        /// Registers a listener to receive animation callbacks. Use this for components added after
        /// Awake() — they will not be discovered by the automatic GetComponents search.
        /// </summary>
        public void Register(ISpriteAnimator listener) {
            if(listener == null) throw new ArgumentNullException(nameof(listener), "BBUnity SpriteAnimator: 'listener' must not be null.");
            AnimationCompleted += listener.OnAnimationComplete;
            AnimationLooped += listener.OnAnimationLoop;
            AnimationFrameChanged += listener.OnAnimationChangedFrame;
        }

        /// <summary>
        /// Unregisters a listener from all animation callbacks.
        /// </summary>
        public void Unregister(ISpriteAnimator listener) {
            if(listener == null) throw new ArgumentNullException(nameof(listener), "BBUnity SpriteAnimator: 'listener' must not be null.");
            AnimationCompleted -= listener.OnAnimationComplete;
            AnimationLooped -= listener.OnAnimationLoop;
            AnimationFrameChanged -= listener.OnAnimationChangedFrame;
        }

        /*
         * Editor
         */

        private void OnValidate() {
            if(_framesPerSecond <= 0) {
                Debug.LogWarning("BBUnity SpriteAnimator: Frames Per Second must be greater than 0. Reset to 1.", this);
                _framesPerSecond = 1;
            }
            if(_speedMultiplier <= 0) {
                Debug.LogWarning("BBUnity SpriteAnimator: Speed Multiplier must be greater than 0. Reset to 1.", this);
                _speedMultiplier = 1.0f;
            }
            CalculateTimePerFrame();
        }
    }
}
