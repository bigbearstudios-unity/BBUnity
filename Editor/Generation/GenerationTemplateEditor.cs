using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Templates;

namespace BBUnity.Editor.Generation {

    /// <summary>
    /// Edits a generation template as a recipe: its size, seed and chunk library, the parameters
    /// object, and the ordered list of steps — each step is added from a type list, reordered with
    /// the arrows, and shows its own settings (with type pickers for parts such as the room
    /// placer, shape and connector).
    /// </summary>
    [CustomEditor(typeof(GenerationTemplateDefinition), editorForChildClasses: true)]
    public class GenerationTemplateEditor : UnityEditor.Editor {

        public override VisualElement CreateInspectorGUI() {
            VisualElement root = InspectorElements.CreateDefault(serializedObject, "_parameters", "_steps", "m_Script");

            root.Add(InspectorElements.CreateHeading("Parameters"));
            root.Add(new ManagedReferenceField(
                serializedObject.FindProperty("_parameters"),
                "Type",
                () => GenerationEditors.ConcreteTypesOf(typeof(GenerationParameters)),
                "Generation Parameters"
            ));

            root.Add(InspectorElements.CreateHeading("Steps"));

            var list = new ArrayListElement(
                serializedObject,
                "_steps",
                createBody: element => {
                    var body = new VisualElement();
                    GenerationEditors.AddFields(body, element);
                    return body;
                },
                getTitle: (element, index) => (element.managedReferenceValue as GenerationStep)?.DisplayName ?? "(missing step)"
            );
            root.Add(list);

            Button addButton = null;
            addButton = new Button(() => {
                var dropdown = new TypeDropdown(new AdvancedDropdownState(), GenerationEditors.ConcreteTypesOf(typeof(GenerationStep)), type => {
                    serializedObject.Update();
                    SerializedProperty steps = serializedObject.FindProperty("_steps");
                    steps.arraySize++;
                    steps.GetArrayElementAtIndex(steps.arraySize - 1).managedReferenceValue = Activator.CreateInstance(type);
                    serializedObject.ApplyModifiedProperties();
                    list.Rebuild();
                }, "Generation Step");
                dropdown.Show(addButton.worldBound);
            }) { text = "Add Step" };
            root.Add(addButton);

            return root;
        }
    }
}
