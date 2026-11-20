using System.Collections.Generic;

namespace BBUnity.Generation.Parameters {

    /// <summary>
    /// Values that replace named parameters for one run — how a test pins "rooms = 4" without
    /// changing the template. Keys are the names steps resolve their parameters under (see
    /// GenerationContext.Resolve).
    /// </summary>
    public class ParameterOverrideSet {

        private readonly Dictionary<string, int> _ints = new Dictionary<string, int>();
        private readonly Dictionary<string, float> _floats = new Dictionary<string, float>();

        public void SetInt(string key, int value) {
            _ints[key] = value;
        }

        public void SetFloat(string key, float value) {
            _floats[key] = value;
        }

        public bool TryGetInt(string key, out int value) {
            return _ints.TryGetValue(key, out value);
        }

        public bool TryGetFloat(string key, out float value) {
            return _floats.TryGetValue(key, out value);
        }

        public void Remove(string key) {
            _ints.Remove(key);
            _floats.Remove(key);
        }

        public void Clear() {
            _ints.Clear();
            _floats.Clear();
        }
    }
}
