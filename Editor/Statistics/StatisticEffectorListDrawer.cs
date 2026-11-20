using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using BBUnity.Statistics.Effectors;


namespace BBUnity.Editor.Statistics {

    /// <summary>
    /// An statistic's effector list, with an "Add Effector" type picker.
    /// </summary>
    [CustomPropertyDrawer(typeof(StatisticEffectorList))]
    public class StatisticEffectorListDrawer : TypePickerListDrawer {

        private static List<Type> _cachedTypes;

        protected override string ListFieldName { get { return "_effectors"; } }
        protected override string AddButtonLabel { get { return "Add Effector"; } }
        protected override string DropdownTitle { get { return "Effector Type"; } }

        protected override List<Type> GetAvailableTypes() {
            if (_cachedTypes == null) {
                _cachedTypes = TypeCache.GetTypesDerivedFrom<StatisticEffector>()
                    .Where(t => !t.IsAbstract && !t.IsNested && t.GetConstructor(Type.EmptyTypes) != null)
                    .OrderBy(t => t.FullName)
                    .ToList();
            }

            return _cachedTypes;
        }
    }
}
