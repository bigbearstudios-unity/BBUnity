using UnityEngine;

namespace BBUnity.Spawning {

    /// <summary>
    /// Marks a prefab as a selectable enemy for EnemySpawnNode's dropdown (see
    /// EnemySpawnNodeEditor) — deliberately minimal for now, just an identity marker.
    /// </summary>
    [AddComponentMenu("BBUnity/Spawning/Enemy")]
    public class Enemy : MonoBehaviour {

    }
}
