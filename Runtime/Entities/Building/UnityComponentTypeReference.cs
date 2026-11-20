using System;
using UnityEngine;

namespace BBUnity.Entities.Building {

    /// <summary>
    /// A designer-picked reference to one of Unity's own built-in Component types (Rigidbody2D,
    /// BoxCollider2D, ...) — used only by BBUnitySettings.Entities.AdditionalComponentTypes. Kept
    /// separate from ComponentTypeReference (which backs ComponentSpec/SubclassSlot and covers
    /// BBUnity's own types too) because the stored value is deliberately just "Namespace.Type"
    /// with no assembly — Unity's built-in components are spread across many small module
    /// assemblies (UnityEngine.Physics2DModule, UnityEngine.PhysicsModule, ...) that aren't worth
    /// asking anyone to know or type, so this resolves by scanning every loaded assembly instead.
    /// </summary>
    [Serializable]
    public class UnityComponentTypeReference {

        [SerializeField]
        private string _typeName;

        public Type ResolveType() {
            if (string.IsNullOrEmpty(_typeName)) { return null; }

            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                Type type = assembly.GetType(_typeName);
                if (type != null) { return type; }
            }

            Debug.LogWarning($"UnityComponentTypeReference could not resolve '{_typeName}' — the type may have been renamed or removed since this was picked. Re-pick it in the Inspector.");
            return null;
        }
    }
}
