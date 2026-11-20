using UnityEngine;
using System.Collections;

namespace BBUnity.GameFlow {

    /// <summary>
    /// How the screen is covered while one level is swapped for another. SessionController asks
    /// for the first half (Out) before it starts loading and the second half (In) once the new
    /// level is in place, so every type just has to implement those two halves.
    /// </summary>
    public enum LevelTransitionType {
        /// <summary>
        /// Fade to a color (black by default), swap the level, fade back in.
        /// </summary>
        Fade,

        /// <summary>
        /// A dedicated loading screen. Not built yet — currently behaves as an instant cut.
        /// </summary>
        [InspectorName("Loading Screen")]
        LoadingScreen
    }
}
