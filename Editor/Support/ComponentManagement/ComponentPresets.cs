using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

using BBUnity.Core;

namespace BBUnity.Editor.Support.ComponentManagement {

    /// <summary>One category's components on a GameObject, as the active preset sorts them.</summary>
    internal class ComponentGroup {
        public string Category;
        public List<Component> Components = new List<Component>();
    }

    /// <summary>
    /// Finds, creates and selects component category presets. The built-in BBUnity preset has the
    /// id "" (so a fresh BBUnitySettings uses it); a custom preset's id is its asset GUID, which
    /// is what BBUnitySettings > Component Management > Preset stores.
    /// </summary>
    internal static class ComponentPresets {

        public const string PresetFolder = "Assets/Editor/BBUnity/ComponentPresets";
        private const string PresetPropertyPath = "_componentManagement._preset";

        public static ComponentCategoryPresetDefinition BuiltIn {
            get { return BuiltInComponentPresets.BBUnity; }
        }

        public static List<ComponentCategoryPresetDefinition> GetCustom() {
            return AssetDatabase.FindAssets("t:" + nameof(ComponentCategoryPresetDefinition))
                .Select(guid => AssetDatabase.LoadAssetAtPath<ComponentCategoryPresetDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(preset => preset != null)
                .OrderBy(preset => preset.name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static string GetId(ComponentCategoryPresetDefinition preset) {
            return preset == null || preset.IsBuiltIn ? string.Empty : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(preset));
        }

        /// <summary>The preset with this id; the built-in one if the id is empty or no longer exists.</summary>
        public static ComponentCategoryPresetDefinition GetById(string id) {
            if (!string.IsNullOrEmpty(id)) {
                var preset = AssetDatabase.LoadAssetAtPath<ComponentCategoryPresetDefinition>(AssetDatabase.GUIDToAssetPath(id));
                if (preset != null) { return preset; }
            }

            return BuiltIn;
        }

        public static string ActiveId {
            get {
                BBUnitySettings settings = BBUnitySettings.Load();
                return settings != null ? settings.ComponentManagement.Preset : string.Empty;
            }
        }

        public static ComponentCategoryPresetDefinition GetActive() {
            return GetById(ActiveId);
        }

        public static bool IsActive(ComponentCategoryPresetDefinition preset) {
            return GetActive() == preset;
        }

        /// <summary>Makes a preset the one in use. Needs a BBUnitySettings asset to store the choice in.</summary>
        public static void SetActive(ComponentCategoryPresetDefinition preset) {
            BBUnitySettings settings = BBUnitySettings.Load();
            if (settings == null) {
                Debug.LogWarning("BBUnity: there are no BBUnity Settings to store the preset in. Create them from Window > BBUnity > Settings.");
                return;
            }

            var serializedObject = new SerializedObject(settings);
            serializedObject.FindProperty(PresetPropertyPath).stringValue = GetId(preset);
            serializedObject.ApplyModifiedProperties();

            NotifyChanged();
        }

        /// <summary>Creates a new preset asset as a copy of another one.</summary>
        public static ComponentCategoryPresetDefinition CreateFrom(ComponentCategoryPresetDefinition source, string name) {
            EnsureFolder();

            var preset = ScriptableObject.CreateInstance<ComponentCategoryPresetDefinition>();
            if (source != null) { preset.CopyRulesFrom(source); }

            string path = AssetDatabase.GenerateUniqueAssetPath($"{PresetFolder}/{name}.asset");
            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();

            NotifyChanged();
            return preset;
        }

        public static string Rename(ComponentCategoryPresetDefinition preset, string newName) {
            if (string.IsNullOrWhiteSpace(newName)) { return "A preset needs a name."; }
            if (string.Equals(newName.Trim(), BuiltInComponentPresets.BBUnityName, StringComparison.OrdinalIgnoreCase)) {
                return $"'{BuiltInComponentPresets.BBUnityName}' is the built-in preset's name.";
            }

            string error = AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(preset), newName.Trim());
            if (string.IsNullOrEmpty(error)) { NotifyChanged(); }
            return error;
        }

        public static void Delete(ComponentCategoryPresetDefinition preset) {
            if (preset == null || preset.IsBuiltIn) { return; }

            bool wasActive = IsActive(preset);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(preset));
            if (wasActive) { SetActive(BuiltIn); }

            NotifyChanged();
        }

        /// <summary>Anything that changes what the Inspector should show asks it to refresh.</summary>
        public static void NotifyChanged() {
            ComponentManager.ScheduleRefresh();
        }

        /// <summary>
        /// A GameObject's components that the active preset puts in a category, grouped, in the
        /// preset's category order and the GameObject's own component order within each.
        /// Transform and missing scripts are never grouped.
        /// </summary>
        public static List<ComponentGroup> GroupComponents(GameObject gameObject) {
            ComponentCategoryPresetDefinition preset = GetActive();
            var groups = new Dictionary<string, ComponentGroup>();

            foreach (Component component in gameObject.GetComponents<Component>()) {
                if (component == null || component is Transform) { continue; }

                string category = preset.CategoryFor(component.GetType());
                if (category == null) { continue; }

                if (!groups.TryGetValue(category, out ComponentGroup group)) {
                    group = new ComponentGroup { Category = category };
                    groups[category] = group;
                }

                group.Components.Add(component);
            }

            return preset.CategoryOrder().Where(groups.ContainsKey).Select(name => groups[name]).ToList();
        }

        private static void EnsureFolder() {
            string parent = "Assets";
            foreach (string segment in PresetFolder.Substring("Assets/".Length).Split('/')) {
                string path = parent + "/" + segment;
                if (!AssetDatabase.IsValidFolder(path)) { AssetDatabase.CreateFolder(parent, segment); }
                parent = path;
            }
        }
    }
}
