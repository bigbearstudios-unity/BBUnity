using System;

using UnityEngine;

using BBUnity.Generation.Pipeline;

namespace BBUnity.Generation.Parameters {

    /// <summary>The decimal counterpart of IntParameter: one fixed value, or a range picked per generation.</summary>
    [Serializable]
    public struct FloatParameter {

        [SerializeField]
        private ParameterMode _mode;

        [SerializeField]
        private float _fixed;

        [SerializeField]
        private float _min;

        [SerializeField]
        private float _max;

        public static FloatParameter Fixed(float value) {
            return new FloatParameter { _mode = ParameterMode.Fixed, _fixed = value, _min = value, _max = value };
        }

        public static FloatParameter Range(float min, float max) {
            return new FloatParameter { _mode = ParameterMode.Range, _fixed = min, _min = min, _max = max };
        }

        public ParameterMode Mode {
            get { return _mode; }
        }

        public float Min {
            get { return _mode == ParameterMode.Fixed ? _fixed : Mathf.Min(_min, _max); }
        }

        public float Max {
            get { return _mode == ParameterMode.Fixed ? _fixed : Mathf.Max(_min, _max); }
        }

        public float Resolve(GenerationRandom random) {
            return _mode == ParameterMode.Fixed ? _fixed : random.Range(Min, Max);
        }
    }
}
