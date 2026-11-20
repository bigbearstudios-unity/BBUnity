using System;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

using BBUnity.Editor.Toolkit;
using BBUnity.Tiles;

namespace BBUnity.Editor.Tiles {

    /// <summary>
    /// The editor for one tile rule set: its fallback tile, a wizard that builds the rules from a
    /// standard tile layout, the rules themselves (a 3 x 3 neighbour grid and the tiles each
    /// produces) and a live preview. Rules are matched top to bottom; the first match wins.
    /// </summary>
    internal class TileRuleSetEditorView : VisualElement {

        private readonly TileRuleSetDefinition _ruleSet;
        private readonly SerializedObject _serializedObject;
        private readonly SerializedObject _wizardObject;
        private readonly Action _rebuild;

        private TileRulePreview _preview;

        public TileRuleSetEditorView(TileRuleSetDefinition ruleSet, SerializedObject serializedObject, SerializedObject wizardObject, Action rebuild) {
            _ruleSet = ruleSet;
            _serializedObject = serializedObject;
            _wizardObject = wizardObject;
            _rebuild = rebuild;

            Add(new PropertyField(serializedObject.FindProperty("_fallbackTile")));
            Add(new PropertyField(serializedObject.FindProperty("_outOfBoundsCountsAsSame")));

            Add(CreateWizard());

            Add(InspectorElements.CreateHeading("Rules (first match wins)"));
            Add(CreateRules());

            _preview = new TileRulePreview(ruleSet);
            _preview.style.marginTop = 8;
            Add(_preview);

            this.TrackSerializedObjectValue(serializedObject, _ => _preview.Refresh());
        }

        private VisualElement CreateRules() {
            var container = new VisualElement();

            var list = new ArrayListElement(
                _serializedObject,
                "_rules",
                createBody: CreateRuleBody,
                getTitle: (element, index) => {
                    string name = element.FindPropertyRelative("_name").stringValue;
                    return $"{index + 1}. {(string.IsNullOrEmpty(name) ? "Rule" : name)}";
                }
            );
            container.Add(list);

            container.Add(new Button(() => {
                _serializedObject.Update();
                SerializedProperty rules = _serializedObject.FindProperty("_rules");
                rules.arraySize++;
                rules.GetArrayElementAtIndex(rules.arraySize - 1).FindPropertyRelative("_name").stringValue = "New Rule";
                _serializedObject.ApplyModifiedProperties();
                list.Rebuild();
            }) { text = "+ Add Rule" });

            return container;
        }

        private VisualElement CreateRuleBody(SerializedProperty rule) {
            string path = rule.propertyPath;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            row.Add(new NeighborPicker(_serializedObject, path));

            var right = new VisualElement();
            right.style.flexGrow = 1;
            right.style.flexBasis = 0;

            right.Add(new PropertyField(rule.FindPropertyRelative("_name"), "Name"));
            right.Add(new PropertyField(rule.FindPropertyRelative("_outputs"), "Tiles"));
            row.Add(right);

            return row;
        }

        private VisualElement CreateWizard() {
            var foldout = new Foldout { text = "Build Rules From A Tile Layout", value = false };
            foldout.style.marginTop = 6;

            foldout.Add(InspectorElements.CreateHelpBox(
                "Drag your tiles into the slots, then Generate. This replaces every rule in this set. Slots you leave empty are skipped.",
                HelpBoxMessageType.Info
            ));

            string[][] layout = {
                new[] { "TopLeft", "Top", "TopRight", null, "InnerTopLeft", "InnerTopRight" },
                new[] { "Left", "Center", "Right", null, "InnerBottomLeft", "InnerBottomRight" },
                new[] { "BottomLeft", "Bottom", "BottomRight", null, "Single", null }
            };

            foreach (string[] line in layout) {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;

                foreach (string slot in line) { row.Add(CreateSlot(slot)); }
                foldout.Add(row);
            }

            foldout.Add(new Button(GenerateFromLayout) { text = "Generate Rules From Layout" });
            foldout.Add(new Label("Single is used for pieces that match nothing else (and as the fallback if none is set)."));

            foldout.Add(InspectorElements.CreateHeading("Horizontal Strip (one-way platforms)"));
            var strip = new VisualElement();
            strip.style.flexDirection = FlexDirection.Row;
            foreach (string slot in new[] { "_stripLeft", "_stripMiddle", "_stripRight", "_stripSingle" }) { strip.Add(CreateWindowSlot(slot, slot.Substring(6))); }
            foldout.Add(strip);
            foldout.Add(new Button(GenerateStrip) { text = "Generate Strip Rules" });

            return foldout;
        }

        private VisualElement CreateSlot(string field) {
            return field == null ? CreateSpacer() : CreateWindowSlot("_wizard." + field, field);
        }

        private static VisualElement CreateSpacer() {
            var spacer = new VisualElement();
            spacer.style.width = 120;
            return spacer;
        }

        // The slots are plain fields written straight to the window's serialized data. They are not
        // bound with a binding path because this view as a whole is bound to the rule set.
        private VisualElement CreateWindowSlot(string propertyPath, string title) {
            var slot = new VisualElement();
            slot.style.width = 120;
            slot.style.marginRight = 6;
            slot.style.marginBottom = 4;

            slot.Add(new Label(title));

            var field = new ObjectField { objectType = typeof(TileBase), allowSceneObjects = false, value = GetTile(propertyPath) };
            field.RegisterValueChangedCallback(change => {
                _wizardObject.Update();
                _wizardObject.FindProperty(propertyPath).objectReferenceValue = change.newValue;
                _wizardObject.ApplyModifiedProperties();
            });
            slot.Add(field);

            return slot;
        }

        private TileBase GetTile(string propertyPath) {
            _wizardObject.Update();
            return _wizardObject.FindProperty(propertyPath).objectReferenceValue as TileBase;
        }

        private void GenerateFromLayout() {
            if (!ConfirmReplace()) { return; }

            var tiles = new EdgeCornerTileSet {
                Center = GetTile("_wizard.Center"), Top = GetTile("_wizard.Top"), Bottom = GetTile("_wizard.Bottom"),
                Left = GetTile("_wizard.Left"), Right = GetTile("_wizard.Right"),
                TopLeft = GetTile("_wizard.TopLeft"), TopRight = GetTile("_wizard.TopRight"),
                BottomLeft = GetTile("_wizard.BottomLeft"), BottomRight = GetTile("_wizard.BottomRight"),
                InnerTopLeft = GetTile("_wizard.InnerTopLeft"), InnerTopRight = GetTile("_wizard.InnerTopRight"),
                InnerBottomLeft = GetTile("_wizard.InnerBottomLeft"), InnerBottomRight = GetTile("_wizard.InnerBottomRight"),
                Single = GetTile("_wizard.Single")
            };

            ApplyRules(TileRuleGenerator.FromEdgeCornerSet(tiles), tiles.Single != null ? tiles.Single : tiles.Center, "Generate Tile Rules");
        }

        private void GenerateStrip() {
            if (!ConfirmReplace()) { return; }

            TileBase left = GetTile("_stripLeft");
            TileBase middle = GetTile("_stripMiddle");
            TileBase right = GetTile("_stripRight");
            TileBase single = GetTile("_stripSingle");

            ApplyRules(TileRuleGenerator.FromHorizontalStrip(left, middle, right, single), single != null ? single : middle, "Generate Tile Rules");
        }

        private bool ConfirmReplace() {
            return _ruleSet.Rules.Count == 0 || EditorUtility.DisplayDialog("Replace Rules", $"Replace the {_ruleSet.Rules.Count} existing rule(s) in '{_ruleSet.name}'?", "Replace", "Cancel");
        }

        private void ApplyRules(System.Collections.Generic.List<TileRule> rules, TileBase fallback, string undoLabel) {
            Undo.RecordObject(_ruleSet, undoLabel);
            _ruleSet.SetRules(rules, fallback);
            EditorUtility.SetDirty(_ruleSet);
            AssetDatabase.SaveAssetIfDirty(_ruleSet);

            _rebuild();
        }
    }
}
