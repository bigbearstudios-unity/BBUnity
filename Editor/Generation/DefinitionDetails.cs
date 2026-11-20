using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Generation.Placement;
using BBUnity.Generation.Rooms;
using BBUnity.World.Elements;

namespace BBUnity.Editor.Generation {

    /// <summary>The type-specific fields of the level element, category and room type editors.</summary>
    internal static class DefinitionDetails {

        public static VisualElement CreateElement(LevelElementDefinition element, SerializedObject serializedObject) {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty("_displayName"), "Display Name"));
            root.Add(new PropertyField(serializedObject.FindProperty("_category")));

            SerializedProperty prefab = serializedObject.FindProperty("_prefab");
            root.Add(new PropertyField(prefab));
            HelpBox noPrefab = InspectorElements.CreateHelpBox("This element has no prefab, so nothing will appear in the level for it.", HelpBoxMessageType.Warning);
            root.Add(noPrefab);
            InspectorElements.SetVisible(noPrefab, prefab.objectReferenceValue == null);
            root.TrackPropertyValue(prefab, changed => InspectorElements.SetVisible(noPrefab, changed.objectReferenceValue == null));

            root.Add(InspectorElements.CreateHeading("Placement"));

            SerializedProperty surface = serializedObject.FindProperty("_surface");
            SerializedProperty clearance = serializedObject.FindProperty("_clearance");
            root.Add(new PropertyField(surface));
            root.Add(new PropertyField(clearance));

            var summary = new Label();
            summary.style.whiteSpace = WhiteSpace.Normal;
            summary.style.opacity = 0.7f;
            summary.style.marginBottom = 4;
            root.Add(summary);

            void UpdateSummary() {
                summary.text = DescribePlacement((PlacementSurface)surface.enumValueIndex, clearance.vector2IntValue);
            }

            UpdateSummary();
            root.TrackPropertyValue(surface, _ => UpdateSummary());
            root.TrackPropertyValue(clearance, _ => UpdateSummary());

            root.Add(new PropertyField(serializedObject.FindProperty("_minSpacing")));
            root.Add(new PropertyField(serializedObject.FindProperty("_cost")));
            root.Add(new PropertyField(serializedObject.FindProperty("_weight")));

            root.Add(InspectorElements.CreateHeading("Allowed Room Types"));
            root.Add(InspectorElements.CreateHelpBox("Tick the room types this may be placed in. With none ticked it can go in any room.", HelpBoxMessageType.None));
            root.Add(CreateRoomTypeToggles(serializedObject));

            return root;
        }

        public static VisualElement CreateCategory(LevelElementCategoryDefinition category, SerializedObject serializedObject) {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty("_displayName"), "Display Name"));
            root.Add(new PropertyField(serializedObject.FindProperty("_previewColor")));

            root.Add(InspectorElements.CreateHeading("Elements In This Category"));
            List<LevelElementDefinition> members = FindAll<LevelElementDefinition>().Where(element => element.Category == category).ToList();
            AddNames(root, members.Select(element => element.DisplayName), "No elements use this category yet.");

            return root;
        }

        public static VisualElement CreateRoomType(RoomTypeDefinition roomType, SerializedObject serializedObject) {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty("_displayName"), "Display Name"));
            root.Add(new PropertyField(serializedObject.FindProperty("_previewColor")));

            List<LevelElementDefinition> elements = FindAll<LevelElementDefinition>();

            root.Add(InspectorElements.CreateHeading("Elements Allowed In This Room Type"));
            AddNames(root, elements.Where(element => element.AllowedRoomTypes.Contains(roomType)).Select(element => element.DisplayName), "No element is limited to this room type.");

            int anywhere = elements.Count(element => element.AllowedRoomTypes.Count == 0);
            if (anywhere > 0) {
                var note = new Label($"{anywhere} element(s) are not limited to any room type and can also appear here.");
                note.style.opacity = 0.7f;
                note.style.whiteSpace = WhiteSpace.Normal;
                root.Add(note);
            }

            return root;
        }

        private static VisualElement CreateRoomTypeToggles(SerializedObject serializedObject) {
            var container = new VisualElement();
            SerializedProperty allowed = serializedObject.FindProperty("_allowedRoomTypes");
            List<RoomTypeDefinition> roomTypes = FindAll<RoomTypeDefinition>().OrderBy(roomType => roomType.DisplayName).ToList();

            if (roomTypes.Count == 0) {
                container.Add(new Label("There are no room types in the project yet."));
                return container;
            }

            foreach (RoomTypeDefinition roomType in roomTypes) {
                RoomTypeDefinition captured = roomType;
                var toggle = new Toggle(roomType.DisplayName) { value = IndexOf(allowed, roomType) >= 0 };

                toggle.RegisterValueChangedCallback(change => {
                    serializedObject.Update();
                    int index = IndexOf(allowed, captured);

                    if (change.newValue && index < 0) {
                        allowed.arraySize++;
                        allowed.GetArrayElementAtIndex(allowed.arraySize - 1).objectReferenceValue = captured;
                    } else if (!change.newValue && index >= 0) {
                        // Deleting an object reference first clears it; the second call removes the slot.
                        allowed.GetArrayElementAtIndex(index).objectReferenceValue = null;
                        allowed.DeleteArrayElementAtIndex(index);
                    }

                    serializedObject.ApplyModifiedProperties();
                });

                container.Add(toggle);
            }

            return container;
        }

        private static int IndexOf(SerializedProperty array, Object target) {
            for (int i = 0; i < array.arraySize; i++) {
                if (array.GetArrayElementAtIndex(i).objectReferenceValue == target) { return i; }
            }

            return -1;
        }

        private static void AddNames(VisualElement container, IEnumerable<string> names, string emptyText) {
            List<string> sorted = names.OrderBy(name => name).ToList();

            if (sorted.Count == 0) {
                var empty = new Label(emptyText);
                empty.style.opacity = 0.7f;
                container.Add(empty);
                return;
            }

            foreach (string name in sorted) { container.Add(new Label("• " + name)); }
        }

        private static List<T> FindAll<T>() where T : Object {
            return AssetDatabase.FindAssets("t:" + typeof(T).Name)
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null)
                .ToList();
        }

        private static string DescribePlacement(PlacementSurface surface, Vector2Int clearance) {
            int width = Mathf.Max(1, clearance.x);
            int height = Mathf.Max(1, clearance.y);
            string area = $"{width} × {height} cell free area";

            switch (surface) {
                case PlacementSurface.Floor: return $"Stands on the ground: needs a {area} with solid ground or a platform under it.";
                case PlacementSurface.Ceiling: return $"Hangs from the ceiling: needs a {area} with a solid cell above it.";
                case PlacementSurface.Wall: return $"Sits against a wall: needs a {area} with a solid wall beside it.";
                default: return $"Floats anywhere: needs a {area}.";
            }
        }
    }
}
