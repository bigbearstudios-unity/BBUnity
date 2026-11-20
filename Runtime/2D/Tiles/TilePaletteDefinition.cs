using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Grid;

namespace BBUnity.Tiles {

    /// <summary>Which rules draw a cell type, and on which tilemap layer.</summary>
    [Serializable]
    public class TilePaletteEntry {

        [SerializeField]
        private CellTypeDefinition _cellType;

        [SerializeField]
        private TileRuleSetDefinition _ruleSet;

        [SerializeField, Tooltip("The name of the tilemap layer (below) these tiles are drawn on.")]
        private string _layerName = "Ground";

        public CellTypeDefinition CellType {
            get { return _cellType; }
        }

        public TileRuleSetDefinition RuleSet {
            get { return _ruleSet; }
        }

        public string LayerName {
            get { return _layerName; }
        }

        public TilePaletteEntry() { }

        public TilePaletteEntry(CellTypeDefinition cellType, TileRuleSetDefinition ruleSet, string layerName) {
            _cellType = cellType;
            _ruleSet = ruleSet;
            _layerName = layerName;
        }
    }

    /// <summary>
    /// The look of a level: for each cell type, the rule set that picks its tiles and the tilemap
    /// layer it is drawn on, plus how each layer is set up (sorting, physics layer, collision). A
    /// theme swaps palettes; generation never needs to know which tiles are used.
    /// </summary>
    [CreateAssetMenu(fileName = "New Tile Palette", menuName = "BBUnity/Tiles/Tile Palette Definition")]
    public class TilePaletteDefinition : ScriptableObject {

        [SerializeField]
        private List<TilePaletteEntry> _entries = new List<TilePaletteEntry>();

        [SerializeField]
        private List<TilemapLayerSettings> _layers = new List<TilemapLayerSettings> { new TilemapLayerSettings("Ground", TilemapCollisionMode.Solid) };

        public IReadOnlyList<TilePaletteEntry> Entries {
            get { return _entries; }
        }

        public IReadOnlyList<TilemapLayerSettings> Layers {
            get { return _layers; }
        }

        public bool TryGetEntry(CellTypeDefinition cellType, out TilePaletteEntry entry) {
            foreach (TilePaletteEntry candidate in _entries) {
                if (candidate != null && candidate.CellType == cellType) {
                    entry = candidate;
                    return true;
                }
            }

            entry = null;
            return false;
        }

        /// <summary>The settings for a named layer; a plain non-colliding layer if none is defined.</summary>
        public TilemapLayerSettings FindLayer(string name) {
            foreach (TilemapLayerSettings layer in _layers) {
                if (layer != null && layer.Name == name) { return layer; }
            }

            return new TilemapLayerSettings(name, TilemapCollisionMode.None);
        }
    }
}
