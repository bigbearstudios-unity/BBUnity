using UnityEngine;

namespace BBUnity.Gameplay.Entities.Building {

    /// <summary>
    /// Pre-configures a component EntityBuilder just added — e.g. Rigidbody2DSettings exposing
    /// mass/gravity scale/constraints so a definition can describe exactly what physics body an
    /// entity gets, not just that it gets one. Optional: most ComponentSpecs (anything with its
    /// own normal Inspector, or configured entirely through FieldWiring/the built instance
    /// afterward) have no settings at all. Only registered for types worth pre-configuring this
    /// way — see ComponentSettingsRegistry (Editor) for which Type maps to which settings class.
    /// </summary>
    public interface IComponentSettings {
        void Apply(Component instance);
    }
}
