namespace BBUnity.Generation.Placement {

    /// <summary>Where a placeable sits relative to the level's solid cells.</summary>
    public enum PlacementSurface {
        /// <summary>On top of something that can be stood on (enemies, chests, floor traps).</summary>
        Floor,

        /// <summary>Hanging under a blocking cell (ceiling spikes).</summary>
        Ceiling,

        /// <summary>Against a blocking cell to the left or right (wall torches, wall traps).</summary>
        Wall,

        /// <summary>Anywhere there is room (floating pickups).</summary>
        Air
    }
}
