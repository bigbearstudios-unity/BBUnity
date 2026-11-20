using UnityEngine;

namespace BBUnity.Gameplay.Entities.Controllers.Combat {

    /// <summary>
    /// A label an Attack or CombatActionDefinition can carry, which travels with every hit it produces
    /// (HitInfo.Tags) so the defender can tell what kind of hit it is. BBUnity attaches no meaning
    /// to any tag — a game creates the ones it cares about ("Unparryable", "Fire", "Heavy", ...)
    /// and decides what to do when it sees one. A tag is an asset rather than a string so it can
    /// be picked from a list, renamed in one place, and never mistyped.
    /// </summary>
    [CreateAssetMenu(fileName = "Tag_", menuName = "BBUnity/Gameplay/Entities/Controllers/Combat/Combat Tag Definition")]
    public class CombatTagDefinition : ScriptableObject {

        [SerializeField, TextArea, Tooltip("Optional note on what this tag is for.")]
        private string _description;

        public string Description {
            get { return _description; }
        }
    }
}
