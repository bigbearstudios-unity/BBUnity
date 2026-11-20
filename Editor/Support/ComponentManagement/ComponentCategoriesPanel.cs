using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;

namespace BBUnity.Editor.Support.ComponentManagement {

    /// <summary>
    /// Shown under a GameObject's header (see ComponentManager): one collapsible section per
    /// category the active preset sorted components into, each unfolding to the full Inspector of
    /// its components. Unity's own editors for those components are hidden, so a component
    /// appears exactly once — here. Whether a section is open is a per-session UI preference.
    /// </summary>
    internal class ComponentCategoriesPanel : VisualElement {

        private const string HiddenCategory = "Hidden";

        public GameObject Target { get; }

        private string _signature;

        public ComponentCategoriesPanel(GameObject target) {
            Target = target;
            style.marginTop = 4;
            style.marginBottom = 4;
            Rebuild();
        }

        /// <summary>Rebuilds only if the preset sorts the components differently than what is shown.</summary>
        public void Refresh() {
            if (Target == null) { return; }

            if (ComputeSignature(ComponentPresets.GroupComponents(Target)) != _signature) { Rebuild(); }
        }

        private static string ComputeSignature(List<ComponentGroup> groups) {
            var builder = new StringBuilder();

            foreach (ComponentGroup group in groups) {
                builder.Append('|').Append(group.Category);
                foreach (Component component in group.Components) { builder.Append(',').Append(component.GetInstanceID()); }
            }

            return builder.ToString();
        }

        private void Rebuild() {
            Clear();

            List<ComponentGroup> groups = ComponentPresets.GroupComponents(Target);
            _signature = ComputeSignature(groups);

            foreach (ComponentGroup group in groups) { Add(CreateCategorySection(group)); }
        }

        private VisualElement CreateCategorySection(ComponentGroup group) {
            string stateKey = $"BBUnity.ComponentCategory.{group.Category}";

            var section = new CollapsibleSection(
                SessionState.GetBool(stateKey, group.Category != HiddenCategory),
                expanded => SessionState.SetBool(stateKey, expanded)
            );

            var title = new Label(group.Category);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            section.Header.Add(title);
            section.AttachToggle(title);

            var count = new Label($"({group.Components.Count})");
            count.style.opacity = 0.6f;
            count.style.marginLeft = 4;
            section.Header.Add(count);
            section.AttachToggle(count);

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            section.Header.Add(spacer);
            section.AttachToggle(spacer);

            foreach (Component component in group.Components) { section.Body.Add(CreateComponentSection(component)); }

            return section;
        }

        private VisualElement CreateComponentSection(Component component) {
            string stateKey = $"BBUnity.ComponentSection.{component.GetInstanceID()}";

            var section = new CollapsibleSection(
                SessionState.GetBool(stateKey, true),
                expanded => SessionState.SetBool(stateKey, expanded)
            );

            var icon = new Image { image = AssetPreview.GetMiniThumbnail(component) };
            icon.style.width = 16;
            icon.style.height = 16;
            icon.style.marginRight = 4;
            section.Header.Add(icon);

            // Components that have an enabled checkbox lose Unity's header while their editor is
            // hidden, so it is offered here.
            var serializedObject = new SerializedObject(component);
            if (serializedObject.FindProperty("m_Enabled") != null) {
                var enabled = new Toggle { bindingPath = "m_Enabled" };
                enabled.style.marginRight = 2;
                enabled.Bind(serializedObject);
                section.Header.Add(enabled);
            }

            var title = new Label(ObjectNames.GetInspectorTitle(component));
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            section.Header.Add(title);
            section.AttachToggle(title);

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            section.Header.Add(spacer);
            section.AttachToggle(spacer);

            section.Header.Add(CreateMenuButton(() => BuildComponentMenu(component)));

            section.Body.Add(new InspectorElement(component));

            return section;
        }

        // Unity's own component menu belongs to the header that is hidden here, so the useful
        // parts of it are offered again.
        private static GenericMenu BuildComponentMenu(Component component) {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Reset"), false, () => {
                Undo.RecordObject(component, "Reset " + component.GetType().Name);
                Unsupported.SmartReset(component);
            });

            menu.AddItem(new GUIContent("Remove Component"), false, () => Undo.DestroyObjectImmediate(component));

            if (component is MonoBehaviour behaviour) {
                MonoScript script = MonoScript.FromMonoBehaviour(behaviour);
                if (script != null) {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Ping Script"), false, () => EditorGUIUtility.PingObject(script));
                }
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Edit Presets..."), false, () => BBUnity.Editor.Windows.ComponentPresetsWindow.Open(null));

            return menu;
        }

        private static Button CreateMenuButton(Func<GenericMenu> buildMenu) {
            Button button = null;
            button = new Button(() => buildMenu().DropDown(button.worldBound)) { text = "⋮" };
            button.style.width = 22;
            return button;
        }
    }
}
