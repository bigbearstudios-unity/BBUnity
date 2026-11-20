using System;
using System.Collections.Generic;

using UnityEngine;

namespace BBUnity.Editor.Support.ComponentManagement {

    /// <summary>
    /// One way a rule recognizes a component: by type (the type itself or anything derived from
    /// it, so "Collider2D" covers every 2D collider) or by namespace (the namespace itself or
    /// anything below it, so "Unity.Cinemachine" covers all of Cinemachine).
    /// </summary>
    [Serializable]
    public class ComponentMatch {

        public enum MatchKind {
            Type,
            Namespace
        }

        // Type names are resolved by scanning every loaded assembly, like UnityComponentTypeReference,
        // so nobody has to know which module assembly a built-in Unity type lives in.
        private static readonly Dictionary<string, Type> ResolvedTypes = new Dictionary<string, Type>();

        [SerializeField]
        private MatchKind _kind = MatchKind.Type;

        [SerializeField]
        private string _typeName;

        [SerializeField]
        private string _namespace;

        public static ComponentMatch ForType(Type type) {
            return new ComponentMatch { _kind = MatchKind.Type, _typeName = type.FullName };
        }

        public static ComponentMatch ForNamespace(string namespacePrefix) {
            return new ComponentMatch { _kind = MatchKind.Namespace, _namespace = namespacePrefix };
        }

        public bool Matches(Type type) {
            if (type == null) { return false; }

            if (_kind == MatchKind.Namespace) {
                if (string.IsNullOrWhiteSpace(_namespace) || type.Namespace == null) { return false; }

                string prefix = _namespace.Trim();
                return type.Namespace == prefix || type.Namespace.StartsWith(prefix + ".", StringComparison.Ordinal);
            }

            Type target = Resolve(_typeName);
            return target != null && target.IsAssignableFrom(type);
        }

        private static Type Resolve(string typeName) {
            if (string.IsNullOrEmpty(typeName)) { return null; }
            if (ResolvedTypes.TryGetValue(typeName, out Type cached)) { return cached; }

            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                Type type = assembly.GetType(typeName);
                if (type != null) {
                    ResolvedTypes[typeName] = type;
                    return type;
                }
            }

            return null;
        }
    }
}
