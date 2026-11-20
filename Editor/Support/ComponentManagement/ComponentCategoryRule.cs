using System;
using System.Collections.Generic;

using UnityEngine;

namespace BBUnity.Editor.Support.ComponentManagement {

    /// <summary>
    /// "Components matching any of these go into this category." Rules in a preset are checked
    /// in order and the first that matches wins, which is also the order categories are shown in.
    /// </summary>
    [Serializable]
    public class ComponentCategoryRule {

        [SerializeField]
        private string _category = "New Category";

        [SerializeField]
        private List<ComponentMatch> _matches = new List<ComponentMatch>();

        public ComponentCategoryRule() { }

        public ComponentCategoryRule(string category, params ComponentMatch[] matches) {
            _category = category;
            _matches = new List<ComponentMatch>(matches);
        }

        public string Category {
            get { return _category; }
        }

        public IReadOnlyList<ComponentMatch> Matches {
            get { return _matches; }
        }

        public bool AppliesTo(Type type) {
            foreach (ComponentMatch match in _matches) {
                if (match != null && match.Matches(type)) { return true; }
            }

            return false;
        }
    }
}
