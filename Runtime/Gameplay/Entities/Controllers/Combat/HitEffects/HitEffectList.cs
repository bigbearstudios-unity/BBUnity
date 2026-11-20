using UnityEngine;

using System.Collections.Generic;

namespace BBUnity.Gameplay.Entities.Controllers.Combat.HitEffects {

    /// <summary>
    /// A serialized list of HitEffects, played together. A class of its own (rather than a bare
    /// List field) so the Inspector can attach an "Add Effect" type picker to it — see
    /// HitEffectListDrawer.
    /// </summary>
    [System.Serializable]
    public class HitEffectList {

        [SerializeReference]
        private List<HitEffect> _effects = new List<HitEffect>();

        public int Count {
            get { return _effects.Count; }
        }

        public HitEffect this[int index] {
            get { return _effects[index]; }
        }

        public void Add(HitEffect effect) {
            if (effect != null) { _effects.Add(effect); }
        }

        public void Play(HitContext context) {
            foreach (HitEffect effect in _effects) {
                effect?.Play(context);
            }
        }
    }
}
