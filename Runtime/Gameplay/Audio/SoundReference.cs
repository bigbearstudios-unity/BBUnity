using UnityEngine;

using BBUnity.Gameplay.Entities.Controllers.Combat.HitEffects;
namespace BBUnity.Gameplay.Audio {

    /// <summary>
    /// "A sound", authored either inline or as a shared SoundDefinition asset — if a Definition is
    /// assigned it is used, otherwise the inline Sound is. Everything that plays a sound (a
    /// SoundEffect, a PooledSoundController entry) holds one of these, so the inline-first, extract-to-an-
    /// asset-later workflow is the same everywhere.
    /// </summary>
    [System.Serializable]
    public class SoundReference {

        [SerializeField, Tooltip("Optional. A shared sound; when set it is used instead of the inline sound below.")]
        private SoundDefinition _definition;

        [SerializeField]
        private Sound _inline = new Sound();

        public Sound Sound {
            get { return _definition != null ? _definition.Sound : _inline; }
        }

        public bool Play(Vector3 position) {
            return Sound.Play(position);
        }
    }
}
