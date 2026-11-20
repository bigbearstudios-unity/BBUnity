using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;

namespace BBUnity.Editor.Generation {

    /// <summary>
    /// Helpers for drawing generation settings: the fields of a step or parameters object, with a
    /// type picker wherever a field holds one of several implementations (a room placer, shape or
    /// connector).
    /// </summary>
    internal static class GenerationEditors {

        /// <summary>Adds an editor for every visible child of the property; [SerializeReference] children get a type picker.</summary>
        public static void AddFields(VisualElement container, SerializedProperty parent) {
            foreach (SerializedProperty child in InspectorElements.VisibleChildren(parent)) {
                if (child.propertyType == SerializedPropertyType.ManagedReference) {
                    Type baseType = GetDeclaredType(child);
                    container.Add(new ManagedReferenceField(child, child.displayName, () => ConcreteTypesOf(baseType), baseType?.Name ?? "Type"));
                } else {
                    container.Add(new PropertyField(child));
                }
            }
        }

        /// <summary>The declared type of a [SerializeReference] field (not the type of what it currently holds).</summary>
        public static Type GetDeclaredType(SerializedProperty property) {
            // Unity reports it as "AssemblyName Namespace.TypeName".
            string[] parts = property.managedReferenceFieldTypename.Split(' ');
            return parts.Length == 2 ? Type.GetType($"{parts[1]}, {parts[0]}") : null;
        }

        public static List<Type> ConcreteTypesOf(Type baseType) {
            if (baseType == null) { return new List<Type>(); }

            return TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition && t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.FullName)
                .ToList();
        }
    }
}
