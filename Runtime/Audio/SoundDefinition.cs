using UnityEngine;

namespace BBUnity.Audio {

    /// <summary>
    /// A Sound kept in an asset, so several things (every skeleton's hit, every footstep) can share
    /// one. Reference it from a SoundReference; set the sound inline there instead when it is only
    /// used in one place.
    /// </summary>
    [CreateAssetMenu(fileName = "Sound_", menuName = "BBUnity/Audio/Sound Definition")]
    public class SoundDefinition : ScriptableObject {

        [SerializeField]
        private Sound _sound = new Sound();

        public Sound Sound {
            get { return _sound; }
        }
    }
}
