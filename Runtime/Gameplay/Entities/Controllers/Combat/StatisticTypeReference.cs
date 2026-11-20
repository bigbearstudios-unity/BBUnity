using UnityEngine;
using UnityEngine.Serialization;

using BBUnity.Gameplay.Entities.Controllers.Statistics;

namespace BBUnity.Gameplay.Entities.Controllers.Combat {

    /// <summary>
    /// A designer-picked reference to an EntityStatistic type (e.g. HealthValue), stored as
    /// its AssemblyQualifiedName and resolved against a specific StatisticController at runtime
    /// via StatisticController.GetStatistic(Type). Used anywhere combat data needs to say "read
    /// this from a named statistic" — see DamageValue/Reduction. Edited via
    /// StatisticTypeReferenceDrawer, which reuses the same type picker as StatisticController's
    /// own "Add Statistic" dropdown.
    /// </summary>
    [System.Serializable]
    public class StatisticTypeReference {

        [SerializeField]
        [FormerlySerializedAs("_attributeTypeName")]
        private string _statisticTypeName;

        public EntityStatistic Resolve(StatisticController controller) {
            System.Type type = ResolveType();
            return type == null ? null : controller.GetStatistic(type);
        }

        /// <summary>
        /// The picked type itself, without resolving it against any particular controller — used
        /// where the type needs to travel from one controller to another (e.g. HitInfo, built by
        /// the attacker but applied by the defender).
        /// </summary>
        public System.Type ResolveType() {
            if (string.IsNullOrEmpty(_statisticTypeName)) { return null; }

            System.Type type = System.Type.GetType(_statisticTypeName);
            if (type == null) {
                Debug.LogWarning($"StatisticTypeReference could not resolve '{_statisticTypeName}' — the statistic type may have been renamed or removed since this was picked. Re-pick it in the Inspector.");
            }

            return type;
        }
    }
}
