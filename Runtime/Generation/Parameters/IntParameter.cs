using System;

using UnityEngine;

using BBUnity.Generation.Pipeline;

namespace BBUnity.Generation.Parameters {

    public enum ParameterMode {
        Fixed,
        Range
    }

    /// <summary>
    /// A whole-number generation setting that is either one fixed value (4 rooms, handy for
    /// testing) or a range (6 to 10 rooms) picked from the seeded random each time a level is
    /// generated.
    /// </summary>
    [Serializable]
    public struct IntParameter {

        [SerializeField]
        private ParameterMode _mode;

        [SerializeField]
        private int _fixed;

        [SerializeField]
        private int _min;

        [SerializeField]
        private int _max;

        public static IntParameter Fixed(int value) {
            return new IntParameter { _mode = ParameterMode.Fixed, _fixed = value, _min = value, _max = value };
        }

        public static IntParameter Range(int min, int max) {
            return new IntParameter { _mode = ParameterMode.Range, _fixed = min, _min = min, _max = max };
        }

        public ParameterMode Mode {
            get { return _mode; }
        }

        public int Min {
            get { return _mode == ParameterMode.Fixed ? _fixed : Mathf.Min(_min, _max); }
        }

        public int Max {
            get { return _mode == ParameterMode.Fixed ? _fixed : Mathf.Max(_min, _max); }
        }

        public int Resolve(GenerationRandom random) {
            return _mode == ParameterMode.Fixed ? _fixed : random.Range(Min, Max);
        }
    }
}
