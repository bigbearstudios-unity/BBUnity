using UnityEngine;

using System.Collections.Generic;

using BBUnity.Gameplay.Audio;

namespace BBUnity.Gameplay.Entities.Controllers {

    /// <summary>
    /// An entity's named sounds, played from Animation Events: put this next to the Animator and
    /// add an event calling PlaySound with the sound's name (a sword swing on the swing frame, a
    /// footstep on the foot-down frame). It only chooses which sound; playback is the
    /// SoundPlayer's, so a sound still finishes if the entity is pooled or destroyed straight
    /// after.
    /// </summary>
    [AddComponentMenu("BBUnity/Gameplay/Entities/Controllers/Pooled Sound Controller")]
    public class PooledSoundController : MonoBehaviour {

        [System.Serializable]
        private class NamedSound {

            [SerializeField, Tooltip("What an Animation Event passes to PlaySound.")]
            private string _name;

            [SerializeField]
            private SoundReference _sound = new SoundReference();

            public string Name {
                get { return _name; }
            }

            public SoundReference Sound {
                get { return _sound; }
            }
        }

        [SerializeField]
        private List<NamedSound> _sounds = new List<NamedSound>();

        /// <summary>
        /// Plays the sound with the given name at this entity's position. Animation Event target.
        /// </summary>
        public void PlaySound(string name) {
            NamedSound entry = _sounds.Find(s => s.Name == name);
            if (entry == null) {
                Debug.LogWarning($"PooledSoundController on '{gameObject.name}' has no sound named '{name}'.", this);
                return;
            }

            entry.Sound.Play(transform.position);
        }
    }
}
