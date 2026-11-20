using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Combat;
using BBUnity.Editor.Magnitudes;
using BBUnity.Editor.Statistics;
using BBUnity.Statistics.Base;

namespace BBUnity.Editor.Combat {

    /// <summary>
    /// Lays out a DamageEntry as two rows — Target Statistic (via StatisticTypeReferenceDrawer)
    /// then Damage (via DamageValueDrawer) — both of which already have their own
    /// registered drawers, so this just positions them under the labels it wants.
    /// </summary>
    [CustomPropertyDrawer(typeof(DamageEntry))]
    public class DamageEntryDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var root = new VisualElement();
            root.Add(StatisticTypeReferenceDrawer.Create(property.FindPropertyRelative("_targetStatistic"), "Target Statistic"));
            root.Add(DamageValueDrawer.Create(property.FindPropertyRelative("_damageValue"), "Damage"));
            return root;
        }
    }
}
