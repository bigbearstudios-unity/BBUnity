using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Gameplay.Entities.Controllers;
using BBUnity.Gameplay.Entities.Controllers.States;

namespace BBUnity.Editor.Gameplay.Entities.Controllers {

    [CustomEditor(typeof(StateController), editorForChildClasses: true)]
    public class StateControllerEditor : UnityEditor.Editor {

        private static List<Type> _cachedStateTypes;

        private SerializedProperty _statesProperty;
        private SerializedProperty _defaultStateProperty;

        public override VisualElement CreateInspectorGUI() {
            _statesProperty = serializedObject.FindProperty("_states");
            _defaultStateProperty = serializedObject.FindProperty("_defaultState");

            VisualElement root = InspectorElements.CreateDefault(serializedObject, "_states", "_defaultState", "m_Script");
            root.Add(InspectorElements.CreateHeading("States"));

            // States are pure logic with no editable data, so a row is just its title.
            var list = new ArrayListElement(
                serializedObject,
                _statesProperty.propertyPath,
                createBody: element => null,
                getTitle: (element, index) => {
                    EntityState instance = element.managedReferenceValue as EntityState;
                    return instance != null ? TypeDropdown.BuildLabel(instance.GetType()) : "(missing state)";
                },
                onRemoving: ClearDefaultStateIfRemoved
            );
            root.Add(list);

            var defaultState = new DropdownField("Default State");
            defaultState.AddToClassList("unity-base-field__aligned");
            defaultState.RegisterValueChangedCallback(change => SetDefaultState(defaultState.index));
            list.Rebuilt += () => RefreshDefaultStateSelector(defaultState);
            RefreshDefaultStateSelector(defaultState);
            root.Add(defaultState);

            Button addButton = null;
            addButton = new Button(() => {
                serializedObject.UpdateIfRequiredOrScript();
                List<Type> availableTypes = GetAvailableStateTypes().Where(t => !ContainsStateOfType(t)).ToList();
                var dropdown = new TypeDropdown(new AdvancedDropdownState(), availableTypes, type => {
                    AddState(type);
                    list.Rebuild();
                }, "State Type");
                dropdown.Show(addButton.worldBound);
            }) { text = "Add State" };
            root.Add(addButton);

            return root;
        }

        // Single-selection dropdown listing every registered state, replacing what was previously
        // a per-row "Default" toggle. A dropdown makes the exclusivity (only one default state)
        // structural rather than something the user has to maintain by hand across rows.
        private void RefreshDefaultStateSelector(DropdownField dropdown) {
            serializedObject.UpdateIfRequiredOrScript();

            var labels = new List<string> { "(None)" };
            int currentIndex = 0;

            for (int i = 0; i < _statesProperty.arraySize; i++) {
                SerializedProperty element = _statesProperty.GetArrayElementAtIndex(i);
                EntityState instance = element.managedReferenceValue as EntityState;
                if (instance == null) { continue; }

                labels.Add(TypeDropdown.BuildLabel(instance.GetType()));
                if (_defaultStateProperty.managedReferenceId == element.managedReferenceId) {
                    currentIndex = labels.Count - 1;
                }
            }

            dropdown.choices = labels;
            dropdown.SetValueWithoutNotify(labels[currentIndex]);
        }

        private void SetDefaultState(int choiceIndex) {
            serializedObject.Update();

            if (choiceIndex <= 0) {
                _defaultStateProperty.managedReferenceValue = null;
            } else {
                List<EntityState> instances = GetRegisteredStateInstances();
                if (choiceIndex - 1 >= instances.Count) { return; }
                _defaultStateProperty.managedReferenceValue = instances[choiceIndex - 1];
            }

            serializedObject.ApplyModifiedProperties();
        }

        private List<EntityState> GetRegisteredStateInstances() {
            var instances = new List<EntityState>();

            for (int i = 0; i < _statesProperty.arraySize; i++) {
                EntityState instance = _statesProperty.GetArrayElementAtIndex(i).managedReferenceValue as EntityState;
                if (instance != null) { instances.Add(instance); }
            }

            return instances;
        }

        private void ClearDefaultStateIfRemoved(SerializedProperty removedElement) {
            if (_defaultStateProperty.managedReferenceId == removedElement.managedReferenceId) {
                _defaultStateProperty.managedReferenceValue = null;
            }
        }

        private void AddState(Type type) {
            // Mirrors the one-state-per-type constraint StateController.AddState already enforces
            // at runtime (keys are derived from the state's type, and registering two states under
            // the same key throws) — catching it here means a duplicate is rejected at edit time
            // rather than only surfacing as an exception when the scene is played.
            if (ContainsStateOfType(type)) {
                Debug.LogError($"'{target.name}' already has a state of type '{TypeDropdown.BuildLabel(type)}' — only one instance per state type is supported.", target);
                return;
            }

            var instance = Activator.CreateInstance(type) as EntityState;

            serializedObject.Update();
            _statesProperty.arraySize++;
            SerializedProperty newElement = _statesProperty.GetArrayElementAtIndex(_statesProperty.arraySize - 1);
            newElement.managedReferenceValue = instance;
            serializedObject.ApplyModifiedProperties();
        }

        private bool ContainsStateOfType(Type type) {
            for (int i = 0; i < _statesProperty.arraySize; i++) {
                EntityState existing = _statesProperty.GetArrayElementAtIndex(i).managedReferenceValue as EntityState;
                if (existing != null && existing.GetType() == type) { return true; }
            }

            return false;
        }

        private static List<Type> GetAvailableStateTypes() {
            if (_cachedStateTypes == null) {
                _cachedStateTypes = TypeCache.GetTypesDerivedFrom<EntityState>()
                    .Where(t => !t.IsAbstract && !t.IsNested && !IsFromTestAssembly(t) && HasParameterlessConstructor(t))
                    .OrderBy(t => t.FullName)
                    .ToList();
            }

            return _cachedStateTypes;
        }

        private static bool HasParameterlessConstructor(Type type) {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        // Excludes test-double state classes (e.g. StateControllerTests' private nested stubs)
        // from the picker. Nested types are already caught by !t.IsNested above — this also
        // catches any future test double declared as a top-level type in a *.Tests assembly.
        private static bool IsFromTestAssembly(Type type) {
            return type.Assembly.GetName().Name.EndsWith(".Tests", StringComparison.Ordinal);
        }
    }
}
