using UnityEngine;

using BBUnity.Generation.Placement;

namespace BBUnity.Gameplay.Levels.Elements {

    /// <summary>
    /// One thing level generation can put in a level. Generation decides where it goes (the
    /// Surface, Clearance, room types and budgets inherited from PlaceableDefinition); this adds
    /// what it becomes: the prefab that is instantiated there. For things that already have a
    /// node — a player spawn, a level transition, an enemy spawn node — the prefab is a
    /// GameObject carrying that node, set up once and reused. The prefab's pivot should be at
    /// the bottom centre, which is where it is placed.
    /// </summary>
    [CreateAssetMenu(fileName = "New Level Element", menuName = "BBUnity/Gameplay/Levels/Elements/Level Element Definition")]
    public class LevelElementDefinition : PlaceableDefinition {

        [SerializeField]
        private LevelElementCategoryDefinition _category;

        [SerializeField, Tooltip("What is instantiated at the placed position (pivot at the bottom centre).")]
        private GameObject _prefab;

        public LevelElementCategoryDefinition Category {
            get { return _category; }
        }

        public GameObject Prefab {
            get { return _prefab; }
        }
    }
}
