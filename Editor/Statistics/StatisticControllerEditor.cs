using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Statistics.Base;
using BBUnity.Statistics;

namespace BBUnity.Editor.Statistics {

    [CustomEditor(typeof(StatisticController), editorForChildClasses: true)]
    public class StatisticControllerEditor : UnityEditor.Editor {

        private static List<Type> _cachedStatisticTypes;

        private SerializedProperty _statisticsProperty;

        public override VisualElement CreateInspectorGUI() {
            _statisticsProperty = serializedObject.FindProperty("_entityStatistics");

            VisualElement root = InspectorElements.CreateDefault(serializedObject, GetExcludedProperties());

            // Unlike StateControllerEditor's states (pure logic, no data), statistics carry
            // editable data (the boxed Health/Mana/Poise fields, plus the Slider) — so every
            // visible field of the concrete type is drawn here, fully editable, exactly as it
            // would be if it were still a direct [SerializeField] on the component.
            root.Add(InspectorElements.CreateHeading("Statistics"));

            var list = new ArrayListElement(
                serializedObject,
                _statisticsProperty.propertyPath,
                createBody: element => {
                    var body = new VisualElement();
                    InspectorElements.AddChildren(body, element);
                    return body;
                },
                getTitle: (element, index) => {
                    EntityStatistic instance = element.managedReferenceValue as EntityStatistic;
                    return instance != null ? TypeDropdown.BuildLabel(instance.GetType()) : "(missing statistic)";
                }
            );
            root.Add(list);

            Button addButton = null;
            addButton = new Button(() => {
                serializedObject.UpdateIfRequiredOrScript();
                List<Type> availableTypes = GetAvailableStatisticTypes().Where(t => AllowsMultiple(t) || !ContainsStatisticOfType(t)).ToList();
                var dropdown = new TypeDropdown(new AdvancedDropdownState(), availableTypes, type => {
                    AddStatistic(type);
                    list.Rebuild();
                }, "Statistic Type");
                dropdown.Show(addButton.worldBound);
            }) { text = "Add Statistic" };
            root.Add(addButton);

            CreateAdditionalContent(root);

            return root;
        }

        // Overridden by StatisticCombatControllerEditor to also exclude its own fields
        // (Attacks, Reductions) from the default property drawing, so they aren't drawn
        // twice — once by the default pass, once by CreateAdditionalContent().
        protected virtual string[] GetExcludedProperties() {
            return new[] { "_entityStatistics", "m_Script" };
        }

        // Extension point for a subclass's own inspector content (see
        // StatisticCombatControllerEditor), added after the statistics list and Add button.
        protected virtual void CreateAdditionalContent(VisualElement root) { }

        private void AddStatistic(Type type) {
            // Mirrors the one-statistic-per-type constraint below — a second Health entry would
            // just shadow the first one via GetStatistic<T>() without this check.
            if (!AllowsMultiple(type) && ContainsStatisticOfType(type)) {
                Debug.LogError($"'{target.name}' already has an statistic of type '{TypeDropdown.BuildLabel(type)}' — only one instance per statistic type is supported.", target);
                return;
            }

            var instance = Activator.CreateInstance(type) as EntityStatistic;

            serializedObject.Update();
            _statisticsProperty.arraySize++;
            SerializedProperty newElement = _statisticsProperty.GetArrayElementAtIndex(_statisticsProperty.arraySize - 1);
            newElement.managedReferenceValue = instance;
            serializedObject.ApplyModifiedProperties();
        }

        // Generic statistics (told apart by Name) can be added repeatedly; typed ones are one per
        // controller. Asked of an instance because it is a per-type fact declared on the class.
        private static bool AllowsMultiple(Type type) {
            return (Activator.CreateInstance(type) as EntityStatistic)?.AllowsMultiple ?? false;
        }

        private bool ContainsStatisticOfType(Type type) {
            for (int i = 0; i < _statisticsProperty.arraySize; i++) {
                EntityStatistic existing = _statisticsProperty.GetArrayElementAtIndex(i).managedReferenceValue as EntityStatistic;
                if (existing != null && existing.GetType() == type) { return true; }
            }

            return false;
        }

        // Internal rather than private: StatisticTypeReferenceDrawer reuses this same scan for
        // its "pick an statistic type" dropdown (DamageValue/Reduction), rather than repeating
        // the TypeCache query.
        internal static List<Type> GetAvailableStatisticTypes() {
            if (_cachedStatisticTypes == null) {
                _cachedStatisticTypes = TypeCache.GetTypesDerivedFrom<EntityStatistic>()
                    .Where(t => !t.IsAbstract && !t.IsNested && HasParameterlessConstructor(t))
                    .OrderBy(t => t.FullName)
                    .ToList();
            }

            return _cachedStatisticTypes;
        }

        private static bool HasParameterlessConstructor(Type type) {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}
