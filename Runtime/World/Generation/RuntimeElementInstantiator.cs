using UnityEngine;

namespace BBUnity.World.Generation {

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
