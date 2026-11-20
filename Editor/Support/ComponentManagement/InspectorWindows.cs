using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace BBUnity.Editor.Support.ComponentManagement {

    /// <summary>
    /// The one place that reaches into Unity's internal InspectorWindow. Unity exposes no public
    /// way to add UI between the GameObject header and the component list, nor to find the
    /// Inspector windows, so this looks the window type, its tracker and its editor list up by
    /// name. Where Unity has moved them between versions (the tracker is a property of the
    /// PropertyEditor base class in Unity 6, a GetTracker method on the window before that) both
    /// are tried. If a Unity upgrade renames any of them the component management tools switch
    /// themselves off (with one warning) rather than throwing.
    /// </summary>
    internal static class InspectorWindows {

        private const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        // Fallback for the editors list if the window doesn't expose it: the element Unity puts
        // every editor (GameObject header, Transform, components...) in.
        private const string EditorsListClass = "unity-inspector-editors-list";

        private static readonly Type WindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");

        private static readonly PropertyInfo TrackerProperty = FindProperty(WindowType, "tracker");
        private static readonly MethodInfo GetTrackerMethod = FindMethod(WindowType, "GetTracker");
        private static readonly PropertyInfo EditorsElementProperty = FindProperty(WindowType, "editorsElement");

        private static bool _warned;

        public static bool IsAvailable {
            get { return WindowType != null && (TrackerProperty != null || GetTrackerMethod != null); }
        }

        public static IEnumerable<EditorWindow> All() {
            if (!IsAvailable) {
                WarnOnce(WindowType == null
                    ? "the InspectorWindow type was not found"
                    : "the InspectorWindow's tracker (a 'tracker' property or GetTracker method) was not found");
                return Enumerable.Empty<EditorWindow>();
            }

            return Resources.FindObjectsOfTypeAll(WindowType).OfType<EditorWindow>();
        }

        public static ActiveEditorTracker GetTracker(EditorWindow window) {
            if (TrackerProperty != null) { return TrackerProperty.GetValue(window) as ActiveEditorTracker; }

            return GetTrackerMethod.Invoke(window, null) as ActiveEditorTracker;
        }

        /// <summary>The element holding the window's editors, or null if Unity's layout has changed.</summary>
        /// <summary>
        /// The Editor an entry of the editors list draws (its internal EditorElement's "editor"
        /// property), or null for anything else in the list, such as our own panel.
        /// </summary>
        public static UnityEditor.Editor GetEditor(VisualElement entry) {
            PropertyInfo property = entry.GetType().GetProperty("editor", AnyInstance);
            return property != null && property.PropertyType == typeof(UnityEditor.Editor) ? property.GetValue(entry) as UnityEditor.Editor : null;
        }

        public static VisualElement FindEditorsList(EditorWindow window) {
            VisualElement list = EditorsElementProperty?.GetValue(window) as VisualElement;
            if (list != null) { return list; }

            list = window.rootVisualElement.Q(className: EditorsListClass);
            if (list == null) { WarnOnce("the Inspector's editors list was not found"); }
            return list;
        }

        // Members may be declared on the window or on a base class (PropertyEditor), and are
        // often non-public.
        private static PropertyInfo FindProperty(Type type, string name) {
            for (; type != null; type = type.BaseType) {
                PropertyInfo property = type.GetProperty(name, AnyInstance | BindingFlags.DeclaredOnly);
                if (property != null) { return property; }
            }

            return null;
        }

        private static MethodInfo FindMethod(Type type, string name) {
            for (; type != null; type = type.BaseType) {
                MethodInfo method = type.GetMethod(name, AnyInstance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
                if (method != null) { return method; }
            }

            return null;
        }

        private static void WarnOnce(string reason) {
            if (_warned) { return; }

            _warned = true;
            Debug.LogWarning($"BBUnity component management is inactive: {reason}. This is likely caused by a Unity version change to the Inspector's internals.");
        }
    }
}
