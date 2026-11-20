using System;

using UnityEngine;

using BBUnity.Generation.Grid;

namespace BBUnity.Generation.Chunks {

    /// <summary>One named layer of a hand-authored chunk: width x height cell types, null meaning empty.</summary>
    [Serializable]
    public class ChunkLayer {

        [SerializeField]
        private string _name = GridLayerNames.Main;

        [SerializeField]
        private CellTypeDefinition[] _cells = new CellTypeDefinition[0];

        public string Name {
            get { return _name; }
        }

        public ChunkLayer() { }

        public ChunkLayer(string name, int cellCount) {
            _name = name;
            _cells = new CellTypeDefinition[cellCount];
        }

        public CellTypeDefinition Get(int index) {
            return index >= 0 && index < _cells.Length ? _cells[index] : null;
        }

        public void Set(int index, CellTypeDefinition type) {
            if (index >= 0 && index < _cells.Length) { _cells[index] = type; }
        }

        public void Resize(int cellCount) {
            Array.Resize(ref _cells, cellCount);
        }

        /// <summary>Changes the layer from one width and height to another, keeping the cells that still fit where they are.</summary>
        public void Remap(int oldWidth, int oldHeight, int newWidth, int newHeight) {
            var resized = new CellTypeDefinition[newWidth * newHeight];

            for (int y = 0; y < Math.Min(oldHeight, newHeight); y++) {
                for (int x = 0; x < Math.Min(oldWidth, newWidth); x++) {
                    resized[y * newWidth + x] = Get(y * oldWidth + x);
                }
            }

            _cells = resized;
        }
    }
}
