using UnityEngine;

namespace BBUnity.World.Elements {

    /// <summary>
    /// Added to everything level generation puts in a level, so the scene remembers what each
    /// object is (and editors can list, filter or regenerate it) without the prefab itself
    /// needing to know about generation.
    /// </summary>
    [AddComponentMenu("BBUnity/World/Elements/Level Element")]
    public class LevelElement : MonoBehaviour {

        [SerializeField]
        private LevelElementDefinition _definition;

        [SerializeField, Tooltip("The id of the generated room this was placed in.")]
        private int _roomId = -1;

        public LevelElementDefinition Definition {
            get { return _definition; }
        }

        public int RoomId {
            get { return _roomId; }
        }

        public void Initialize(LevelElementDefinition definition, int roomId) {
            _definition = definition;
            _roomId = roomId;
        }
    }
}
