using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BBUnity.Gameplay.Entities.Building {

    /// <summary>
    /// A named, project-specific composition of EntityCapabilityDefinitions — "Enemy", "Projectile",
    /// "Shopkeeper NPC", whatever a given game needs. This is the thing the "Entity Type"
    /// dropdown lists (every EntityDefinition asset found in the project, via AssetDatabase — never
    /// a hardcoded enum), and the thing new archetypes are added as: an asset, not a code change.
    /// Components/ChildObjects exist for the odd one-off that doesn't earn its own reusable
    /// EntityCapabilityDefinition — e.g. a Rigidbody2D specific to this one definition's physics needs.
    /// </summary>
    [CreateAssetMenu(fileName = "New Entity Definition", menuName = "BBUnity/Gameplay/Entities/Building/Entity Definition")]
    public class EntityDefinition : ScriptableObject {

        [SerializeField, Tooltip("Shown in the Entity Type dropdown, and used for \"{EntityName}\" substitution in any SubclassSlot name templates.")]
        private string _displayName;

        [SerializeField]
        [FormerlySerializedAs("_blocks")]
        private List<EntityCapabilityDefinition> _capabilities = new List<EntityCapabilityDefinition>();

        [SerializeField, Tooltip("Root components specific to this definition alone, not worth their own EntityCapabilityDefinition.")]
        private List<ComponentSpec> _components = new List<ComponentSpec>();

        [SerializeField, Tooltip("Child objects specific to this definition alone, not worth their own EntityCapabilityDefinition.")]
        private List<ChildObjectSpec> _childObjects = new List<ChildObjectSpec>();

        public string DisplayName {
            get { return string.IsNullOrEmpty(_displayName) ? name : _displayName; }
        }

        public IReadOnlyList<EntityCapabilityDefinition> Capabilities {
            get { return _capabilities; }
        }

        public IReadOnlyList<ComponentSpec> Components {
            get { return _components; }
        }

        public IReadOnlyList<ChildObjectSpec> ChildObjects {
            get { return _childObjects; }
        }

        /// <summary>
        /// Every SubclassSlot across every capability in this definition — what the entity-building
        /// inspector needs to offer "use existing / create new subclass" for once the structural
        /// build (see EntityBuilder) is done.
        /// </summary>
        public IEnumerable<SubclassSlot> AllSubclassSlots() {
            foreach (EntityCapabilityDefinition capability in _capabilities) {
                if (capability == null) { continue; }
                foreach (SubclassSlot slot in capability.SubclassSlots) {
                    yield return slot;
                }
            }
        }
    }
}
