using UnityEngine;

namespace BBUnity.Gameplay.Entities.Building {

    /// <summary>
    /// Marks a point in a EntityCapabilityDefinition where the real component is expected to be a
    /// project-specific subclass of a BBUnity base type — e.g. EnemyAIController is meant to be
    /// subclassed per game, not used directly. EntityBuilder does not create a component for a
    /// SubclassSlot itself; it's surfaced separately by the entity-building inspector, which
    /// offers either an existing subclass already in the project or generating a new one (see
    /// SubclassGenerator).
    /// </summary>
    [System.Serializable]
    public class SubclassSlot {

        [SerializeField, Tooltip("The BBUnity base type this slot expects a subclass of (e.g. EnemyAIController).")]
        private ComponentTypeReference _baseType;

        [SerializeField, Tooltip("Suggested class name for a newly-generated subclass. \"{EntityName}\" is replaced with the definition/entity's display name — e.g. \"{EntityName}Controller\" for a \"Goblin\" entity suggests \"GoblinController\".")]
        private string _nameTemplate = "{EntityName}Controller";

        public System.Type BaseType {
            get { return _baseType?.ResolveType(); }
        }

        public string NameTemplate {
            get { return _nameTemplate; }
        }

        public string SuggestedName(string entityName) {
            return _nameTemplate.Replace("{EntityName}", entityName ?? string.Empty);
        }
    }
}
