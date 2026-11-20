using UnityEngine;

using BBUnity.Gameplay.Entities.Controllers;

namespace BBUnity.Gameplay.Controllers {

    /// <summary>
    /// Lives on the root of the Player prefab — the stable handle the rest of the game looks up
    /// via Current rather than needing a reference threaded through from SessionController
    /// everywhere (a Level repositioning the Player at a PlayerSpawnNode, UI binding to it, ...).
    ///
    /// LoadState() is called once by SessionController right after this Player is instantiated —
    /// it's an extension point, not an implementation: save-file format is entirely game-specific,
    /// so this framework has no opinion on it and does nothing by default. Usable as-is; a game
    /// subclasses it only once it has a save system to load from.
    /// </summary>
    [AddComponentMenu("BBUnity/Gameplay/Controllers/Player Controller")]
    public class PlayerController : MonoBehaviour {

        public static PlayerController Current { get; private set; }

        protected virtual void Awake() {
            Current = this;
        }

        protected virtual void OnDestroy() {
            if (Current == this) { Current = null; }
        }

        public virtual void LoadState() { }

        /// <summary>
        /// Puts the Player at `position` with no leftover momentum. Goes through the
        /// PlayerMovementController (whichever implementation the Player has) when there is one,
        /// since it owns the Player's velocity.
        /// </summary>
        public virtual void TeleportTo(Vector3 position) {
            PlayerMovementController movement = GetComponent<PlayerMovementController>();
            if (movement != null) {
                movement.Teleport(position);
                return;
            }

            transform.position = position;
        }
    }
}
