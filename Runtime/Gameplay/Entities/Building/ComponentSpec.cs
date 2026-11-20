using System.Collections.Generic;
using UnityEngine;

namespace BBUnity.Gameplay.Entities.Building {

    /// <summary>
    /// "Add this component, then wire these fields on it" — the smallest unit EntityCapabilityDefinition and
    /// EntityDefinition are made of. Field wiring is resolved after the whole definition has been built
    /// (see EntityBuilder), so a ComponentSpec can safely reference a child object created by a
    /// completely different capability.
    /// </summary>
    [System.Serializable]
    public class ComponentSpec {

        [SerializeField, Tooltip("The component to add — must derive from UnityEngine.Component.")]
        private ComponentTypeReference _type;

        [SerializeField]
        private List<FieldWiring> _fieldWirings = new List<FieldWiring>();

        // Populated automatically (see ComponentTypeReferenceDrawer, Editor) when Type is set to
        // something with a registered settings class — e.g. picking Rigidbody2D fills this with
        // a Rigidbody2DSettings. Null for any type with no registered settings, which is most of
        // them; nothing requires every ComponentSpec to have one.
        [SerializeReference]
        private IComponentSettings _settings;

        public System.Type Type {
            get { return _type?.ResolveType(); }
        }

        public IReadOnlyList<FieldWiring> FieldWirings {
            get { return _fieldWirings; }
        }

        public IComponentSettings Settings {
            get { return _settings; }
        }
    }
}
