using UnityEngine;

namespace BBUnity.Entities.Building {

    /// <summary>
    /// A designer-picked reference to a Component-derived type, stored as its
    /// AssemblyQualifiedName — same approach as
    /// BBUnity.Statistics.StatisticTypeReference, generalized from
    /// "any EntityStatistic" to "any Component". Used wherever the entity-building system needs
    /// to say "add this component" or "this needs to be a subclass of this" without hardcoding
    /// the type in code.
    /// </summary>
    [System.Serializable]
    public class ComponentTypeReference {

        [SerializeField]
        private string _typeName;

        public System.Type ResolveType() {
            if (string.IsNullOrEmpty(_typeName)) { return null; }

            System.Type type = System.Type.GetType(_typeName);
            if (type == null) {
                Debug.LogWarning($"ComponentTypeReference could not resolve '{_typeName}' — the type may have been renamed or removed since this was picked. Re-pick it in the Inspector.");
            }

            return type;
        }
    }
}
