using UnityEngine;
using UnityEditor;
using BBUnity.World;


namespace BBUnity.Editor.Generation {

    /// <summary>
    /// Creates the objects of a generated level in the editor: prefab instances that stay linked to
    /// their prefab, with the creation recorded for undo.
    /// </summary>
    internal class EditorElementInstantiator : IElementInstantiator {

        public GameObject Instantiate(GameObject prefab, Vector3 position, Transform parent) {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = position;
            Undo.RegisterCreatedObjectUndo(instance, "Build Generated Level");
            return instance;
        }

        public GameObject CreateEmpty(string name, Transform parent) {
            var created = new GameObject(name);
            created.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(created, "Build Generated Level");
            return created;
        }
    }
}
