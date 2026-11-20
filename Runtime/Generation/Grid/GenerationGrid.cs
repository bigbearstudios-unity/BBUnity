using System.Collections.Generic;

namespace BBUnity.Generation.Grid {

    /// <summary>The conventional layer names. Generators may add their own.</summary>
    public static class GridLayerNames {
        public const string Main = "Main";
        public const string Background = "Background";
        public const string Decoration = "Decoration";
    }

    /// <summary>
    /// The abstract picture of a level: one or more same-sized GridLayers of cell types. It has no
    /// tiles and no prefabs — those are produced from it afterwards — which is what makes a
    /// generated level inspectable without any art.
    /// </summary>
    public class GenerationGrid {

        private readonly Dictionary<string, GridLayer> _layers = new Dictionary<string, GridLayer>();

        public int Width { get; }
        public int Height { get; }

        public GenerationGrid(int width, int height) {
            Width = UnityEngine.Mathf.Max(1, width);
            Height = UnityEngine.Mathf.Max(1, height);
        }

        public IEnumerable<GridLayer> Layers {
            get { return _layers.Values; }
        }

        /// <summary>The layer generation carves rooms and platforms into.</summary>
        public GridLayer Main {
            get { return GetOrAddLayer(GridLayerNames.Main); }
        }

        public GridLayer GetOrAddLayer(string name) {
            if (!_layers.TryGetValue(name, out GridLayer layer)) {
                layer = new GridLayer(name, Width, Height);
                _layers[name] = layer;
            }

            return layer;
        }

        public bool TryGetLayer(string name, out GridLayer layer) {
            return _layers.TryGetValue(name, out layer);
        }
    }
}
