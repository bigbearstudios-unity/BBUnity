using UnityEngine;
using BBUnity.Controllers;


namespace BBUnity.World {

    /// <summary>
    /// The main level manager — deliberately minimal for now. Will grow into the place that holds
    /// references to everything else loaded in a level (enemies, spawn/transition nodes, ...).
    ///
    /// For now it does one job: when the level is entered directly (pressing Play on the scene, or
    /// the first level of a session) rather than through a LevelTransitionNode, it puts the
    /// Player at this level's default PlayerSpawnNode instead of leaving it wherever
    /// SessionController's own anchor is. Entering through a transition is handled by the
    /// transition itself, so this stays out of its way.
    /// </summary>
    [AddComponentMenu("BBUnity/World/Level")]
    public class Level : MonoBehaviour {

        [SerializeField, Tooltip("The PlayerSpawnNode id the Player is placed at when this level is entered directly. The first PlayerSpawnNode in the scene is used if left blank.")]
        private string _defaultSpawnNodeId;

        private void Start() {
            SessionController gameplay = SessionController.Current;
            if (gameplay == null || gameplay.Player == null || gameplay.IsTransitioning) { return; }

            PlayerSpawnNode node = PlayerSpawnNode.Find(gameObject.scene, _defaultSpawnNodeId);
            if (node == null) { return; }

            gameplay.Player.TeleportTo(node.transform.position);
        }
    }
}
