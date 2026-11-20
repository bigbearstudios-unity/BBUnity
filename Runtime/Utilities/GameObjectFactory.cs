using System;
using UnityEngine;

namespace BBUnity.Utilities {

    public static class GameObjectFactory {
        public static GameObject Create(string name, Transform parent = null, Type[] components = null) {
            if (name == null) throw new ArgumentNullException(nameof(name), "BBUnity GameObjectFactory.Create: 'name' must not be null.");

            GameObject obj = components != null
                ? new GameObject(name, components)
                : new GameObject(name);

            if (parent != null) {
                // worldPositionStays: false places the object at the parent's local origin.
                obj.transform.SetParent(parent, false);
                obj.transform.localPosition = Vector3.zero;
            } else {
                obj.transform.position = Vector3.zero;
            }

            obj.transform.localScale = Vector3.one;
            return obj;
        }

        public static T InstantiatedGameObjectWithComponent<T>(GameObject prefab, Transform parentTransform = null) where T : Component {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab), "BBUnity GameObjectFactory.InstantiatedGameObjectWithComponent: 'prefab' must not be null.");

            GameObject obj = UnityEngine.GameObject.Instantiate<GameObject>(prefab, parentTransform);
            return AddOrGetComponent<T>(obj);
        }

        public static T AddOrGetComponent<T>(GameObject obj) where T : Component {
            if (obj == null) throw new ArgumentNullException(nameof(obj), "BBUnity GameObjectFactory.AddOrGetComponent: 'obj' must not be null.");
            return obj.GetComponent<T>() ?? obj.AddComponent<T>();
        }
    }
}
