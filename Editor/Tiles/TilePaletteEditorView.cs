using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Generation.Grid;
using BBUnity.Tiles;

namespace BBUnity.Editor.Tiles {

    /// <summary>
    /// The editor for one tile palette: the tilemap layers a level is drawn on (sorting, physics
    /// layer, collision) and, for each cell type, which rule set draws it and on which layer. It
    /// also checks the palette for gaps — a cell type nothing draws, a layer that isn't defined.
    /// </summary>
    internal class TilePaletteEditorView : VisualElement {

        private const string LayerDropdownClass = "palette-layer-dropdown";

        private readonly SerializedObject _serializedObject;
        private readonly VisualElement _warnings = new VisualElement();

        public TilePaletteEditorView(SerializedObject serializedObject) {
            _serializedObject = serializedObject;

            Add(InspectorElements.CreateHeading("Tilemap Layers"));
            Add(CreateLayers());

            Add(InspectorElements.CreateHeading("Tiles By Cell Type"));
            Add(CreateEntries());

            Add(InspectorElements.CreateHeading("Checks"));
            Add(_warnings);

            this.TrackSerializedObjectValue(serializedObject, _ => {
                RefreshLayerChoices();
                UpdateWarnings();
            });

            UpdateWarnings();
        }

        // ---- layers ----

        private VisualElement CreateLayers() {
            var container = new VisualElement();

            var list = new ArrayListElement(
                _serializedObject,
                "_layers",
                createBody: CreateLayerBody,
                getTitle: (element, index) => {
                    string name = element.FindPropertyRelative("_name").stringValue;
                    return string.IsNullOrEmpty(name) ? $"Layer {index + 1}" : name;
                }
            );
            container.Add(list);

            container.Add(new Button(() => {
                _serializedObject.Update();
                SerializedProperty layers = _serializedObject.FindProperty("_layers");
                layers.arraySize++;
                SerializedProperty added = layers.GetArrayElementAtIndex(layers.arraySize - 1);
                added.FindPropertyRelative("_name").stringValue = $"Layer {layers.arraySize}";
                added.FindPropertyRelative("_collision").enumValueIndex = (int)TilemapCollisionMode.None;
                _serializedObject.ApplyModifiedProperties();
                list.Rebuild();
            }) { text = "+ Add Layer" });

            return container;
        }

        private VisualElement CreateLayerBody(SerializedProperty layer) {
            string path = layer.propertyPath;
            var body = new VisualElement();

            body.Add(new PropertyField(layer.FindPropertyRelative("_name"), "Name"));
            body.Add(new PropertyField(layer.FindPropertyRelative("_collision"), "Collision"));

            var sorting = new DropdownField("Sorting Layer", SortingLayer.layers.Select(l => l.name).ToList(), 0);
            sorting.SetValueWithoutNotify(CurrentSortingLayer(path));
            sorting.RegisterValueChangedCallback(change => {
                _serializedObject.Update();
                _serializedObject.FindProperty(path + "._sortingLayerName").stringValue = change.newValue;
                _serializedObject.ApplyModifiedProperties();
            });
            body.Add(sorting);

            body.Add(new PropertyField(layer.FindPropertyRelative("_sortingOrder"), "Sorting Order"));

            var physics = new LayerField("Physics Layer");
            physics.BindProperty(layer.FindPropertyRelative("_physicsLayer"));
            body.Add(physics);

            return body;
        }

        private string CurrentSortingLayer(string layerPath) {
            string saved = _serializedObject.FindProperty(layerPath + "._sortingLayerName").stringValue;
            return string.IsNullOrEmpty(saved) || SortingLayer.NameToID(saved) == 0 && saved != "Default" ? "Default" : saved;
        }

        // ---- entries ----

        private VisualElement CreateEntries() {
            var container = new VisualElement();

            var list = new ArrayListElement(
                _serializedObject,
                "_entries",
                createBody: CreateEntryBody,
                getTitle: (element, index) => {
                    var cellType = element.FindPropertyRelative("_cellType").objectReferenceValue as CellTypeDefinition;
                    var ruleSet = element.FindPropertyRelative("_ruleSet").objectReferenceValue as TileRuleSetDefinition;
                    return $"{(cellType != null ? cellType.DisplayName : "(no cell type)")}  →  {(ruleSet != null ? ruleSet.name : "(no rule set)")}";
                }
            );
            container.Add(list);

            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;

            buttons.Add(new Button(() => {
                AddEntry(null);
                list.Rebuild();
            }) { text = "+ Add Entry" });

            buttons.Add(new Button(() => {
                AddMissingCellTypes();
                list.Rebuild();
            }) { text = "Add Missing Cell Types" });

            container.Add(buttons);
            return container;
        }

        private VisualElement CreateEntryBody(SerializedProperty entry) {
            string path = entry.propertyPath;
            var body = new VisualElement();

            body.Add(new PropertyField(entry.FindPropertyRelative("_cellType")));
            body.Add(new PropertyField(entry.FindPropertyRelative("_ruleSet")));

            var layer = new DropdownField("Layer", LayerNames(), 0);
            layer.AddToClassList(LayerDropdownClass);
            layer.SetValueWithoutNotify(_serializedObject.FindProperty(path + "._layerName").stringValue);
            layer.RegisterValueChangedCallback(change => {
                _serializedObject.Update();
                _serializedObject.FindProperty(path + "._layerName").stringValue = change.newValue;
                _serializedObject.ApplyModifiedProperties();
            });
            body.Add(layer);

            return body;
        }

        private void AddEntry(CellTypeDefinition cellType) {
            _serializedObject.Update();
            SerializedProperty entries = _serializedObject.FindProperty("_entries");
            entries.arraySize++;

            SerializedProperty added = entries.GetArrayElementAtIndex(entries.arraySize - 1);
            added.FindPropertyRelative("_cellType").objectReferenceValue = cellType;
            added.FindPropertyRelative("_ruleSet").objectReferenceValue = null;

            List<string> layers = LayerNames();
            added.FindPropertyRelative("_layerName").stringValue = layers.Count > 0 ? layers[0] : "Ground";
            _serializedObject.ApplyModifiedProperties();
        }

        private void AddMissingCellTypes() {
            HashSet<CellTypeDefinition> present = CellTypesInEntries();

            foreach (CellTypeDefinition cellType in AllCellTypes()) {
                if (!present.Contains(cellType)) { AddEntry(cellType); }
            }
        }

        // ---- checks ----

        private List<string> LayerNames() {
            _serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty layers = _serializedObject.FindProperty("_layers");

            var names = new List<string>();
            for (int i = 0; i < layers.arraySize; i++) { names.Add(layers.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue); }
            return names;
        }

        private void RefreshLayerChoices() {
            List<string> names = LayerNames();
            this.Query<DropdownField>(className: LayerDropdownClass).ForEach(dropdown => dropdown.choices = names);
        }

        private HashSet<CellTypeDefinition> CellTypesInEntries() {
            _serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty entries = _serializedObject.FindProperty("_entries");

            var found = new HashSet<CellTypeDefinition>();
            for (int i = 0; i < entries.arraySize; i++) {
                var cellType = entries.GetArrayElementAtIndex(i).FindPropertyRelative("_cellType").objectReferenceValue as CellTypeDefinition;
                if (cellType != null) { found.Add(cellType); }
            }

            return found;
        }

        private static List<CellTypeDefinition> AllCellTypes() {
            return AssetDatabase.FindAssets("t:" + nameof(CellTypeDefinition))
                .Select(guid => AssetDatabase.LoadAssetAtPath<CellTypeDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null)
                .ToList();
        }

        private void UpdateWarnings() {
            _warnings.Clear();

            SerializedProperty entries = _serializedObject.FindProperty("_entries");
            List<string> layers = LayerNames();
            var seen = new HashSet<CellTypeDefinition>();
            bool problem = false;

            for (int i = 0; i < entries.arraySize; i++) {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                var cellType = entry.FindPropertyRelative("_cellType").objectReferenceValue as CellTypeDefinition;
                var ruleSet = entry.FindPropertyRelative("_ruleSet").objectReferenceValue;
                string layer = entry.FindPropertyRelative("_layerName").stringValue;
                string label = $"Entry {i + 1}";

                if (cellType == null) { Warn($"{label} has no cell type."); problem = true; }
                else if (!seen.Add(cellType)) { Warn($"{label}: '{cellType.DisplayName}' already has an entry above — only the first is used."); problem = true; }

                if (ruleSet == null) { Warn($"{label} has no rule set, so nothing will be drawn for it."); problem = true; }
                if (!layers.Contains(layer)) { Warn($"{label} uses the layer '{layer}', which isn't defined above."); problem = true; }
            }

            List<string> uncovered = AllCellTypes().Where(cellType => !seen.Contains(cellType)).Select(cellType => cellType.DisplayName).ToList();
            if (uncovered.Count > 0) {
                _warnings.Add(InspectorElements.CreateHelpBox("No entry draws: " + string.Join(", ", uncovered) + ". Cells of those types will be invisible in a built level.", HelpBoxMessageType.Info));
            }

            if (!problem && uncovered.Count == 0) {
                _warnings.Add(InspectorElements.CreateHelpBox("Every cell type has an entry with a rule set and a defined layer.", HelpBoxMessageType.None));
            }
        }

        private void Warn(string message) {
            _warnings.Add(InspectorElements.CreateHelpBox(message, HelpBoxMessageType.Warning));
        }
    }
}
