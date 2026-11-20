using UnityEngine;

namespace BBUnity.World {

    /// <summary>
    /// How LevelBuilder creates the objects it places. At runtime that is plain Instantiate; the
    /// editor supplies one that creates real prefab instances and records undo.
    /// </summary>
    public interface IElementInstantiator {
        GameObject Instantiate(GameObject prefab, Vector3 position, Transform parent);
        GameObject CreateEmpty(string name, Transform parent);
    }
}
