using UnityEngine;

using System.Collections.Generic;

namespace BBUnity.Statistics.Effectors {

    /// <summary>
    /// The serialized list of an statistic's authored effectors. A class of its own (rather than a
    /// bare List field) so the Inspector can attach an "Add Effector" type picker to it — see
    /// StatisticEffectorListDrawer.
    /// </summary>
    [System.Serializable]
    public class StatisticEffectorList {

        [SerializeReference]
        private List<StatisticEffector> _effectors = new List<StatisticEffector>();

        public int Count {
            get { return _effectors.Count; }
        }

        public StatisticEffector this[int index] {
            get { return _effectors[index]; }
        }

        public void Add(StatisticEffector effector) {
            if (effector != null) { _effectors.Add(effector); }
        }
    }
}
