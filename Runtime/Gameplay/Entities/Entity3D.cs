using UnityEngine;

namespace BBUnity.Gameplay.Entities {

    /// <summary>
    /// The concrete, addable Entity variant for 3D games. Vampire Empire itself is 2D-only —
    /// this exists so the framework isn't 2D-locked for other BBUnity-based projects.
    /// </summary>
    [AddComponentMenu("BBUnity/Gameplay/Entities/Entity 3D")]
    public class Entity3D : Entity {

    }
}
