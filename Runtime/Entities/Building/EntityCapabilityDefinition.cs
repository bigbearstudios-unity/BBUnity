using System.Collections.Generic;
using UnityEngine;

namespace BBUnity.Entities.Building {

    /// <summary>
    /// A single reusable capability an entity can be built from — e.g. "Damageable"
    /// (AttackReceiver + StatisticCombatController), "Attacker" (an AttackCollider2D on a
    /// child), "Visual2D" (a Visual child with SpriteRenderer + SpriteAnimator). Deliberately
    /// small in number and stable — unlike EntityDefinition, which is meant to grow freely per
    /// project, the set of capabilities should only grow when BBUnity itself gains a new kind of
    /// component worth composing with. A "Projectile" and a "Boss" definition can both use the same
    /// Attacker capability despite having nothing else in common.
    /// </summary>
    [CreateAssetMenu(fileName = "New Entity Capability", menuName = "BBUnity/Entities/Building/Entity Capability Definition")]
    public class EntityCapabilityDefinition : ScriptableObject {

        [SerializeField, Tooltip("What this capability gives an entity — shown to whoever is composing an EntityDefinition from it.")]
        private string _description;

        [SerializeField]
        private List<ComponentSpec> _rootComponents = new List<ComponentSpec>();

        [SerializeField]
        private List<ChildObjectSpec> _childObjects = new List<ChildObjectSpec>();

        [SerializeField]
        private List<SubclassSlot> _subclassSlots = new List<SubclassSlot>();

        public string Description {
            get { return _description; }
        }

        public IReadOnlyList<ComponentSpec> RootComponents {
            get { return _rootComponents; }
        }

        public IReadOnlyList<ChildObjectSpec> ChildObjects {
            get { return _childObjects; }
        }

        public IReadOnlyList<SubclassSlot> SubclassSlots {
            get { return _subclassSlots; }
        }
    }
}
