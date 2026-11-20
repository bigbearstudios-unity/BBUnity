using System.Collections.Generic;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BBUnity.Editor {

    /// <summary>
    /// Unity's default array "+" and "Duplicate Array Element" clone a new element from the
    /// previous one — for plain fields (int, string, ObjectReference, ...) that's a normal value
    /// copy, but for a [SerializeReference] field nested anywhere inside that element (e.g.
    /// DamageEntry.DamageValue, ReductionEntry.Reduction) it clones the *managed reference id*
    /// rather than the object, so the two elements end up editing the exact same underlying
    /// instance.
    ///
    /// Track(root, ...) runs Deduplicate over the given fields once and then again whenever the
    /// serialized object changes — it walks everything nested under each field and nulls out
    /// every occurrence of a managed reference id after its first, so nothing can stay aliased.
    /// This both prevents new aliasing (a fresh duplicate's shared field resets to "none" rather
    /// than silently sharing) and repairs data that's already aliased from before this fix
    /// existed, since it runs whenever the Inspector is shown.
    /// </summary>
    internal static class ManagedReferenceDeduplicator {

        /// <summary>De-aliases the named fields now and after every change to the object.</summary>
        public static void Track(VisualElement root, SerializedObject serializedObject, params string[] fieldNames) {
            root.TrackSerializedObjectValue(serializedObject, changed => DeduplicateFields(changed, fieldNames));

            // After the first bind, so the initial repair also reaches data aliased before this ran.
            root.schedule.Execute(() => DeduplicateFields(serializedObject, fieldNames));
        }

        private static void DeduplicateFields(SerializedObject serializedObject, string[] fieldNames) {
            serializedObject.UpdateIfRequiredOrScript();

            foreach (string field in fieldNames) {
                SerializedProperty property = serializedObject.FindProperty(field);
                if (property != null) { Deduplicate(property); }
            }
        }

        public static void Deduplicate(SerializedProperty root) {
            var seenIds = new HashSet<long>();
            bool changed = false;

            SerializedProperty iterator = root.Copy();
            SerializedProperty end = iterator.GetEndProperty();

            while (iterator.NextVisible(true) && !SerializedProperty.EqualContents(iterator, end)) {
                if (iterator.propertyType != SerializedPropertyType.ManagedReference) { continue; }
                if (iterator.managedReferenceValue == null) { continue; }

                if (!seenIds.Add(iterator.managedReferenceId)) {
                    iterator.managedReferenceValue = null;
                    changed = true;
                }
            }

            if (changed) {
                root.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
