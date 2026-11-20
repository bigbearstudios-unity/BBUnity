using UnityEngine;
using UnityEngine.Audio;

using BBUnity.Gameplay.Audio;

namespace BBUnity.Gameplay.Audio {

    /// <summary>
    /// The one place sound effects are played from — a fixed pool of AudioSources on the persistent
    /// Gameplay object, reached through SessionController.Current.SoundPlayer (added
    /// automatically, like the TimeController). Because the sources belong to this rather than to
    /// whatever triggered the sound, a sound is never cut off by its trigger being pooled, disabled
    /// or destroyed, and the number of voices — and of copies of any one sound — is capped in one
    /// place. Looping or stateful sounds (an ambient loop) are a different job and stay on an
    /// AudioSource on their own object.
    ///
    /// When every voice is busy the oldest sound is cut short to make room.
    /// </summary>
    [AddComponentMenu("BBUnity/Gameplay/Audio/Sound Player")]
    public class SoundPlayer : MonoBehaviour {

        [SerializeField, Min(1), Tooltip("How many sounds can play at once.")]
        private int _voices = 16;

        [SerializeField, Tooltip("Used by any Sound that doesn't set its own mixer group.")]
        private AudioMixerGroup _defaultMixerGroup;

        private AudioSource[] _sources;
        private Sound[] _playing;
        private float[] _startedAt;

        private void Awake() {
            _sources = new AudioSource[_voices];
            _playing = new Sound[_voices];
            _startedAt = new float[_voices];

            for (int i = 0; i < _voices; i++) {
                var voice = new GameObject($"Voice {i}");
                voice.transform.SetParent(transform, false);

                AudioSource source = voice.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _sources[i] = source;
            }
        }

        /// <summary>
        /// Plays the sound at a position (only audible as a position if its Spatial Blend is above
        /// zero). Returns false if nothing played: it has no clips, or too many copies of it are
        /// already sounding.
        /// </summary>
        public bool Play(Sound sound, Vector3 position) {
            if (sound == null || _sources == null) { return false; }

            AudioClip clip = sound.PickClip();
            if (clip == null) { return false; }

            int copies = 0;
            int free = -1;
            int oldest = 0;

            for (int i = 0; i < _sources.Length; i++) {
                if (!_sources[i].isPlaying) {
                    if (free < 0) { free = i; }
                    continue;
                }

                if (_playing[i] == sound) { copies++; }
                if (_startedAt[i] < _startedAt[oldest]) { oldest = i; }
            }

            if (copies >= sound.MaxVoices) { return false; }

            int index = free >= 0 ? free : oldest;
            AudioSource source = _sources[index];

            source.Stop();
            source.transform.position = position;
            source.clip = clip;
            source.volume = sound.PickVolume();
            source.pitch = sound.PickPitch();
            source.spatialBlend = sound.SpatialBlend;
            source.outputAudioMixerGroup = sound.MixerGroup != null ? sound.MixerGroup : _defaultMixerGroup;
            source.Play();

            _playing[index] = sound;
            _startedAt[index] = Time.unscaledTime;

            return true;
        }
    }
}
