using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace BBUnity.Editor.Toolkit {

    /// <summary>
    /// A boxed section with a header row (an arrow plus whatever the caller adds to Header) and a
    /// Body that shows or hides when the arrow — or any element passed to AttachToggle — is
    /// clicked. Used where a Foldout's single label isn't enough (buttons, toggles and icons in
    /// the header).
    /// </summary>
    internal class CollapsibleSection : VisualElement {

        private readonly Label _arrow = new Label();
        private readonly Action<bool> _toggled;
        private bool _expanded;

        public VisualElement Header { get; } = new VisualElement();
        public VisualElement Body { get; } = new VisualElement();

        public CollapsibleSection(bool expanded, Action<bool> toggled = null) {
            _toggled = toggled;

            VisualElement frame = InspectorElements.CreateBox();
            Add(frame);

            Header.style.flexDirection = FlexDirection.Row;
            Header.style.alignItems = Align.Center;

            _arrow.style.width = 14;
            _arrow.style.unityTextAlign = TextAnchor.MiddleCenter;
            Header.Add(_arrow);
            AttachToggle(_arrow);

            Body.style.marginTop = 2;

            frame.Add(Header);
            frame.Add(Body);

            SetExpanded(expanded, notify: false);
        }

        public bool Expanded {
            get { return _expanded; }
        }

        public void AttachToggle(VisualElement element) {
            element.RegisterCallback<ClickEvent>(_ => SetExpanded(!_expanded, notify: true));
        }

        public void SetExpanded(bool expanded, bool notify) {
            _expanded = expanded;
            _arrow.text = expanded ? "▾" : "▸";
            InspectorElements.SetVisible(Body, expanded);

            if (notify) { _toggled?.Invoke(expanded); }
        }
    }
}
