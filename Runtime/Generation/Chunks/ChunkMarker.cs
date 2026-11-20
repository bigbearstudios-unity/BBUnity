using System;

using UnityEngine;

using BBUnity.Generation.Placement;

namespace BBUnity.Generation.Chunks {

    /// <summary>A hand-placed spot inside a chunk where a specific placeable (enemy, chest, trap...) goes.</summary>
    [Serializable]
    public struct ChunkMarker {

        [SerializeField]
        private PlaceableDefinition _placeable;

        [SerializeField]
        private Vector2Int _position;

        public PlaceableDefinition Placeable {
            get { return _placeable; }
        }

        public Vector2Int Position {
            get { return _position; }
        }

        public ChunkMarker(PlaceableDefinition placeable, Vector2Int position) {
            _placeable = placeable;
            _position = position;
        }
    }
}
