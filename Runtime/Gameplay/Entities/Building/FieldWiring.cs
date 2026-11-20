using UnityEngine;

namespace BBUnity.Gameplay.Entities.Building {

    /// <summary>
    /// Points one serialized field on a built component at either the entity's root or a named
    /// child object created elsewhere in the same EntityDefinition — e.g. EnemyAIController's
    /// "_visual" field wired to a child object named "Visual" created by a different
    /// EntityCapabilityDefinition. Resolved by EntityBuilder once every component/child object in the definition
    /// exists, so wiring order never depends on capability order.
    /// </summary>
    [System.Serializable]
    public class FieldWiring {

        [SerializeField, Tooltip("The private/public field name on the component being wired, exactly as declared in code (e.g. \"_visual\").")]
        private string _fieldName;

        [SerializeField, Tooltip("Name of the child object to wire this field to. Leave empty to wire it to the entity's own root GameObject/Transform instead.")]
        private string _targetChildName;

        public string FieldName {
            get { return _fieldName; }
        }

        public string TargetChildName {
            get { return _targetChildName; }
        }

        public bool TargetsRoot {
            get { return string.IsNullOrEmpty(_targetChildName); }
        }
    }
}
