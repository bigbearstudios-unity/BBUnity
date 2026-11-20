using UnityEngine;
using UnityEngine.Serialization;

using System.Collections.Generic;

namespace BBUnity.Statistics {

    /// <summary>
    /// A serialized list of EntityStatistics, for data assets (see CombatantDefinition) that hold
    /// statistics outside an StatisticController. A class of its own so the Inspector can attach
    /// an "Add Statistic" type picker to it — see EntityStatisticListDrawer.
    /// </summary>
    [System.Serializable]
    public class EntityStatisticList {

        [SerializeReference]
        [FormerlySerializedAs("_attributes")]
        private List<EntityStatistic> _statistics = new List<EntityStatistic>();

        public int Count {
            get { return _statistics.Count; }
        }

        public EntityStatistic this[int index] {
            get { return _statistics[index]; }
        }
    }
}
