using System;

namespace BBUnity.Gameplay.Entities.Building {

    /// <summary>
    /// Marks a Component as eligible for the entity-building system's "Component Type" dropdown
    /// (see ComponentSpec/EntityCapabilityDefinition). Anything deriving from EntityController is already
    /// included automatically, so this is for the rest — components worth composing entities
    /// from that aren't themselves EntityControllers (e.g. AttackReceiver). Unity's own built-in
    /// types (Rigidbody2D, BoxCollider2D, ...) can't be attributed like this since BBUnity
    /// doesn't own them — see BBUnitySettings.Entities.AdditionalComponentTypes for those instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CapabilityComponentAttribute : Attribute {

    }
}
