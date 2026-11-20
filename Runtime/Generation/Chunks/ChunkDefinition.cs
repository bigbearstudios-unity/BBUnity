using System.Collections.Generic;

using UnityEngine;

using BBUnity.Generation.Grid;
using BBUnity.Generation.Placement;
using BBUnity.Generation.Rooms;

namespace BBUnity.Generation.Chunks {

    /// <summary>
    /// A small hand-authored piece of level — a room, a shaft, a set piece — that generation can
    /// stamp into the grid. Its cells are stored per layer, with sockets marking where other
    /// chunks can attach and markers for hand-placed enemies, items and traps. It is edited in
    /// the Chunk Editor window.
    /// </summary>
    [CreateAssetMenu(fileName = "New Chunk", menuName = "BBUnity/Generation/Chunks/Chunk Definition")]
    public class ChunkDefinition : ScriptableObject {

        [SerializeField, Min(1)]
        private int _width = 16;

        [SerializeField, Min(1)]
        private int _height = 12;

        [SerializeField]
        private List<ChunkLayer> _layers = new List<ChunkLayer>();

        [SerializeField]
        private List<ChunkSocket> _sockets = new List<ChunkSocket>();

        [SerializeField]
        private List<ChunkMarker> _markers = new List<ChunkMarker>();

        [SerializeField, Tooltip("Free-form labels used to pick chunks (for example \"shaft\", \"arena\").")]
        private List<string> _tags = new List<string>();

        [SerializeField, Tooltip("The room type this chunk is when it is used as a room.")]
        private RoomTypeDefinition _roomType;

        [SerializeField, Min(0f), Tooltip("How likely this is chosen over other matching chunks.")]
        private float _weight = 1f;

        public int Width {
            get { return _width; }
        }

        public int Height {
            get { return _height; }
        }

        public IReadOnlyList<ChunkLayer> Layers {
            get { return _layers; }
        }

        public IReadOnlyList<ChunkSocket> Sockets {
            get { return _sockets; }
        }

        public IReadOnlyList<ChunkMarker> Markers {
            get { return _markers; }
        }

        public IReadOnlyList<string> Tags {
            get { return _tags; }
        }

        public RoomTypeDefinition RoomType {
            get { return _roomType; }
        }

        public float Weight {
            get { return _weight; }
        }

        // ---- editing (used by the Chunk Editor; the cells are stored per layer, bottom row first) ----

        /// <summary>Changes the chunk's size, keeping the cells that still fit, and drops sockets and markers that no longer do.</summary>
        public void Resize(int width, int height) {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);

            foreach (ChunkLayer layer in _layers) { layer.Remap(_width, _height, width, height); }

            _width = width;
            _height = height;

            _sockets.RemoveAll(socket => socket.Edge == ChunkEdge.Left || socket.Edge == ChunkEdge.Right ? socket.Offset >= height : socket.Offset >= width);
            _markers.RemoveAll(marker => marker.Position.x >= width || marker.Position.y >= height);
        }

        /// <summary>The named layer, created (empty) if the chunk doesn't have it yet.</summary>
        public ChunkLayer EnsureLayer(string name) {
            foreach (ChunkLayer layer in _layers) {
                if (layer.Name == name) { return layer; }
            }

            var created = new ChunkLayer(name, _width * _height);
            _layers.Add(created);
            return created;
        }

        public CellTypeDefinition GetCell(string layerName, int x, int y) {
            if (x < 0 || y < 0 || x >= _width || y >= _height) { return null; }

            foreach (ChunkLayer layer in _layers) {
                if (layer.Name == layerName) { return layer.Get(y * _width + x); }
            }

            return null;
        }

        public void SetCell(string layerName, int x, int y, CellTypeDefinition type) {
            if (x < 0 || y < 0 || x >= _width || y >= _height) { return; }

            EnsureLayer(layerName).Set(y * _width + x, type);
        }

        /// <summary>Puts a marker at a cell, replacing any marker already there.</summary>
        public void AddMarker(PlaceableDefinition placeable, Vector2Int position) {
            RemoveMarkerAt(position);
            _markers.Add(new ChunkMarker(placeable, position));
        }

        public bool RemoveMarkerAt(Vector2Int position) {
            return _markers.RemoveAll(marker => marker.Position == position) > 0;
        }

        /// <summary>Adds a socket, or removes it if one with the same edge and offset is already there.</summary>
        public void ToggleSocket(ChunkEdge edge, int offset, string tag) {
            if (_sockets.RemoveAll(socket => socket.Edge == edge && socket.Offset == offset) > 0) { return; }

            _sockets.Add(new ChunkSocket(edge, offset, tag));
        }

        public bool HasTag(string tag) {
            return _tags.Contains(tag);
        }

        /// <summary>Copies the chunk into the grid with its bottom-left corner at `origin` (cells outside the grid are skipped).</summary>
        public void Stamp(GenerationGrid grid, Vector2Int origin) {
            foreach (ChunkLayer layer in _layers) {
                GridLayer target = grid.GetOrAddLayer(layer.Name);

                for (int y = 0; y < _height; y++) {
                    for (int x = 0; x < _width; x++) {
                        target.Set(origin.x + x, origin.y + y, layer.Get(y * _width + x));
                    }
                }
            }
        }
    }
}
