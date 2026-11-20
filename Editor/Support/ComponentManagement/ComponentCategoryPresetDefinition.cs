using System;
using System.Collections.Generic;

using UnityEngine;

namespace BBUnity.Editor.Support.ComponentManagement {

    /// <summary>
    /// A named set of component type -> category rules for the Inspector's component management.
    /// BBUnity ships one built-in preset (BuiltInComponentPresets); anyone can create their own,
    /// stored as assets in the project and managed in Window > BBUnity > Component Presets. The
    /// preset in use is chosen in BBUnitySettings > Component Management.
    /// </summary>
    public class ComponentCategoryPresetDefinition : ScriptableObject {

        [Serializable]
        private class RuleList {
            public List<ComponentCategoryRule> Rules;
        }

        [SerializeField]
        private List<ComponentCategoryRule> _rules = new List<ComponentCategoryRule>();

        [NonSerialized]
        private bool _builtIn;

        /// <summary>Built-in presets live in code and can't be edited, only duplicated.</summary>
        public bool IsBuiltIn {
            get { return _builtIn; }
        }

        public IReadOnlyList<ComponentCategoryRule> Rules {
            get { return _rules; }
        }

        /// <summary>An in-memory preset (not an asset).</summary>
        public static ComponentCategoryPresetDefinition Create(string name, IEnumerable<ComponentCategoryRule> rules) {
            var preset = CreateInstance<ComponentCategoryPresetDefinition>();
            preset.hideFlags = HideFlags.HideAndDontSave;
            preset.name = name;
            preset._rules = new List<ComponentCategoryRule>(rules);
            return preset;
        }

        internal void MarkBuiltIn() {
            _builtIn = true;
        }

        /// <summary>The category the first matching rule puts this component type in, or null.</summary>
        public string CategoryFor(Type type) {
            foreach (ComponentCategoryRule rule in _rules) {
                if (rule != null && !string.IsNullOrWhiteSpace(rule.Category) && rule.AppliesTo(type)) {
                    return rule.Category.Trim();
                }
            }

            return null;
        }

        /// <summary>Category names in the order they are shown (the order of their first rule).</summary>
        public List<string> CategoryOrder() {
            var order = new List<string>();

            foreach (ComponentCategoryRule rule in _rules) {
                if (rule == null || string.IsNullOrWhiteSpace(rule.Category)) { continue; }

                string name = rule.Category.Trim();
                if (!order.Contains(name)) { order.Add(name); }
            }

            return order;
        }

        /// <summary>Replaces this preset's rules with an independent copy of another's.</summary>
        public void CopyRulesFrom(ComponentCategoryPresetDefinition other) {
            string json = JsonUtility.ToJson(new RuleList { Rules = new List<ComponentCategoryRule>(other._rules) });
            _rules = JsonUtility.FromJson<RuleList>(json).Rules ?? new List<ComponentCategoryRule>();
        }
    }
}
