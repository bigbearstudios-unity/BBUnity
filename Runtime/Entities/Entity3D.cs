using UnityEngine;

namespace BBUnity.Entities {

    /// <summary>
    /// The concrete, addable Entity variant for 3D games. Vampire Empire itself is 2D-only —
    /// this exists so the framework isn't 2D-locked for other BBUnity-based projects.
    /// </summary>
    [AddComponentMenu("BBUnity/Entities/Entity 3D")]
    public class Entity3D : Entity {

    }
}
