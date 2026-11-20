using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BBUnity.EditorAttributes;

namespace BBUnity.Entities.Building {

    /// <summary>
    /// Interprets an EntityDefinition against a GameObject: adds every root/child component each
    /// capability (and the definition itself) describes, then — in a second pass, once every component and
    /// child object exists — resolves every FieldWiring. The two-pass split is what lets a
    /// ComponentSpec from one capability safely wire a field to a child object created by a completely
    /// different block, regardless of capability order.
    ///
    /// Deliberately free of any UnityEditor dependency (field wiring uses plain reflection, not
    /// SerializedObject) — this only needs to run at edit time today, via the entity-building
    /// inspector, but nothing here stops it from also driving a fully runtime, data-driven spawn
    /// later (e.g. building a Projectile straight from its EntityDefinition with no editor involved).
    /// </summary>
    public static class EntityBuilder {

        private const BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

        public static void Build(GameObject root, EntityDefinition definition) {
            if (root == null) { throw new ArgumentNullException(nameof(root)); }
            if (definition == null) { throw new ArgumentNullException(nameof(definition)); }

            var childObjects = new Dictionary<string, GameObject>();
            var pendingWirings = new List<(Component instance, ComponentSpec spec)>();

            foreach (EntityCapabilityDefinition capability in definition.Capabilities) {
                if (capability == null) { continue; }

                ApplyComponents(root, capability.RootComponents, pendingWirings);
                ApplyChildObjects(root, capability.ChildObjects, childObjects, pendingWirings);
            }

            ApplyComponents(root, definition.Components, pendingWirings);
            ApplyChildObjects(root, definition.ChildObjects, childObjects, pendingWirings);

            ResolveWirings(root, childObjects, pendingWirings);
        }

        private static void ApplyComponents(GameObject target, IReadOnlyList<ComponentSpec> specs, List<(Component, ComponentSpec)> pendingWirings) {
            foreach (ComponentSpec spec in specs) {
                Type type = spec.Type;
                if (type == null) {
                    Debug.LogWarning($"EntityBuilder: a ComponentSpec on '{target.name}' has no resolvable type — skipped.", target);
                    continue;
                }

                Component instance = target.GetComponent(type);
                if (instance == null) { instance = target.AddComponent(type); }

                spec.Settings?.Apply(instance);
                pendingWirings.Add((instance, spec));
            }
        }

        private static void ApplyChildObjects(GameObject root, IReadOnlyList<ChildObjectSpec> specs, Dictionary<string, GameObject> childObjects, List<(Component, ComponentSpec)> pendingWirings) {
            foreach (ChildObjectSpec spec in specs) {
                if (string.IsNullOrEmpty(spec.Name)) {
                    Debug.LogWarning($"EntityBuilder: a child object spec on '{root.name}' has no name — skipped.", root);
                    continue;
                }

                GameObject child = FindOrCreateChild(root, spec.Name);
                childObjects[spec.Name] = child;

                if (spec.SetLayer) { child.layer = spec.Layer; }

                ApplyComponents(child, spec.Components, pendingWirings);
            }
        }

        private static GameObject FindOrCreateChild(GameObject root, string name) {
            Transform existing = root.transform.Find(name);
            if (existing != null) { return existing.gameObject; }

            var child = new GameObject(name);
            child.transform.SetParent(root.transform, false);
            return child;
        }

        private static void ResolveWirings(GameObject root, Dictionary<string, GameObject> childObjects, List<(Component instance, ComponentSpec spec)> pendingWirings) {
            foreach ((Component instance, ComponentSpec spec) in pendingWirings) {
                foreach (FieldWiring wiring in spec.FieldWirings) {
                    GameObject targetObject = ResolveTargetObject(root, childObjects, wiring);
                    if (targetObject == null) {
                        Debug.LogWarning($"EntityBuilder: '{instance.GetType().Name}' on '{instance.gameObject.name}' wants to wire '{wiring.FieldName}' to child '{wiring.TargetChildName}', but no such child was created by this definition.", instance);
                        continue;
                    }

                    WireField(instance, wiring.FieldName, targetObject);
                }
            }
        }

        private static GameObject ResolveTargetObject(GameObject root, Dictionary<string, GameObject> childObjects, FieldWiring wiring) {
            if (wiring.TargetsRoot) { return root; }

            childObjects.TryGetValue(wiring.TargetChildName, out GameObject target);
            return target;
        }

        private static void WireField(Component instance, string fieldName, GameObject targetObject) {
            FieldInfo field = FindField(instance.GetType(), fieldName);
            if (field == null) {
                Debug.LogWarning($"EntityBuilder: '{instance.GetType().Name}' has no field '{fieldName}' to wire.", instance);
                return;
            }

            UnityEngine.Object value = ResolveFieldValue(targetObject, field.FieldType);
            if (value == null) {
                Debug.LogWarning($"EntityBuilder: '{instance.GetType().Name}.{fieldName}' is a {field.FieldType.Name}, which couldn't be resolved from child '{targetObject.name}'.", instance);
                return;
            }

            field.SetValue(instance, value);
        }

        // Walks the type hierarchy manually: DeclaredOnly + NonPublic only finds fields declared
        // directly on the type passed in, so a field declared on a base class (as most of these
        // are) needs each base type checked in turn.
        private static FieldInfo FindField(Type type, string fieldName) {
            for (Type current = type; current != null; current = current.BaseType) {
                FieldInfo field = current.GetField(fieldName, FieldBindingFlags);
                if (field != null) { return field; }
            }

            return null;
        }

        private static UnityEngine.Object ResolveFieldValue(GameObject targetObject, Type fieldType) {
            if (fieldType == typeof(GameObject)) { return targetObject; }
            if (fieldType == typeof(Transform)) { return targetObject.transform; }
            if (typeof(Component).IsAssignableFrom(fieldType)) { return targetObject.GetComponent(fieldType); }

            return null;
        }
    }
}
