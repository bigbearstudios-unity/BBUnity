using UnityEngine;
using UnityEngine.Serialization;

using BBUnity.Support.Scenes;
namespace BBUnity.Gameplay.Controllers {

    /// <summary>
    /// Must live at Assets/Resources/BootSettings.asset — the one thing BootLoaderController's static
    /// bootstrap (see BootLoaderController.EnsureLoaded) can read before any scene, including OnBoot
    /// itself, has loaded. Resources.Load works regardless of what's currently loaded; a
    /// serialized field on a BootLoaderController instance wouldn't be reachable yet at that point.
    ///
    /// MainMenu/Gameplay are named here (not just left to BootLoaderController's own scene list)
    /// because EnsureLoaded() needs to know their identity specifically — both to recognize "I'm
    /// already entering one of the known tiers, don't do anything extra" and to know which scene
    /// to guarantee-load when that's not the case (see EnsureLoaded's doc comment).
    /// </summary>
    [CreateAssetMenu(fileName = "BootSettings", menuName = "BBUnity/Gameplay/Controllers/Boot Settings")]
    public class BootSettings : ScriptableObject {

        [SerializeField, FormerlySerializedAs("_bootScene")]
        private SceneReference _onBootScene;

        [SerializeField, FormerlySerializedAs("_frontendScene")]
        private SceneReference _mainMenuScene;

        [SerializeField]
        private SceneReference _gameplayScene;

        public SceneReference OnBootScene {
            get { return _onBootScene; }
        }

        public SceneReference MainMenuScene {
            get { return _mainMenuScene; }
        }

        public SceneReference GameplayScene {
            get { return _gameplayScene; }
        }
    }
}
