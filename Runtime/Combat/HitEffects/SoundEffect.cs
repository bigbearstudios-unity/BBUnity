using UnityEngine;
using BBUnity.Audio;


namespace BBUnity.Combat.HitEffects {

    /// <summary>
    /// Plays a sound at the point of contact when a hit lands. The sound belongs to the hit event
    /// (it sits in the same effect list as the spark that goes with it), not to the spark's
    /// prefab, and is played by the SoundPlayer — so it carries on after the spark has
    /// finished and been returned to its pool.
    /// </summary>
    [System.Serializable]
    public class SoundEffect : HitEffect {

        [SerializeField]
        private SoundReference _sound = new SoundReference();

        public override void Play(HitContext context) {
            _sound.Play(context.Point);
        }
    }
}
