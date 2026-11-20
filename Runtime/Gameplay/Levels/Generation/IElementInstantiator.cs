using UnityEngine;

namespace BBUnity.Gameplay.Levels.Generation {

    /// <summary>
    /// How LevelBuilder creates the objects it places. At runtime that is plain Instantiate; the
    /// editor supplies one that creates real prefab instances and records undo.
    /// </summary>
    public interface IElementInstantiator {
        GameObject Instantiate(GameObject prefab, Vector3 position, Transform parent);
        GameObject CreateEmpty(string name, Transform parent);
    }

    public class RuntimeElementInstantiator : IElementInstantiator {

        public GameObject Instantiate(GameObject prefab, Vector3 position, Transform parent) {
            return Object.Instantiate(prefab, position, Quaternion.identity, parent);
        }

        public GameObject CreateEmpty(string name, Transform parent) {
            var created = new GameObject(name);
            created.transform.SetParent(parent, false);
            return created;
        }
    }
}
