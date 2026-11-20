using System;
using System.Collections.Generic;

using UnityEditor;

using BBUnity.Gameplay.Entities.Controllers.Statistics;

namespace BBUnity.Editor.Gameplay.Entities.Controllers.Statistics {

    /// <summary>
    /// A data asset's statistic list (see CombatantDefinition), with the same "Add Statistic"
    /// picker and one-per-type rule as StatisticController's own inspector.
    /// </summary>
    [CustomPropertyDrawer(typeof(EntityStatisticList))]
    public class EntityStatisticListDrawer : TypePickerListDrawer {

        protected override string ListFieldName { get { return "_statistics"; } }
        protected override string AddButtonLabel { get { return "Add Statistic"; } }
        protected override string DropdownTitle { get { return "Statistic Type"; } }

        protected override List<Type> GetAvailableTypes() {
            return StatisticControllerEditor.GetAvailableStatisticTypes();
        }

        protected override bool CanAdd(Type type, SerializedProperty list) {
            if ((Activator.CreateInstance(type) as EntityStatistic)?.AllowsMultiple ?? false) { return true; }

            for (int i = 0; i < list.arraySize; i++) {
                object existing = list.GetArrayElementAtIndex(i).managedReferenceValue;
                if (existing != null && existing.GetType() == type) { return false; }
            }

            return true;
        }

        protected override string DescribeEntry(object instance) {
            return (instance as EntityStatistic)?.Name;
        }
    }
}
