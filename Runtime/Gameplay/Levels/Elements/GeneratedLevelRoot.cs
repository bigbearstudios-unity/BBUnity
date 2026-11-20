using UnityEngine;

namespace BBUnity.Gameplay.Levels.Elements {

    /// <summary>
    /// The root of a generated level in a scene. Everything generation creates is under it, so
    /// regenerating replaces just this object and anything placed by hand elsewhere in the scene
    /// is left alone. It remembers the seed so a level can be recreated exactly.
    /// </summary>
    [AddComponentMenu("BBUnity/Gameplay/Levels/Elements/Generated Level Root")]
    public class GeneratedLevelRoot : MonoBehaviour {

        public const string RoomsContainer = "Rooms";
        public const string ElementsContainer = "Elements";
        public const string TilesContainer = "Tiles";

        [SerializeField]
        private int _seed;

        [SerializeField]
        private string _templateName;

        public int Seed {
            get { return _seed; }
        }

        public string TemplateName {
            get { return _templateName; }
        }

        public void Initialize(int seed, string templateName) {
            _seed = seed;
            _templateName = templateName;
        }
    }
}
