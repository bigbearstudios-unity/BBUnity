using UnityEngine;
using BBUnity.Entities;

namespace BBUnity.Movement {

    /// <summary>
    /// The contract the Player's movement implementation fulfills, so the rest of the framework
    /// (PlayerController, levels, transitions) can move the Player without knowing how it
    /// moves. PlatformingPlayerMovementController is the side-on platformer implementation; a
    /// top-down or other style of game adds its own subclass.
    /// </summary>
    public abstract class PlayerMovementController : EntityController {

        /// <summary>
        /// The Player's current velocity.
        /// </summary>
        public abstract Vector2 Velocity { get; }

        /// <summary>
        /// Moves the Player to `position` instantly and kills all of its momentum — for spawning
        /// at a PlayerSpawnNode or crossing a scene transition.
        /// </summary>
        public abstract void Teleport(Vector2 position);
    }
}
