using BBUnity.Statistics.Generic;


namespace BBUnity.Statistics.Specialized {
    
    [System.Serializable]
    public class Health : DynamicIntegerStatistic {

        public bool Dead { get { return _current <= 0; } }
    }
}