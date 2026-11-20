using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace BBUnity.Tiles {

    /// <summary>One tile (or one of several, by weight) a rule can produce.</summary>
    [Serializable]
    public struct WeightedTile {

        [SerializeField]
        private TileBase _tile;

        [SerializeField, Min(0f)]
        private float _weight;

        public TileBase Tile {
            get { return _tile; }
        }

        public float Weight {
            get { return _weight; }
        }

        public WeightedTile(TileBase tile, float weight = 1f) {
            _tile = tile;
            _weight = weight;
        }
    }

    /// <summary>
    /// "When the cells around a tile look like this, use one of these tiles." Each of the eight
    /// neighbours is Any, Same (same cell type as the tile) or Different (empty or another type).
    /// This is BBUnity's own replacement for Unity's rule tiles: rules live in a
    /// TileRuleSetDefinition, are matched top to bottom, and the first match wins.
    /// </summary>
    [Serializable]
    public class TileRule {

        [SerializeField]
        private string _name = "Rule";

        [SerializeField]
        private NeighborRequirement[] _neighbors = new NeighborRequirement[TileNeighbors.Count];

        [SerializeField]
        private List<WeightedTile> _outputs = new List<WeightedTile>();

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public IReadOnlyList<WeightedTile> Outputs {
            get { return _outputs; }
        }

        public TileRule() { }

        public TileRule(string name, TileBase tile) {
            _name = name;
            if (tile != null) { _outputs.Add(new WeightedTile(tile)); }
        }

        public NeighborRequirement Get(TileNeighbor neighbor) {
            EnsureNeighbors();
            return _neighbors[(int)neighbor];
        }

        public TileRule Require(TileNeighbor neighbor, NeighborRequirement requirement) {
            EnsureNeighbors();
            _neighbors[(int)neighbor] = requirement;
            return this;
        }

        public TileRule AddOutput(TileBase tile, float weight = 1f) {
            _outputs.Add(new WeightedTile(tile, weight));
            return this;
        }

        /// <summary>
        /// True if a tile whose surroundings are described by `sameMask` (a bit per TileNeighbor,
        /// set where that neighbour is the same cell type) satisfies every requirement.
        /// </summary>
        public bool Matches(int sameMask) {
            EnsureNeighbors();

            for (int i = 0; i < TileNeighbors.Count; i++) {
                bool same = (sameMask & (1 << i)) != 0;

                if (_neighbors[i] == NeighborRequirement.Same && !same) { return false; }
                if (_neighbors[i] == NeighborRequirement.Different && same) { return false; }
            }

            return true;
        }

        /// <summary>The output chosen by `roll` (0 up to 1), so the same cell always gets the same tile.</summary>
        public TileBase Pick(float roll) {
            float total = 0f;
            foreach (WeightedTile output in _outputs) {
                if (output.Tile != null) { total += output.Weight; }
            }

            if (total <= 0f) {
                foreach (WeightedTile output in _outputs) {
                    if (output.Tile != null) { return output.Tile; }
                }

                return null;
            }

            float target = roll * total;
            TileBase last = null;
            foreach (WeightedTile output in _outputs) {
                if (output.Tile == null) { continue; }

                last = output.Tile;
                target -= output.Weight;
                if (target < 0f) { return output.Tile; }
            }

            return last;
        }

        private void EnsureNeighbors() {
            if (_neighbors == null || _neighbors.Length != TileNeighbors.Count) {
                Array.Resize(ref _neighbors, TileNeighbors.Count);
            }
        }
    }
}
