using BBUnity.Statistics.Base;


namespace BBUnity.Statistics.Generic {

    /// <summary>
    /// A generic, designer-named statistic that does support effectors — a "Stamina" or "Rage"
    /// that regenerates, drains or has its maximum modified, without needing a dedicated
    /// Specialized type of its own. Read back with StatisticController.Get("Stamina").
    /// </summary>
    [System.Serializable]
    [System.ComponentModel.DisplayName("Dynamic Integer Statistic")]
    public class NamedDynamicIntegerValue : EntityStatistic<DynamicIntegerStatistic> {

        public override bool AllowsMultiple {
            get { return true; }
        }
    }
}
