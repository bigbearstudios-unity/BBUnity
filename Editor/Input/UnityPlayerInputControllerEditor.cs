using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Input;

namespace BBUnity.Editor.Input {

    [CustomEditor(typeof(UnityPlayerInputController))]
    public class UnityPlayerInputControllerEditor : UnityEditor.Editor {

        private UnityPlayerInputController Controller {
            get { return (UnityPlayerInputController)target; }
        }

        private SerializedProperty _playerInputProperty;
        private SerializedProperty _mappingsProperty;

        private VisualElement _warnings;
        private VisualElement _unmapped;

        public override VisualElement CreateInspectorGUI() {
            _playerInputProperty = serializedObject.FindProperty("_playerInput");
            _mappingsProperty = serializedObject.FindProperty("_actionMappings");

            var root = new VisualElement();
            root.Add(new PropertyField(_playerInputProperty));
            root.Add(InspectorElements.CreateHeading("Action Mappings"));

            // No add button on the list — new mappings are only added via the "Actions not yet
            // mapped" +Button/+Axis buttons below, which also stamp the action name. A bare "+"
            // would insert an element with no action name (and, since UnityActionMapping is
            // abstract, nothing valid to instantiate) rather than a usable mapping.
            //
            // Draws every visible serialized field of the mapping's concrete type (Action, and
            // whatever else that subclass declares, e.g. Buffer Threshold on a button mapping) —
            // Action is disabled since it's only ever meant to be set via those buttons, not
            // typed in by hand.
            var list = new ArrayListElement(
                serializedObject,
                _mappingsProperty.propertyPath,
                createBody: element => {
                    var body = new VisualElement();
                    foreach (SerializedProperty child in InspectorElements.VisibleChildren(element)) {
                        var field = new PropertyField(child);
                        if (child.name == "_action") { field.SetEnabled(false); }
                        body.Add(field);
                    }
                    return body;
                },
                getTitle: (element, index) => {
                    SerializedProperty action = element.FindPropertyRelative("_action");
                    return action != null && !string.IsNullOrEmpty(action.stringValue) ? action.stringValue : "(unnamed mapping)";
                }
            );
            root.Add(list);

            _warnings = new VisualElement();
            _unmapped = new VisualElement();
            root.Add(_warnings);
            root.Add(_unmapped);

            list.Rebuilt += RefreshSetupSections;
            root.TrackSerializedObjectValue(serializedObject, _ => RefreshSetupSections());
            RefreshSetupSections();

            return root;
        }

        private void RefreshSetupSections() {
            serializedObject.UpdateIfRequiredOrScript();

            _warnings.Clear();
            _unmapped.Clear();

            BuildSetupWarnings(_warnings);
            BuildPopulateFromInputActions(_unmapped);
        }

        /// <summary>
        /// Surfaces the setup problems this controller would otherwise only reveal by throwing
        /// at runtime: a missing Player Input, a Player Input with no actions asset, no mappings
        /// configured at all, or a mapping whose action name doesn't exist in the assigned asset.
        /// </summary>
        private void BuildSetupWarnings(VisualElement container) {
            var playerInput = _playerInputProperty.objectReferenceValue as UnityEngine.InputSystem.PlayerInput;

            if (playerInput == null) {
                container.Add(InspectorElements.CreateHelpBox("No Player Input component assigned — this controller will throw on Awake().", HelpBoxMessageType.Error));
                return;
            }

            if (playerInput.actions == null) {
                container.Add(InspectorElements.CreateHelpBox("The assigned Player Input has no Input Actions asset set.", HelpBoxMessageType.Error));
                return;
            }

            if (_mappingsProperty.arraySize == 0) {
                container.Add(InspectorElements.CreateHelpBox("No action mappings configured.", HelpBoxMessageType.Warning));
                return;
            }

            List<string> problemMappings = new List<string>();
            for (int i = 0; i < _mappingsProperty.arraySize; i++) {
                SerializedProperty actionNameProperty = _mappingsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("_action");
                string actionName = actionNameProperty != null ? actionNameProperty.stringValue : null;

                if (string.IsNullOrEmpty(actionName)) {
                    problemMappings.Add("(unnamed mapping)");
                } else if (playerInput.actions.FindAction(actionName) == null) {
                    problemMappings.Add(actionName);
                }
            }

            if (problemMappings.Count > 0) {
                container.Add(InspectorElements.CreateHelpBox(
                    "These mappings don't match any action in the assigned Input Actions asset: " + string.Join(", ", problemMappings),
                    HelpBoxMessageType.Warning
                ));
            }
        }

        /// <summary>
        /// Lists actions present in the assigned Player Input's actions asset that don't have a
        /// mapping yet, with a one-click way to add one — you still choose whether it should be a
        /// button or an axis mapping.
        /// </summary>
        private void BuildPopulateFromInputActions(VisualElement container) {
            var playerInput = _playerInputProperty.objectReferenceValue as UnityEngine.InputSystem.PlayerInput;
            if (playerInput == null || playerInput.actions == null) { return; }

            HashSet<string> mappedActionNames = new HashSet<string>();
            for (int i = 0; i < _mappingsProperty.arraySize; i++) {
                SerializedProperty actionNameProperty = _mappingsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("_action");
                if (actionNameProperty != null && !string.IsNullOrEmpty(actionNameProperty.stringValue)) {
                    mappedActionNames.Add(actionNameProperty.stringValue);
                }
            }

            List<string> unmappedActionNames = playerInput.actions
                .Select(action => action.name)
                .Distinct()
                .Where(name => !mappedActionNames.Contains(name))
                .ToList();

            container.Add(InspectorElements.CreateHeading("Actions not yet mapped"));

            if (unmappedActionNames.Count == 0) {
                container.Add(new Label("All actions mapped"));
                return;
            }

            foreach (string actionName in unmappedActionNames) {
                string captured = actionName;

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;

                var label = new Label(actionName);
                label.style.flexGrow = 1;
                row.Add(label);

                row.Add(new Button(() => AddMapping(captured, isButton: true)) { text = "+ Button" });
                row.Add(new Button(() => AddMapping(captured, isButton: false)) { text = "+ Axis" });

                container.Add(row);
            }
        }

        private void AddMapping(string actionName, bool isButton) {
            Undo.RecordObject(Controller, "Add Action Mapping");

            if (isButton) {
                Controller._Editor_AddButtonMapping();
            } else {
                Controller._Editor_AddAxisMapping();
            }

            serializedObject.Update();

            SerializedProperty newMapping = _mappingsProperty.GetArrayElementAtIndex(_mappingsProperty.arraySize - 1);
            SerializedProperty actionNameProperty = newMapping.FindPropertyRelative("_action");
            if (actionNameProperty != null) {
                actionNameProperty.stringValue = actionName;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
