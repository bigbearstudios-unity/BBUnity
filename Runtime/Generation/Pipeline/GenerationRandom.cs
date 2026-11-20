using System;
using System.Collections.Generic;

namespace BBUnity.Generation.Pipeline {

    /// <summary>
    /// The seeded random source for generation. It is a small self-contained generator (SplitMix64)
    /// rather than System.Random so the same seed gives the same level on every platform and
    /// Unity version. Fork gives a step or parameter its own independent stream, so changing one
    /// part of a template doesn't reshuffle everything generated after it.
    /// </summary>
    public sealed class GenerationRandom {

        private ulong _state;

        public int Seed { get; }

        public GenerationRandom(int seed) {
            Seed = seed;
            _state = (ulong)(uint)seed ^ 0x9E3779B97F4A7C15UL;
        }

        /// <summary>An independent stream derived from this seed and a label (not from how much has been drawn).</summary>
        public GenerationRandom Fork(string label) {
            unchecked {
                uint hash = 2166136261;
                foreach (char c in label) { hash = (hash ^ c) * 16777619; }

                return new GenerationRandom((int)(hash ^ (uint)Seed * 2654435761u));
            }
        }

        private ulong NextUInt64() {
            unchecked {
                _state += 0x9E3779B97F4A7C15UL;
                ulong z = _state;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }

        /// <summary>A whole number from min to max, both included.</summary>
        public int Range(int min, int max) {
            if (max < min) { (min, max) = (max, min); }

            ulong span = (ulong)((long)max - min + 1);
            return (int)(min + (long)(NextUInt64() % span));
        }

        /// <summary>A number from 0 up to (not including) 1.</summary>
        public float Value() {
            return (NextUInt64() >> 40) / (float)(1 << 24);
        }

        public float Range(float min, float max) {
            return min + (max - min) * Value();
        }

        public bool Chance(float probability) {
            return Value() < probability;
        }

        public T Pick<T>(IReadOnlyList<T> items) {
            return items.Count == 0 ? default : items[Range(0, items.Count - 1)];
        }

        public T PickWeighted<T>(IReadOnlyList<T> items, Func<T, float> weight) {
            float total = 0f;
            foreach (T item in items) { total += Math.Max(0f, weight(item)); }
            if (total <= 0f) { return Pick(items); }

            float roll = Range(0f, total);
            foreach (T item in items) {
                roll -= Math.Max(0f, weight(item));
                if (roll <= 0f) { return item; }
            }

            return items[items.Count - 1];
        }

        public void Shuffle<T>(IList<T> items) {
            for (int i = items.Count - 1; i > 0; i--) {
                int j = Range(0, i);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }
    }
}
