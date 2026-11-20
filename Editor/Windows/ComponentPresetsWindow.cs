using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Core;
using BBUnity.Editor.Support.ComponentManagement;
using BBUnity.Editor.Toolkit;

namespace BBUnity.Editor.Windows {

    /// <summary>
    /// Manages the component category presets used by the Inspector's component management: a list
    /// of presets on the left (the built-in BBUnity one plus any created in the project) and the
    /// selected preset's category rules on the right. The built-in preset is read-only — duplicate
    /// it to start a custom one. Which preset is in use is chosen here or in BBUnitySettings.
    /// </summary>
    public class ComponentPresetsWindow : EditorWindow {

        private ComponentCategoryPresetDefinition _selected;

        private VisualElement _list;
        private VisualElement _details;

        [MenuItem("Window/BBUnity/Component Presets")]
        private static void OpenFromMenu() {
            Open(null);
        }

        internal static void Open(ComponentCategoryPresetDefinition select) {
            var window = GetWindow<ComponentPresetsWindow>("Component Presets");
            if (select != null) { window._selected = select; }
            window.Rebuild();
        }

        public void CreateGUI() {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;

            var left = new VisualElement();
            left.style.width = 180;
            left.style.flexShrink = 0;
            left.style.paddingLeft = left.style.paddingRight = left.style.paddingTop = left.style.paddingBottom = 6;
            left.style.borderRightWidth = 1;
            left.style.borderRightColor = new Color(0f, 0f, 0f, 0.4f);

            _list = new VisualElement();
            _list.style.flexGrow = 1;
            left.Add(_list);
            root.Add(left);

            _details = new ScrollView();
            _details.style.flexGrow = 1;
            _details.style.paddingLeft = _details.style.paddingRight = _details.style.paddingTop = 6;
            root.Add(_details);

            Rebuild();
        }

        private void OnEnable() {
            EditorApplication.projectChanged += Rebuild;
        }

        private void OnDisable() {
            EditorApplication.projectChanged -= Rebuild;
        }

        private void Rebuild() {
            if (_list == null) { return; }

            if (_selected == null) { _selected = ComponentPresets.GetActive(); }

            RebuildList();
            RebuildDetails();
        }

        private void RebuildList() {
            _list.Clear();

            AddListEntry(ComponentPresets.BuiltIn);
            foreach (ComponentCategoryPresetDefinition preset in ComponentPresets.GetCustom()) { AddListEntry(preset); }

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            _list.Add(spacer);

            _list.Add(new Button(() => Select(ComponentPresets.CreateFrom(_selected, "New Preset"))) { text = "+ New Preset" });
        }

        private void AddListEntry(ComponentCategoryPresetDefinition preset) {
            string text = ComponentPresets.IsActive(preset) ? preset.name + "  ✓" : preset.name;

            var button = new Button(() => Select(preset)) { text = text };
            button.style.unityTextAlign = TextAnchor.MiddleLeft;
            if (preset == _selected) { button.style.unityFontStyleAndWeight = FontStyle.Bold; }

            _list.Add(button);
        }

        private void Select(ComponentCategoryPresetDefinition preset) {
            _selected = preset;
            Rebuild();
        }

        private void RebuildDetails() {
            _details.Clear();

            ComponentCategoryPresetDefinition preset = _selected;

            if (preset.IsBuiltIn) {
                var title = new Label($"{preset.name} (built-in)");
                title.style.unityFontStyleAndWeight = FontStyle.Bold;
                _details.Add(title);
            } else {
                var name = new TextField("Name") { value = preset.name, isDelayed = true };
                name.RegisterValueChangedCallback(change => {
                    string error = ComponentPresets.Rename(preset, change.newValue);
                    if (!string.IsNullOrEmpty(error)) { Debug.LogWarning("BBUnity: " + error); }
                    Rebuild();
                });
                _details.Add(name);
            }

            _details.Add(CreateActions(preset));

            if (preset.IsBuiltIn) {
                _details.Add(InspectorElements.CreateHelpBox("The built-in preset can't be edited here. Duplicate it to make your own.", HelpBoxMessageType.Info));
            }

            var serializedObject = new SerializedObject(preset);
            var rules = new PropertyField(serializedObject.FindProperty("_rules"), "Categories");
            rules.Bind(serializedObject);
            rules.SetEnabled(!preset.IsBuiltIn);
            _details.Add(rules);

            if (!preset.IsBuiltIn) {
                rules.TrackSerializedObjectValue(serializedObject, _ => {
                    AssetDatabase.SaveAssetIfDirty(preset);
                    ComponentPresets.NotifyChanged();
                });
            }
        }

        private VisualElement CreateActions(ComponentCategoryPresetDefinition preset) {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginTop = 4;
            row.style.marginBottom = 4;

            bool active = ComponentPresets.IsActive(preset);
            var use = new Button(() => {
                ComponentPresets.SetActive(preset);
                Rebuild();
            }) { text = active ? "In Use" : "Use This Preset" };
            use.SetEnabled(!active && BBUnitySettings.Load() != null);
            use.tooltip = BBUnitySettings.Load() == null ? "Create BBUnity Settings first (Window > BBUnity > Settings)." : string.Empty;
            row.Add(use);

            row.Add(new Button(() => Select(ComponentPresets.CreateFrom(preset, preset.name + " Copy"))) { text = "Duplicate" });

            if (!preset.IsBuiltIn) {
                row.Add(new Button(() => {
                    if (!EditorUtility.DisplayDialog("Delete Preset", $"Delete the preset '{preset.name}'?", "Delete", "Cancel")) { return; }

                    ComponentPresets.Delete(preset);
                    _selected = null;
                    Rebuild();
                }) { text = "Delete" });
            }

            return row;
        }
    }
}
