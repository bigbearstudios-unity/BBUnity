using UnityEngine;

using BBUnity.Generation.Pipeline;

namespace BBUnity.World.Generation {

    /// <summary>
    /// Builds one extra layer of a generated level, such as its tilemaps, under a container that
    /// LevelBuilder creates for it. Lives in Core so LevelBuilder does not depend on any
    /// dimension; the 2D assembly supplies the tilemap implementation.
    /// </summary>
    public interface ILevelLayerBuilder {
        /// <summary>The name of the container object LevelBuilder creates for this layer.</summary>
        string ContainerName { get; }

        void Build(GenerationResult result, GridToWorld mapping, Transform container);
    }
}
