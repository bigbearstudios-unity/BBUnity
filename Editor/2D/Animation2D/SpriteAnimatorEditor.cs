using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Animation2D;
using BBUnity.Editor.Toolkit;

namespace BBUnity.Editor.Animation2D {

    [CustomEditor(typeof(SpriteAnimator))]
    public class SpriteAnimatorEditor : UnityEditor.Editor {

        public override VisualElement CreateInspectorGUI() {
            SerializedProperty framesProperty = serializedObject.FindProperty("_frames");

            VisualElement root = InspectorElements.CreateDefault(serializedObject, "_frames");
            root.Add(InspectorElements.CreateHeading("Animation Frames"));

            var list = new ArrayListElement(
                serializedObject,
                framesProperty.propertyPath,
                createBody: element => {
                    var field = new ObjectField { objectType = typeof(Sprite), allowSceneObjects = false };
                    field.BindProperty(element);
                    return field;
                },
                getTitle: (element, index) => $"Frame {index}"
            );
            root.Add(list);

            root.Add(new Button(() => {
                serializedObject.Update();
                framesProperty.InsertArrayElementAtIndex(framesProperty.arraySize);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                list.Rebuild();
            }) { text = "Add Frame" });

            return root;
        }
    }
}
