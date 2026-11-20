using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using BBUnity.Combat.HitEffects;


namespace BBUnity.Editor.Combat {

    /// <summary>
    /// A HitEffectList, with an "Add Effect" type picker listing every concrete HitEffect.
    /// </summary>
    [CustomPropertyDrawer(typeof(HitEffectList))]
    public class HitEffectListDrawer : TypePickerListDrawer {

        private static List<Type> _cachedTypes;

        protected override string ListFieldName { get { return "_effects"; } }
        protected override string AddButtonLabel { get { return "Add Effect"; } }
        protected override string DropdownTitle { get { return "Effect Type"; } }

        protected override List<Type> GetAvailableTypes() {
            if (_cachedTypes == null) {
                _cachedTypes = TypeCache.GetTypesDerivedFrom<HitEffect>()
                    .Where(t => !t.IsAbstract && !t.IsNested && t.GetConstructor(Type.EmptyTypes) != null)
                    .OrderBy(t => t.FullName)
                    .ToList();
            }

            return _cachedTypes;
        }
    }
}
