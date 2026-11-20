using System.Collections.Generic;
using UnityEngine;

using BBUnity.EditorAttributes;

namespace BBUnity.Gameplay.Entities.Building {

    /// <summary>
    /// "Create (or reuse) a child object named X, with these components on it" — e.g. a
    /// "Hurtbox" child carrying an AttackReceiver and a trigger Collider2D on its own physics
    /// layer, or a "Visual" child carrying a SpriteRenderer and SpriteAnimator. Looked up by name
    /// across the whole EntityDefinition build, so a ComponentSpec elsewhere (even from a different
    /// EntityCapabilityDefinition) can wire a field to whatever object this spec ends up creating.
    /// </summary>
    [System.Serializable]
    public class ChildObjectSpec {

        [SerializeField, Tooltip("Name of the child object. If a child with this name already exists under the entity, it's reused rather than duplicated.")]
        private string _name;

        [SerializeField]
        private List<ComponentSpec> _components = new List<ComponentSpec>();

        [SerializeField, Tooltip("Sets this child's GameObject layer once created.")]
        private bool _setLayer;

        [SerializeField, Layer, Tooltip("Only used when Set Layer is on.")]
        private int _layer;

        public string Name {
            get { return _name; }
        }

        public IReadOnlyList<ComponentSpec> Components {
            get { return _components; }
        }

        public bool SetLayer {
            get { return _setLayer; }
        }

        public int Layer {
            get { return _layer; }
        }
    }
}
