using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;

namespace BBUnity.Editor {

    /// <summary>
    /// Base for the drawer of a wrapper class around a [SerializeReference] list (statistics,
    /// effectors): a foldout with each entry's own fields, a remove button per entry, and an "Add"
    /// button that offers the concrete types via the same TypeDropdown "Add Statistic" uses — a
    /// bare [SerializeReference] list has no way to choose which concrete type to create.
    ///
    /// The wrapper must expose its list under the field name returned by ListFieldName.
    /// </summary>
    public abstract class TypePickerListDrawer : PropertyDrawer {

        protected abstract string ListFieldName { get; }
        protected abstract string AddButtonLabel { get; }
        protected abstract string DropdownTitle { get; }

        protected abstract List<Type> GetAvailableTypes();

        // Lets a drawer exclude a type that's already in the list (one Health per controller).
        protected virtual bool CanAdd(Type type, SerializedProperty list) {
            return true;
        }

        // Extra text shown after an entry's type label, e.g. a generic statistic's own Name.
        protected virtual string DescribeEntry(object instance) {
            return null;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            SerializedObject serializedObject = property.serializedObject;
            string wrapperPath = property.propertyPath;
            string listPath = wrapperPath + "." + ListFieldName;
            string label = property.displayName;

            var foldout = new Foldout { value = property.isExpanded };
            foldout.RegisterValueChangedCallback(change => {
                if (change.target != foldout) { return; }

                SerializedProperty wrapper = serializedObject.FindProperty(wrapperPath);
                if (wrapper != null) { wrapper.isExpanded = change.newValue; }
            });

            var list = new ArrayListElement(
                serializedObject,
                listPath,
                createBody: element => {
                    var body = new VisualElement();
                    InspectorElements.AddChildren(body, element);
                    return body;
                },
                getTitle: (element, index) => {
                    object instance = element.managedReferenceValue;
                    string title = instance != null ? TypeDropdown.BuildLabel(instance.GetType()) : "(missing)";
                    string detail = instance != null ? DescribeEntry(instance) : null;
                    return string.IsNullOrEmpty(detail) ? title : $"{title} — {detail}";
                },
                onRemoving: null
            );

            list.Rebuilt += () => {
                serializedObject.UpdateIfRequiredOrScript();
                foldout.text = $"{label} ({serializedObject.FindProperty(listPath).arraySize})";
            };
            foldout.text = $"{label} ({property.FindPropertyRelative(ListFieldName).arraySize})";
            foldout.Add(list);

            Button addButton = null;
            addButton = new Button(() => {
                serializedObject.UpdateIfRequiredOrScript();
                SerializedProperty listProperty = serializedObject.FindProperty(listPath);

                List<Type> types = GetAvailableTypes().FindAll(t => CanAdd(t, listProperty));
                var dropdown = new TypeDropdown(new AdvancedDropdownState(), types, type => {
                    Add(serializedObject, listPath, type);
                    list.Rebuild();
                }, DropdownTitle);
                dropdown.Show(addButton.worldBound);
            }) { text = AddButtonLabel };
            foldout.Add(addButton);

            return foldout;
        }

        private static void Add(SerializedObject serializedObject, string listPath, Type type) {
            serializedObject.Update();
            SerializedProperty list = serializedObject.FindProperty(listPath);
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).managedReferenceValue = Activator.CreateInstance(type);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
