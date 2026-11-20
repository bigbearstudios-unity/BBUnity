using UnityEngine;
using UnityEngine.SceneManagement;

namespace BBUnity.Gameplay.Levels {

    /// <summary>
    /// A single enemy spawn point — this GameObject's own Transform is the spawn location. Which
    /// enemy to spawn is picked from every Enemy-marked prefab in the project (see
    /// EnemySpawnNodeEditor), not dragged in by hand.
    ///
    /// The enemy is spawned once, when the level's scene starts, into the same scene as the node
    /// (so it goes when the level is unloaded). It is then dropped onto the ground directly below
    /// the node: a ray is cast down against GroundLayers and the enemy is placed so the bottom of
    /// its collider rests on whatever it hits. If nothing is below the node it is left exactly
    /// where the node is.
    ///
    /// This is a deliberately simple first version — it spawns on level start and nothing else.
    /// </summary>
    [AddComponentMenu("BBUnity/Gameplay/Levels/Enemy Spawn Node")]
    public class EnemySpawnNode : MonoBehaviour {

        // Gap left between the enemy's feet and the ground, so it doesn't start overlapping it.
        private const float GroundClearance = 0.01f;

        [SerializeField]
        private Enemy _enemy;

        [SerializeField, Tooltip("What counts as ground when placing the enemy. Triggers are always ignored.")]
        private LayerMask _groundLayers = ~0;

        [SerializeField, Min(0f), Tooltip("How far below the node, in units, to look for ground.")]
        private float _maxGroundDistance = 20f;

        private GameObject _spawned;

        public Enemy Enemy {
            get { return _enemy; }
        }

        /// <summary>
        /// The enemy this node spawned, or null before it has spawned.
        /// </summary>
        public GameObject Spawned {
            get { return _spawned; }
        }

        private void Start() {
            Spawn();
        }

        /// <summary>
        /// Spawns the enemy and rests it on the ground below. Does nothing if this node has
        /// already spawned one.
        /// </summary>
        public GameObject Spawn() {
            if (_spawned != null) { return _spawned; }

            if (_enemy == null) {
                Debug.LogWarning($"EnemySpawnNode '{gameObject.name}' has no Enemy picked — nothing was spawned.", this);
                return null;
            }

            _spawned = Instantiate(_enemy.gameObject, transform.position, Quaternion.identity);
            _spawned.name = _enemy.gameObject.name;

            // Instantiate puts it in the active scene, which may not be this node's level.
            if (_spawned.scene != gameObject.scene) {
                SceneManager.MoveGameObjectToScene(_spawned, gameObject.scene);
            }

            PlaceOnGround(_spawned);
            return _spawned;
        }

        private void PlaceOnGround(GameObject instance) {
            Collider2D body = FindBodyCollider(instance);
            if (body == null) {
                Debug.LogWarning($"EnemySpawnNode '{gameObject.name}': '{instance.name}' has no Collider2D to rest on the ground — it was left at the node.", this);
                return;
            }

            Physics2D.SyncTransforms();

            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = false;
            filter.SetLayerMask(_groundLayers);

            RaycastHit2D[] hits = new RaycastHit2D[8];
            int count = Physics2D.Raycast(transform.position, Vector2.down, filter, hits, _maxGroundDistance);

            for (int i = 0; i < count; i++) {
                if (hits[i].collider.transform.IsChildOf(instance.transform)) { continue; }

                float feetOffset = instance.transform.position.y - body.bounds.min.y;
                Vector3 position = instance.transform.position;
                position.y = hits[i].point.y + feetOffset + GroundClearance;
                instance.transform.position = position;

                Physics2D.SyncTransforms();
                return;
            }

            Debug.LogWarning($"EnemySpawnNode '{gameObject.name}' found no ground within {_maxGroundDistance} units below it — '{instance.name}' was left at the node.", this);
        }

        // The collider whose bottom marks the enemy's feet: the one on its root (a character's
        // body collider is often a trigger, with a Rigidbody2D moving it, so triggers count), then
        // any solid collider below it, then any collider at all.
        private static Collider2D FindBodyCollider(GameObject instance) {
            Collider2D own = instance.GetComponent<Collider2D>();
            if (own != null) { return own; }

            Collider2D[] all = instance.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D candidate in all) {
                if (!candidate.isTrigger) { return candidate; }
            }

            return all.Length > 0 ? all[0] : null;
        }
    }
}
