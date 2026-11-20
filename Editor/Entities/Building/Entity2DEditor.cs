using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Entities.Building;
using BBUnity.Entities;

namespace BBUnity.Editor.Entities.Building {

    /// <summary>
    /// The entity-building UI: pick an EntityDefinition ("Entity Type"), Build it onto this
    /// GameObject, then resolve any SubclassSlots the definition's capabilities declared (game-specific
    /// pieces like EnemyAIController that are meant to be subclassed per project) — either an
    /// existing subclass already in the project, or generate a new one via SubclassGenerator.
    ///
    /// Lives on Entity2D specifically (editorForChildClasses, so it also shows for any project
    /// subclass) — Entity3D would get an equivalent inspector of its own later, reusing the same
    /// EntityDefinition/EntityCapabilityDefinition/EntityBuilder underneath; nothing about the data layer itself
    /// is 2D-specific; only the entry point is, matching the Entity2D/Entity3D split.
    /// </summary>
    [CustomEditor(typeof(Entity2D), editorForChildClasses: true)]
    public class Entity2DEditor : UnityEditor.Editor {

        private Entity Target {
            get { return target as Entity; }
        }

        private PickerField _definitionPicker;
        private Button _buildButton;
        private VisualElement _slots;

        public override VisualElement CreateInspectorGUI() {
            VisualElement root = InspectorElements.CreateDefault(serializedObject);
            root.Add(InspectorElements.CreateHeading("Entity Building"));

            List<EntityDefinition> definitions = GetAvailableDefinitions();
            if (definitions.Count == 0) {
                root.Add(InspectorElements.CreateHelpBox("No Entity Definitions found in the project. Create one via Assets > Create > BBUnity > Gameplay > Entities > Building > Entity Definition.", HelpBoxMessageType.Info));
                return root;
            }

            _definitionPicker = new PickerField("Entity Type", picker => {
                var menu = new GenericMenu();
                foreach (EntityDefinition definition in definitions) {
                    EntityDefinition captured = definition;
                    menu.AddItem(new GUIContent(definition.DisplayName), definition == Target.SourceDefinition, () => {
                        Target.SourceDefinition = captured;
                        EditorUtility.SetDirty(Target);
                        Refresh();
                    });
                }
                menu.DropDown(picker.ButtonBounds);
            });
            root.Add(_definitionPicker);

            _buildButton = new Button(() => {
                EntityDefinition selected = Target.SourceDefinition;
                if (selected == null) { return; }

                EntityBuilder.Build(Target.gameObject, selected);
                Target.SourceDefinition = selected;
                EditorUtility.SetDirty(Target);
                Refresh();
            });
            root.Add(_buildButton);

            _slots = new VisualElement();
            root.Add(_slots);

            Refresh();
            return root;
        }

        private void Refresh() {
            EntityDefinition selected = Target.SourceDefinition;

            _definitionPicker.Text = selected != null ? selected.DisplayName : "(choose Entity Type)";
            _buildButton.text = selected != null ? $"Build '{selected.DisplayName}'" : "Build";
            _buildButton.SetEnabled(selected != null);

            _slots.Clear();
            if (selected != null) { BuildSubclassSlots(selected); }
        }

        private void BuildSubclassSlots(EntityDefinition definition) {
            List<SubclassSlot> pending = definition.AllSubclassSlots()
                .Where(slot => slot.BaseType != null && Target.GetComponent(slot.BaseType) == null)
                .ToList();

            if (pending.Count == 0) { return; }

            _slots.Add(InspectorElements.CreateHeading("Game-Specific Pieces Still Needed"));

            foreach (SubclassSlot slot in pending) {
                _slots.Add(CreateSubclassSlot(slot, definition.DisplayName));
            }
        }

        private VisualElement CreateSubclassSlot(SubclassSlot slot, string entityName) {
            Type baseType = slot.BaseType;

            var container = new VisualElement();
            container.Add(InspectorElements.CreateHelpBox($"{TypeDropdown.BuildLabel(baseType)} — subclass required, none attached yet.", HelpBoxMessageType.Warning));

            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;

            Button useExisting = null;
            useExisting = new Button(() => {
                List<Type> subclasses = TypeCache.GetTypesDerivedFrom(baseType)
                    .Where(t => !t.IsAbstract)
                    .OrderBy(t => t.FullName)
                    .ToList();

                if (subclasses.Count == 0) {
                    EditorUtility.DisplayDialog("No Subclasses Found", $"No existing subclasses of {baseType.Name} were found — try Create New instead.", "OK");
                    return;
                }

                var dropdown = new TypeDropdown(new AdvancedDropdownState(), subclasses, type => {
                    Target.gameObject.AddComponent(type);
                    Refresh();
                }, $"{baseType.Name} Subclass");
                dropdown.Show(useExisting.worldBound);
            }) { text = "Use Existing..." };
            useExisting.style.flexGrow = 1;

            var createNew = new Button(() => SubclassGenerator.CreateSubclass(baseType, slot.SuggestedName(entityName))) { text = "Create New..." };
            createNew.style.flexGrow = 1;

            buttons.Add(useExisting);
            buttons.Add(createNew);
            container.Add(buttons);

            return container;
        }

        private static List<EntityDefinition> GetAvailableDefinitions() {
            return AssetDatabase.FindAssets("t:EntityDefinition")
                .Select(guid => AssetDatabase.LoadAssetAtPath<EntityDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(definition => definition != null)
                .OrderBy(definition => definition.DisplayName)
                .ToList();
        }
    }
}
