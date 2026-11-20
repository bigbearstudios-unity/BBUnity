using System;
using System.Collections.Generic;

using UnityEngine;

using BBUnity.Gameplay.Entities.Building;

namespace BBUnity.Editor.Gameplay.Entities.Building {

    /// <summary>
    /// Which Component Type maps to which IComponentSettings, if any — consulted only when a
    /// ComponentTypeReference's dropdown picks a new type, to decide what (if anything) to
    /// auto-populate ComponentSpec's Settings field with. Editor-only: this is purely an
    /// authoring-time convenience, the built IComponentSettings instance itself is what actually
    /// travels into the Runtime-safe EntityBuilder.
    /// </summary>
    internal static class ComponentSettingsRegistry {

        private static readonly Dictionary<Type, Func<IComponentSettings>> _factories = new Dictionary<Type, Func<IComponentSettings>>() {
            { typeof(Rigidbody2D), () => new Rigidbody2DSettings() },
            { typeof(Rigidbody), () => new RigidbodySettings() },
            { typeof(BoxCollider2D), () => new BoxCollider2DSettings() },
        };

        /// <summary>Returns a fresh settings instance for componentType, or null if it has none registered.</summary>
        public static IComponentSettings CreateSettingsFor(Type componentType) {
            return componentType != null && _factories.TryGetValue(componentType, out Func<IComponentSettings> factory) ? factory() : null;
        }
    }
}
