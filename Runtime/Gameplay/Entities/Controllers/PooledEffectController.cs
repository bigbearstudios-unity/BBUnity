
using UnityEngine;

using System.Collections.Generic;

using BBUnity.Gameplay.Effects;
using BBUnity.Pools;

namespace BBUnity.Gameplay.Entities.Controllers {

    /// <summary>
    /// Low-level controller for pooled/positioned spawning — deliberately generic (no combat
    /// awareness) so it stays usable on its own. Effects is a catalog of named, pre-registered
    /// prefab+position pairs (e.g. for Animation Event-driven VFX via InstantiateEffect); Spawn()
    /// is the lower-level primitive underneath it for anything that already has its own prefab
    /// and position and just wants pooling (see StatisticCombatController.PerformAttack).
    /// </summary>
    [AddComponentMenu("BBUnity/Gameplay/Entities/Controllers/Pooled Effect Controller")]
    public class PooledEffectController : EntityController {

        [SerializeField]
        private List<EffectReference> _effects = new List<EffectReference>();

        private readonly Dictionary<string, EffectReference> _internalEffects = new Dictionary<string, EffectReference>();
        private ObjectPool _pool;

        private void Awake() {
            ObjectPool pool = GetOrCreatePool();

            foreach (EffectReference effectReference in _effects) {
                if (effectReference.Name == null) {
                    Debug.LogError($"PooledEffectController on '{gameObject.name}' has an EffectReference with no name set; it will be skipped.", this);
                    continue;
                }

                if (effectReference.Prefab == null) {
                    Debug.LogError($"PooledEffectController on '{gameObject.name}' has no prefab assigned for effect '{effectReference.Name}'; it will be skipped.", this);
                    continue;
                }

                if (pool.FindPoolReference(effectReference.Name) == null) {
                    pool.AddPoolReference(new ObjectPoolReference(effectReference.Name, effectReference.Prefab, 1, 100));
                }

                _internalEffects.Add(effectReference.Name, effectReference);
            }
        }

        public GameObject InstantiateEffect(string name) {
            if (!_internalEffects.TryGetValue(name, out EffectReference reference)) {
                Debug.LogError($"PooledEffectController on '{gameObject.name}' has no registered effect named '{name}'.", this);
                return null;
            }

            return Spawn(name, reference.Prefab, reference.Position.position);
        }

        public GameObject InstantiateEffect(string name, Vector3 position) {
            if (!_internalEffects.TryGetValue(name, out EffectReference reference)) {
                Debug.LogError($"PooledEffectController on '{gameObject.name}' has no registered effect named '{name}'.", this);
                return null;
            }

            return Spawn(name, reference.Prefab, position);
        }

        /// <summary>
        /// Pooled spawn for a prefab that isn't necessarily catalogd in Effects — finds or
        /// creates a pool reference under `name` (creating one on first use) and spawns from it.
        /// </summary>
        public GameObject Spawn(string name, GameObject prefab, Vector3 position) {
            ObjectPool pool = GetOrCreatePool();

            ObjectPoolReference poolReference = pool.FindPoolReference(name);
            if (poolReference == null) {
                poolReference = new ObjectPoolReference(name, prefab, 1, 100);
                pool.AddPoolReference(poolReference);
            }

            PoolBehavior obj = poolReference.Spawn();
            obj.transform.position = position;
            obj.transform.localScale = gameObject.transform.localScale;

            return obj.gameObject;
        }

        private ObjectPool GetOrCreatePool() {
            if (_pool != null) { return _pool; }

            _pool = ObjectPool.FindInScene("Effect Pool");
            if (_pool == null) {
                Debug.Log($"PooledEffectController on '{gameObject.name}' found no existing \"Effect Pool\" in the scene; creating one.", this);
                _pool = Utilities.GameObjectFactory.Create("Effect Pool", components: new[] { typeof(ObjectPool) }).GetComponent<ObjectPool>();
            }

            return _pool;
        }
    }
}
