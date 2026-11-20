using UnityEngine;

namespace BBUnity.Pools {

    /// <summary>
    ///
    /// </summary>
    [AddComponentMenu("BBUnity/Pools/Pool Behavior")]
    public class PoolBehavior : MonoBehaviour {

        public delegate void CreatedEventHandler(PoolBehavior poolBehavior);
        public delegate void SpawnedEventHandler(PoolBehavior poolBehavior);
        public delegate void ReturnedEventHandler(PoolBehavior poolBehavior);

        private ObjectPoolReference _poolReference = null;

        // Prefer IPoolBehavior for logic that lives on the pooled prefab itself (see
        // IPoolBehavior for why). These events are for external code that calls
        // ObjectPool.Spawn()/PoolBehavior.ReturnToPool() without owning the prefab.
        public event CreatedEventHandler Created;
        public event SpawnedEventHandler Spawned;
        public event ReturnedEventHandler Returned;

        /// <summary>
        /// Called internally upon creation.
        /// </summary>
        internal void _OnCreate(ObjectPoolReference poolReference) {
            _poolReference = poolReference;

            IPoolBehavior[] callbacks = GetComponents<IPoolBehavior>();
            foreach(IPoolBehavior behavior in callbacks) {
                Created += behavior.OnCreate;
                Spawned += behavior.OnSpawn;
                Returned += behavior.OnReturn;
            }

            gameObject.SetActive(false);
            CallOnCreateCallbacks();
        }

        /// <summary>
        /// Called internally upon Spawn
        /// </summary>
        internal void OnSpawnInternal() {
            gameObject.SetActive(true);
            CallOnSpawnCallbacks();
        }

        /// <summary>
        /// Returns this instance to its pool, ready to be reused by a future Spawn() call.
        /// </summary>
        public void ReturnToPool() {
            CallOnReturnCallbacks();
            gameObject.SetActive(false);
        }

        private void OnDestroy() {
            _poolReference?.RemoveInstance(this);
        }

        private void CallOnCreateCallbacks() {
            OnCreate();
            Created?.Invoke(this);
        }

        /// <summary>
        /// Calls all of the OnSpawn callbacks in the following order:
        /// - virtual OnSpawn
        /// - Spawned
        /// - poolDefinition.Spawned
        /// </summary>
        private void CallOnSpawnCallbacks() {
            OnSpawn();
            Spawned?.Invoke(this);
            _poolReference._InvokeOnSpawnEvent(this);
        }

        /// <summary>
        /// Calls all of the OnReturn callbacks in the following order:
        /// - virtual OnReturn
        /// - Returned
        /// </summary>
        private void CallOnReturnCallbacks() {
            OnReturn();
            Returned?.Invoke(this);
        }

        /// <summary>
        /// Can be overridden on the sub class to handle the OnCreate event.
        /// Please note all of the Created Events will still be triggered
        /// even when this method is overridden
        /// </summary>
        public virtual void OnCreate() {}

        /// <summary>
        /// Can be overridden on the sub class to handle the OnSpawn event
        /// Please note all of the Spawned Events will still be triggered
        /// even when this method is overridden
        /// </summary>
        public virtual void OnSpawn() {}

        /// <summary>
        /// Can be overridden on the sub class to handle the OnReturn event (see ReturnToPool()).
        /// Please note all of the Returned Events will still be triggered
        /// even when this method is overridden
        /// </summary>
        public virtual void OnReturn() {}

        static public void AddOnCreateEvent(MonoBehaviour behavior, CreatedEventHandler handler) {
            PoolBehavior poolBehavior = behavior.GetComponent<PoolBehavior>();
            if(poolBehavior == null) {
                throw new MissingComponentException($"AddOnCreateEvent requires a PoolBehavior component on '{behavior.gameObject.name}'.");
            }

            poolBehavior.Created += handler;
        }

        static public void AddOnSpawnEvent(MonoBehaviour behavior, SpawnedEventHandler handler) {
            PoolBehavior poolBehavior = behavior.GetComponent<PoolBehavior>();
            if(poolBehavior == null) {
                throw new MissingComponentException($"AddOnSpawnEvent requires a PoolBehavior component on '{behavior.gameObject.name}'.");
            }

            poolBehavior.Spawned += handler;
        }
    }
}
