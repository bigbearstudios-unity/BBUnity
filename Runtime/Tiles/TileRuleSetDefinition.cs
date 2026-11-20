using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace BBUnity.Tiles {

    /// <summary>
    /// An ordered list of TileRules for one kind of cell (ground, walls, platforms): given what
    /// surrounds a cell, it says which of Unity's tiles to draw. Built by hand in the Tile Rule
    /// Editor, or generated from a standard tile layout (see TileRuleGenerator).
    /// </summary>
    [CreateAssetMenu(fileName = "New Tile Rule Set", menuName = "BBUnity/Tiles/Tile Rule Set Definition")]
    public class TileRuleSetDefinition : ScriptableObject {

        [SerializeField]
        private List<TileRule> _rules = new List<TileRule>();

        [SerializeField, Tooltip("Used when no rule matches.")]
        private TileBase _fallbackTile;

        [SerializeField, Tooltip("Treat cells beyond the edge of the level as the same type, so the level border draws as solid rather than as an exposed edge.")]
        private bool _outOfBoundsCountsAsSame = true;

        public IReadOnlyList<TileRule> Rules {
            get { return _rules; }
        }

        public TileBase FallbackTile {
            get { return _fallbackTile; }
        }

        public bool OutOfBoundsCountsAsSame {
            get { return _outOfBoundsCountsAsSame; }
        }

        /// <summary>Replaces the rules (for example with a generated set).</summary>
        public void SetRules(IEnumerable<TileRule> rules, TileBase fallbackTile) {
            _rules = new List<TileRule>(rules);
            _fallbackTile = fallbackTile;
        }

        /// <summary>The index of the first rule that matches these surroundings and produces a tile, or -1.</summary>
        public int FindRuleIndex(int sameMask) {
            for (int i = 0; i < _rules.Count; i++) {
                if (_rules[i] != null && _rules[i].Matches(sameMask) && _rules[i].Pick(0f) != null) { return i; }
            }

            return -1;
        }

        /// <summary>The tile for a cell with these surroundings: the first matching rule's, else the fallback.</summary>
        public TileBase Resolve(int sameMask, float roll) {
            foreach (TileRule rule in _rules) {
                if (rule != null && rule.Matches(sameMask)) {
                    TileBase tile = rule.Pick(roll);
                    if (tile != null) { return tile; }
                }
            }

            return _fallbackTile;
        }
    }
}
