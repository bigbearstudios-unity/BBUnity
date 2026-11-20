using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Combat;
using BBUnity.Editor.Statistics;
using BBUnity.Statistics.Base;

namespace BBUnity.Editor.Combat {

    /// <summary>
    /// Lays out a ReductionEntry as two rows — Target Statistic (via StatisticTypeReferenceDrawer)
    /// then Reduction (via ReductionDrawer) — both of which already have their own
    /// registered drawers, so this just positions them under the labels it wants.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReductionEntry))]
    public class ReductionEntryDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var root = new VisualElement();
            root.Add(StatisticTypeReferenceDrawer.Create(property.FindPropertyRelative("_targetStatistic"), "Target Statistic"));
            root.Add(ReductionDrawer.Create(property.FindPropertyRelative("_reduction"), "Reduction"));
            return root;
        }
    }
}
