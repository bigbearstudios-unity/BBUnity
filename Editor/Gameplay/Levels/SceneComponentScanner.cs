using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace BBUnity.Editor.Gameplay.Levels {

    /// <summary>
    /// Peeks into another scene's contents without disturbing whatever the user currently has
    /// open — opens it additively, scans, then closes it again. Used by
    /// LevelTransitionNodeEditor to list the PlayerSpawnNodes available in a chosen target
    /// scene.
    ///
    /// If the target scene is already open (including being the one currently being edited), it's
    /// scanned in place and never closed — closing a scene the user already had open, especially
    /// one with unsaved changes, would be destructive, so that path is never taken here.
    /// </summary>
    internal static class SceneComponentScanner {

        public static List<T> FindComponentsInScene<T>(string scenePath) where T : Component {
            var results = new List<T>();
            if (string.IsNullOrEmpty(scenePath)) { return results; }

            Scene alreadyOpen = EditorSceneManager.GetSceneByPath(scenePath);
            if (alreadyOpen.IsValid() && alreadyOpen.isLoaded) {
                CollectComponents(alreadyOpen, results);
                return results;
            }

            Scene opened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try {
                CollectComponents(opened, results);
            } finally {
                EditorSceneManager.CloseScene(opened, true);
            }

            return results;
        }

        private static void CollectComponents<T>(Scene scene, List<T> results) where T : Component {
            foreach (GameObject root in scene.GetRootGameObjects()) {
                results.AddRange(root.GetComponentsInChildren<T>(true));
            }
        }
    }
}
