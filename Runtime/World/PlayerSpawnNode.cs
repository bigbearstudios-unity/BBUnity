using UnityEngine;
using UnityEngine.SceneManagement;

namespace BBUnity.World {

    /// <summary>
    /// A single player spawn point — this GameObject's own Transform is the spawn location. Id is
    /// how a LevelTransitionNode in another scene refers to this one: Unity can't hold a direct
    /// object reference to something in a scene that isn't loaded, so the reference is by name
    /// instead (see LevelTransitionNode).
    /// </summary>
    [AddComponentMenu("BBUnity/World/Player Spawn Node")]
    public class PlayerSpawnNode : MonoBehaviour {

        [SerializeField, Tooltip("How a LevelTransitionNode in another scene refers to this spawn point. Defaults to this GameObject's name if left blank.")]
        private string _id;

        public string Id {
            get { return string.IsNullOrEmpty(_id) ? gameObject.name : _id; }
        }

        /// <summary>
        /// The spawn node in `scene` with the given id — or the first one found if `id` is empty.
        /// Null if the scene has none.
        /// </summary>
        public static PlayerSpawnNode Find(Scene scene, string id = null) {
            if (!scene.IsValid() || !scene.isLoaded) { return null; }

            foreach (GameObject root in scene.GetRootGameObjects()) {
                foreach (PlayerSpawnNode node in root.GetComponentsInChildren<PlayerSpawnNode>(true)) {
                    if (string.IsNullOrEmpty(id) || node.Id == id) { return node; }
                }
            }

            return null;
        }
    }
}
