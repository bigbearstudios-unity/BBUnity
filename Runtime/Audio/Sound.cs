using UnityEngine;
using UnityEngine.Audio;
using BBUnity.Controllers;


namespace BBUnity.Audio {

    /// <summary>
    /// Everything about how one sound plays: which clips (one is picked at random each time), how
    /// loud, how much its pitch varies, which mixer group it belongs to, and how many copies of it
    /// may sound at once. Plain data, so it can be authored inline wherever it is used or kept in a
    /// SoundDefinition asset to share; see SoundReference.
    ///
    /// A Sound never plays itself from the object that triggered it. It is handed to the
    /// SoundPlayer (Play), which owns the audio sources — so it keeps sounding after whatever
    /// triggered it has been pooled, disabled or destroyed.
    /// </summary>
    [System.Serializable]
    public class Sound {

        [SerializeField, Tooltip("One of these is chosen at random each time the sound plays.")]
        private AudioClip[] _clips = new AudioClip[0];

        [SerializeField, Range(0f, 1f)]
        private float _volume = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("How far the volume may vary either way on each play, as a fraction of Volume.")]
        private float _volumeVariation = 0f;

        [SerializeField, Range(0.1f, 3f)]
        private float _pitch = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("How far the pitch may vary either way on each play, as a fraction of Pitch.")]
        private float _pitchVariation = 0f;

        [SerializeField, Range(0f, 1f), Tooltip("0 plays the same everywhere (right for a 2D game); 1 is fully positional.")]
        private float _spatialBlend = 0f;

        [SerializeField, Tooltip("Optional. Falls back to the SoundPlayer's default group.")]
        private AudioMixerGroup _mixerGroup;

        [SerializeField, Min(1), Tooltip("How many copies of this sound may play at once. Further plays are dropped until one finishes, which stops a crowd of identical hits piling into noise.")]
        private int _maxVoices = 3;

        public int MaxVoices {
            get { return _maxVoices; }
        }

        public float SpatialBlend {
            get { return _spatialBlend; }
        }

        public AudioMixerGroup MixerGroup {
            get { return _mixerGroup; }
        }

        public bool HasClips {
            get { return _clips != null && _clips.Length > 0; }
        }

        public AudioClip PickClip() {
            if (!HasClips) { return null; }

            return _clips[Random.Range(0, _clips.Length)];
        }

        public float PickVolume() {
            return Mathf.Clamp01(_volume * (1f + Random.Range(-_volumeVariation, _volumeVariation)));
        }

        public float PickPitch() {
            return Mathf.Max(0.01f, _pitch * (1f + Random.Range(-_pitchVariation, _pitchVariation)));
        }

        /// <summary>
        /// Plays it through the SessionController's SoundPlayer. Returns false if nothing
        /// played (no clips, no controller, or too many copies already sounding).
        /// </summary>
        public bool Play(Vector3 position) {
            SessionController gameplay = SessionController.Current;
            if (gameplay == null || gameplay.SoundPlayer == null) { return false; }

            return gameplay.SoundPlayer.Play(this, position);
        }
    }
}
