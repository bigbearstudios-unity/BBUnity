using UnityEngine;

using BBUnity.Gameplay.Controllers;

using BBUnity.Support.Scenes;
namespace BBUnity.Gameplay.Levels {

    /// <summary>
    /// A point that moves the player to a PlayerSpawnNode in another scene. The target spawn node
    /// can't be a direct object reference — Unity has no way to reference an object in a scene
    /// that isn't currently loaded — so it's stored as an id, meant to be resolved against
    /// whichever PlayerSpawnNode in the target scene has a matching Id once that scene is loaded.
    /// See LevelTransitionNodeEditor for how the target scene's node list is picked from at
    /// edit time.
    ///
    /// Needs a Collider2D set to Is Trigger on the same GameObject — when the Player walks into
    /// it, SessionController loads the target scene additively, moves the Player to the target
    /// spawn node, and unloads the scene this node lives in. The GameObject's layer must be one
    /// the Player's layer collides with in the Physics 2D collision matrix — a trigger on a layer
    /// that doesn't interact with the Player's never fires.
    /// </summary>
    [AddComponentMenu("BBUnity/Gameplay/Levels/Level Transition Node")]
    public class LevelTransitionNode : MonoBehaviour {

        [SerializeField]
        private SceneReference _targetScene;

        [SerializeField]
        private string _targetSpawnNodeId;

        public SceneReference TargetScene {
            get { return _targetScene; }
        }

        public string TargetSpawnNodeId {
            get { return _targetSpawnNodeId; }
        }

        private void Reset() {
            Collider2D trigger = GetComponent<Collider2D>();
            if (trigger != null) { trigger.isTrigger = true; }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.GetComponentInParent<PlayerController>() == null) { return; }

            SessionController gameplay = SessionController.Current;
            if (gameplay == null) { return; }

            gameplay.TransitionToLevel(_targetScene, _targetSpawnNodeId, gameObject.scene);
        }
    }
}
