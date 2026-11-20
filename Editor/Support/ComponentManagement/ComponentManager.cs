using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

using BBUnity.Core;

namespace BBUnity.Editor.Support.ComponentManagement {

    /// <summary>
    /// Drives the Inspector's component management: when it is switched on in BBUnitySettings
    /// (Component Management > Enabled), every Inspector window showing a single GameObject whose
    /// components the active preset (see ComponentPresets) puts in categories gets a
    /// ComponentCategoriesPanel under the GameObject header, and Unity's own editors for those
    /// components are hidden (ActiveEditorTracker.SetVisible — nothing about the components or
    /// scene changes, and nothing is stored on the GameObject, so switching the feature off
    /// restores the normal Inspector).
    /// </summary>
    [InitializeOnLoad]
    internal static class ComponentManager {

        private static bool _refreshScheduled;

        // Editors whose visibility this class turned off, so only those are turned back on.
        private static readonly HashSet<int> HiddenTargets = new HashSet<int>();
        private static readonly HashSet<VisualElement> HiddenEntries = new HashSet<VisualElement>();

        static ComponentManager() {
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnHeaderGUI;
            Selection.selectionChanged += ScheduleRefresh;
            Undo.undoRedoPerformed += ScheduleRefresh;
            EditorApplication.hierarchyChanged += ScheduleRefresh;
        }

        public static bool IsEnabled {
            get {
                BBUnitySettings settings = BBUnitySettings.Load();
                return settings != null && settings.ComponentManagement.Enabled;
            }
        }

        // The GameObject header is redrawn whenever the Inspector is, which makes it a cheap
        // "something may have changed" signal (including a settings toggle made elsewhere).
        private static void OnHeaderGUI(UnityEditor.Editor editor) {
            if (editor.target is GameObject) { ScheduleRefresh(); }
        }

        public static void ScheduleRefresh() {
            if (_refreshScheduled) { return; }

            _refreshScheduled = true;
            EditorApplication.delayCall += () => {
                _refreshScheduled = false;
                RefreshAll();
            };
        }

        private static void RefreshAll() {
            bool enabled = IsEnabled;

            foreach (EditorWindow window in InspectorWindows.All()) {
                ApplyTo(window, enabled);
            }
        }

        private static void ApplyTo(EditorWindow window, bool enabled) {
            ActiveEditorTracker tracker = InspectorWindows.GetTracker(window);
            if (tracker == null) { return; }

            UnityEditor.Editor[] editors = tracker.activeEditors;
            GameObject gameObject = editors.Length > 0 && editors[0].targets.Length == 1 ? editors[0].target as GameObject : null;

            List<ComponentGroup> groups = enabled && gameObject != null ? ComponentPresets.GroupComponents(gameObject) : new List<ComponentGroup>();
            var managed = new HashSet<Component>(groups.SelectMany(group => group.Components));

            ApplyVisibility(window, tracker, editors, managed);
            ApplyPanel(window, gameObject, groups.Count > 0);
            ApplyDisplay(window, managed);
        }

        private static void ApplyVisibility(EditorWindow window, ActiveEditorTracker tracker, UnityEditor.Editor[] editors, HashSet<Component> managed) {
            bool changed = false;

            for (int i = 0; i < editors.Length; i++) {
                if (!(editors[i].target is Component component)) { continue; }

                int id = component.GetInstanceID();

                if (managed.Contains(component)) {
                    if (tracker.GetVisible(i) != 0) {
                        tracker.SetVisible(i, 0);
                        changed = true;
                    }
                    HiddenTargets.Add(id);
                } else if (HiddenTargets.Remove(id)) {
                    tracker.SetVisible(i, 1);
                    changed = true;
                }
            }

            if (changed) { window.Repaint(); }
        }

        // SetVisible alone is only honored when Unity rebuilds an editor entry, so the entries
        // are also hidden directly; this runs on every refresh, which puts right anything Unity
        // re-shows in between.
        private static void ApplyDisplay(EditorWindow window, HashSet<Component> managed) {
            VisualElement list = InspectorWindows.FindEditorsList(window);
            if (list == null) { return; }

            foreach (VisualElement entry in list.Children()) {
                UnityEditor.Editor editor = InspectorWindows.GetEditor(entry);
                if (editor == null) { continue; }

                if (editor.target is Component component && managed.Contains(component)) {
                    entry.style.display = DisplayStyle.None;
                    HiddenEntries.Add(entry);
                } else if (HiddenEntries.Remove(entry)) {
                    entry.style.display = StyleKeyword.Null;
                }
            }
        }

        private static void ApplyPanel(EditorWindow window, GameObject gameObject, bool wanted) {
            VisualElement list = InspectorWindows.FindEditorsList(window);
            if (list == null) { return; }

            ComponentCategoriesPanel panel = list.Q<ComponentCategoriesPanel>();

            if (panel != null && (!wanted || panel.Target != gameObject)) {
                panel.RemoveFromHierarchy();
                panel = null;
            }

            if (!wanted) { return; }

            if (panel == null) {
                // Index 0 is the GameObject's own editor (name, tag, layer...); the panel goes under it.
                list.Insert(Mathf.Min(1, list.childCount), new ComponentCategoriesPanel(gameObject));
            } else {
                panel.Refresh();
            }
        }
    }
}
