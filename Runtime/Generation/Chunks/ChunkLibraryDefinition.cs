using System.Collections.Generic;

using UnityEngine;

namespace BBUnity.Generation.Chunks {

    /// <summary>A collection of chunks a template can draw from, searchable by tag.</summary>
    [CreateAssetMenu(fileName = "New Chunk Library", menuName = "BBUnity/Generation/Chunks/Chunk Library Definition")]
    public class ChunkLibraryDefinition : ScriptableObject {

        [SerializeField]
        private List<ChunkDefinition> _chunks = new List<ChunkDefinition>();

        public IReadOnlyList<ChunkDefinition> Chunks {
            get { return _chunks; }
        }

        public void SetChunks(IEnumerable<ChunkDefinition> chunks) {
            _chunks = new List<ChunkDefinition>(chunks);
        }

        public List<ChunkDefinition> WithTag(string tag) {
            var matches = new List<ChunkDefinition>();
            foreach (ChunkDefinition chunk in _chunks) {
                if (chunk != null && chunk.HasTag(tag)) { matches.Add(chunk); }
            }

            return matches;
        }
    }
}
