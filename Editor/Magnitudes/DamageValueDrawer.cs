using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Magnitudes;

namespace BBUnity.Editor.Magnitudes {

    /// <summary>
    /// Generic "pick a concrete type, then edit its fields inline" drawer for any single
    /// [SerializeReference] DamageValue field — used by DamageEntry.DamageValue.
    ///
    /// Unlike StatisticController/StatisticCombatController's "Add X" lists, this field isn't a
    /// list — there's exactly one DamageValue per slot — so the picker replaces the field's
    /// value in place rather than appending to anything.
    /// </summary>
    [CustomPropertyDrawer(typeof(DamageValue), useForChildren: true)]
    public class DamageValueDrawer : PropertyDrawer {

        private static List<Type> _cachedTypes;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            return Create(property, property.displayName);
        }

        // Shared with DamageEntryDrawer, which wants its own label ("Damage").
        internal static VisualElement Create(SerializedProperty property, string label) {
            return new ManagedReferenceField(property, label, GetAvailableTypes, "Damage Type");
        }

        private static List<Type> GetAvailableTypes() {
            if (_cachedTypes == null) {
                _cachedTypes = TypeCache.GetTypesDerivedFrom<DamageValue>()
                    .Where(t => !t.IsAbstract && !t.IsNested && t.GetConstructor(Type.EmptyTypes) != null)
                    .OrderBy(t => t.FullName)
                    .ToList();
            }

            return _cachedTypes;
        }
    }
}
