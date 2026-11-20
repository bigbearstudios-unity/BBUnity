using UnityEngine;

namespace BBUnity.Gameplay.Entities.Building {

    /// <summary>
    /// The BoxCollider2D settings worth setting from a definition — trigger/size/offset/edge radius.
    /// Supersedes the old per-ChildObjectSpec "force IsTrigger" convenience: with this, a definition
    /// can describe a collider fully rather than just its trigger flag.
    /// </summary>
    [System.Serializable]
    public class BoxCollider2DSettings : IComponentSettings {

        [SerializeField]
        private bool _isTrigger;

        [SerializeField]
        private Vector2 _size = Vector2.one;

        [SerializeField]
        private Vector2 _offset = Vector2.zero;

        [SerializeField, Tooltip("Rounds the box's corners — 0 is a sharp-cornered box.")]
        private float _edgeRadius = 0f;

        public void Apply(Component instance) {
            if (instance is not BoxCollider2D collider) { return; }

            collider.isTrigger = _isTrigger;
            collider.size = _size;
            collider.offset = _offset;
            collider.edgeRadius = _edgeRadius;
        }
    }
}
