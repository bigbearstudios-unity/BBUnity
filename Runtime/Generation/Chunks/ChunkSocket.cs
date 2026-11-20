using System;

using UnityEngine;

namespace BBUnity.Generation.Chunks {

    public enum ChunkEdge {
        Left,
        Right,
        Top,
        Bottom
    }

    /// <summary>
    /// A connection point on a chunk's edge: another chunk can be placed against it if it has a
    /// socket on the opposite edge with a matching tag at the same position.
    /// </summary>
    [Serializable]
    public struct ChunkSocket {

        [SerializeField]
        private ChunkEdge _edge;

        [SerializeField, Min(0), Tooltip("Position along the edge, in cells from the bottom (left/right edges) or the left (top/bottom edges).")]
        private int _offset;

        [SerializeField, Tooltip("Sockets only join when their tags match.")]
        private string _tag;

        public ChunkEdge Edge {
            get { return _edge; }
        }

        public int Offset {
            get { return _offset; }
        }

        public string Tag {
            get { return _tag; }
        }

        public ChunkSocket(ChunkEdge edge, int offset, string tag = "") {
            _edge = edge;
            _offset = offset;
            _tag = tag;
        }

        public bool Joins(ChunkSocket other) {
            bool opposite = (_edge == ChunkEdge.Left && other._edge == ChunkEdge.Right)
                || (_edge == ChunkEdge.Right && other._edge == ChunkEdge.Left)
                || (_edge == ChunkEdge.Top && other._edge == ChunkEdge.Bottom)
                || (_edge == ChunkEdge.Bottom && other._edge == ChunkEdge.Top);

            return opposite && _offset == other._offset && (_tag ?? string.Empty) == (other._tag ?? string.Empty);
        }
    }
}
