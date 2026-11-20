using UnityEngine;
using System.Collections.Generic;

using BBUnity;

namespace BBUnity.Pools {

    /// <summary>
    ///
    /// </summary>
    [System.Serializable]
    public class ObjectPoolReference {
        public delegate void SpawnedEventHandler(PoolBehavior poolBehavior);

        [SerializeField, Tooltip("The name of the Definition. This is used to access the definition, defaults to the prefab name")]
        private string _name = null;

        [SerializeField, Tooltip("The prefab which will be instantiated")]
        private GameObject _prefab = null;

        [SerializeField, Tooltip("The starting size of the pool. This will be filled with instantiated prefabs")]
        private int _startingSize = 0;

        [SerializeField, Tooltip("The maximum size of the pool.")]
        private int _maximumSize = 10;

        private List<PoolBehavior> _instances = null;
        public event SpawnedEventHandler Spawned;

        public string Name {
            get { return (_name != null && _name.Length > 0) ? _name : _prefab?.name ?? "(unnamed, no prefab)"; }
            set { _name = value; }
        }

        public GameObject Prefab {
            get { return _prefab; }
            set { _prefab = value; }
        }

        public bool HasPrefab {
            get { return _prefab != null; }
        }

        public int StartingSize {
            get { return _startingSize; }
            set { _startingSize = value; }
        }

        public int MaximumSize {
            get { return _maximumSize; }
            set { SetMaximumSize(value); }
        }

        public bool AllowGrowth {
            get { return NumberOfInstances < _maximumSize; }
        }

        public int NumberOfInstances {
            get { return _instances != null ? _instances.Count : 0; }
        }

        /// <summary>
        /// The number of instances currently spawned (active) out of this pool.
        /// </summary>
        public int ActiveCount {
            get {
                if (_instances == null) { return 0; }

                int count = 0;
                foreach (PoolBehavior instance in _instances) {
                    if (instance != null && instance.gameObject.activeSelf) { count++; }
                }

                return count;
            }
        }

        /// <summary>
        /// The number of instances currently available to be spawned.
        /// </summary>
        public int InactiveCount {
            get { return NumberOfInstances - ActiveCount; }
        }

        /// <summary>
        /// True once this pool has created more instances than its configured StartingSize.
        /// Useful for spotting an undersized StartingSize during development.
        /// </summary>
        public bool HasGrownBeyondStartingSize {
            get { return NumberOfInstances > _startingSize; }
        }

        public bool Valid {
            get { return _prefab != null; }
        }

        public bool Invalid {
            get { return !Valid; }
        }

        public ObjectPoolReference(GameObject prefab, int startingSize = 1, int maximumSize = 50) {
            SetName(prefab.name);
            SetPrefab(prefab);
            SetStartingSize(startingSize);
            SetMaximumSize(maximumSize);
        }

        public ObjectPoolReference(string name, GameObject prefab, int startingSize = 1, int maximumSize = 50) {
            SetName(name);
            SetPrefab(prefab);
            SetStartingSize(startingSize);
            SetMaximumSize(maximumSize);
        }

        public void SetName(string name) {
            _name = name;
        }

        public void SetPrefab(GameObject prefab) {
            _prefab = prefab;
        }

        public void SetStartingSize(int size) {
            _startingSize = size;
        }

        public void SetMaximumSize(int size) {
            if (size <= 0) {
                Debug.LogWarning($"ObjectPoolReference '{Name}': MaximumSize set to {size}, which will prevent this pool from ever growing.");
            }

            _maximumSize = size;
        }

        private PoolBehavior CreateInstance() {
            PoolBehavior poolBehavior = Utilities.GameObjectFactory.InstantiatedGameObjectWithComponent<PoolBehavior>(_prefab);

            poolBehavior._OnCreate(this);

            _instances.Add(poolBehavior);

            return poolBehavior;
        }

        /// <summary>
        /// Performs first-time initialization, filling the pool up to StartingSize. This is a
        /// one-time setup step, not a reset — calling it again after the pool has already been
        /// initialized is a no-op. See ClearAll() to return active instances to the pool.
        /// </summary>
        public void InitializeInstances() {
            if(_instances != null) { return; }

            _instances = new List<PoolBehavior>(_startingSize);
            for(int i = 0; i < _startingSize; i++) {
                CreateInstance();
            }
        }

        internal PoolBehavior GetOrCreateInstance() {
            foreach(PoolBehavior instance in _instances) {
                if(!instance.gameObject.activeSelf) { return instance; }
            }

            if(AllowGrowth) {
                return CreateInstance();
            }

            return null;
        }

        /// <summary>
        /// Removes an instance from this pool's tracking, e.g. because it was destroyed rather
        /// than returned. Called internally by PoolBehavior.OnDestroy().
        /// </summary>
        internal void RemoveInstance(PoolBehavior instance) {
            _instances?.Remove(instance);
        }

        internal void _InvokeOnSpawnEvent(PoolBehavior poolBehavior) {
            Spawned?.Invoke(poolBehavior);
        }

        /*
         * Public Spawn Methods
         */

        /// <summary>
        /// Spawns the PoolBehavior, will call the underlying Spawned if set and also
        /// poolBehavior.OnSpawn. Returns null if the pool is already at MaximumSize and no
        /// inactive instance is available to reuse — callers must null-check the result.
        /// </summary>
        public PoolBehavior Spawn() {
            PoolBehavior poolBehavior = GetOrCreateInstance();
            if(poolBehavior != null) {
                poolBehavior.OnSpawnInternal();
            }

            return poolBehavior;
        }

        /// <summary>
        /// Returns every currently-active instance in this pool back to it, ready to be
        /// respawned. Useful at scene transitions or level resets.
        /// </summary>
        public void ClearAll() {
            if (_instances == null) { return; }

            // Copy first: ReturnToPool() deactivates the GameObject, which must not happen
            // while we're mid-iteration over the same backing list.
            List<PoolBehavior> toReturn = new List<PoolBehavior>();
            foreach (PoolBehavior instance in _instances) {
                if (instance != null && instance.gameObject.activeSelf) {
                    toReturn.Add(instance);
                }
            }

            foreach (PoolBehavior instance in toReturn) {
                instance.ReturnToPool();
            }
        }
    }
}
