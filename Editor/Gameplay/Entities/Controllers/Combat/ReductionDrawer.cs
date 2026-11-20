using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Gameplay.Entities.Controllers.Combat;

namespace BBUnity.Editor.Gameplay.Entities.Controllers.Combat {

    /// <summary>
    /// "Pick a concrete type, then edit its fields inline" drawer for a single [SerializeReference]
    /// Reduction field (ReductionEntry.Reduction) — mirrors DamageValueDrawer exactly, one
    /// per polymorphic slot rather than a list.
    /// </summary>
    [CustomPropertyDrawer(typeof(Reduction), useForChildren: true)]
    public class ReductionDrawer : PropertyDrawer {

        private static List<Type> _cachedTypes;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            return Create(property, property.displayName);
        }

        // Shared with ReductionEntryDrawer, which wants its own label ("Reduction").
        internal static VisualElement Create(SerializedProperty property, string label) {
            return new ManagedReferenceField(property, label, GetAvailableTypes, "Reduction Type");
        }

        private static List<Type> GetAvailableTypes() {
            if (_cachedTypes == null) {
                _cachedTypes = TypeCache.GetTypesDerivedFrom<Reduction>()
                    .Where(t => !t.IsAbstract && !t.IsNested && t.GetConstructor(Type.EmptyTypes) != null)
                    .OrderBy(t => t.FullName)
                    .ToList();
            }

            return _cachedTypes;
        }
    }
}
