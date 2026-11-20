using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Generation;
using BBUnity.Editor.Toolkit;
using BBUnity.Gameplay.Levels.Elements;
using BBUnity.Gameplay.Levels.Generation;
using BBUnity.Generation.Grid;
using BBUnity.Generation.Parameters;
using BBUnity.Generation.Pipeline;
using BBUnity.Generation.Placement;
using BBUnity.Generation.Platformer;
using BBUnity.Generation.Platformer.Reachability;
using BBUnity.Generation.Platformer.Templates;
using BBUnity.Generation.Templates;
using BBUnity.Tiles;

namespace BBUnity.Editor.Windows {

    /// <summary>
    /// Generates levels from a template and shows them as a schematic: pick a template and a seed,
    /// press Generate, and see the walls, rooms, connections, placed elements and any problems
    /// straight away, with no tiles needed. Any parameter the run resolved can be pinned to a fixed
    /// value to test a specific case. Build In Scene then turns the result into real objects (and
    /// tilemaps, if a tile palette is given) in the open scene.
    /// </summary>
    public class LevelGeneratorWindow : EditorWindow {

        private class Pinned {
            public bool Enabled;
            public float Value;
        }

        [SerializeField] private GenerationTemplateDefinition _template;
        [SerializeField] private int _seed = 1;
        [SerializeField] private bool _randomSeed = true;
        [SerializeField] private TilePaletteDefinition _palette;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private Vector2 _origin = Vector2.zero;

        private readonly Dictionary<string, Pinned> _pinned = new Dictionary<string, Pinned>();

        private GenerationResult _result;
        private SchematicView _schematic;
        private VisualElement _templateSection;
        private VisualElement _parameters;
        private VisualElement _issues;
        private Label _summary;
        private IntegerField _seedField;
        private Button _installButton;

        [MenuItem("Window/BBUnity/Generation/Level Generator")]
        private static void Open() {
            GetWindow<LevelGeneratorWindow>("Level Generator");
        }

        public void CreateGUI() {
            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;

            var left = new ScrollView();
            left.style.width = 320;
            left.style.flexShrink = 0;
            left.style.paddingLeft = left.style.paddingRight = left.style.paddingTop = 6;
            left.style.borderRightWidth = 1;
            left.style.borderRightColor = new Color(0f, 0f, 0f, 0.4f);
            root.Add(left);

            BuildControls(left);

            var right = new VisualElement();
            right.style.flexGrow = 1;
            root.Add(right);

            right.Add(BuildViewToolbar());

            _schematic = new SchematicView();
            right.Add(_schematic);

            _summary = new Label();
            _summary.style.marginLeft = 6;
            _summary.style.marginTop = 4;
            right.Add(_summary);

            _issues = new ScrollView();
            _issues.style.maxHeight = 140;
            right.Add(_issues);

            RebuildTemplateSection();
        }

        private void BuildControls(VisualElement container) {
            var templateField = new ObjectField("Template") { objectType = typeof(GenerationTemplateDefinition), value = _template, allowSceneObjects = false };
            templateField.RegisterValueChangedCallback(change => {
                _template = change.newValue as GenerationTemplateDefinition;
                _cellSize = CellSizeOf(_template);
                RebuildTemplateSection();
            });
            container.Add(templateField);

            _installButton = new Button(() => {
                PlatformerLevelTemplateDefinition installed = PlatformerDefaultsInstaller.InstallInteractive();
                if (installed == null) { return; }

                _template = installed;
                templateField.SetValueWithoutNotify(_template);
                _cellSize = CellSizeOf(_template);
                RebuildTemplateSection();
                UpdateInstallButton();
            }) { text = "Install Platformer Defaults..." };
            container.Add(_installButton);
            UpdateInstallButton();

            container.Add(InspectorElements.CreateHeading("Generate"));

            _seedField = new IntegerField("Seed") { value = _seed };
            _seedField.RegisterValueChangedCallback(change => _seed = change.newValue);
            container.Add(_seedField);

            var random = new Toggle("New seed each time") { value = _randomSeed };
            random.RegisterValueChangedCallback(change => _randomSeed = change.newValue);
            container.Add(random);

            container.Add(new Button(Generate) { text = "Generate" });

            _parameters = new VisualElement();
            container.Add(_parameters);

            container.Add(InspectorElements.CreateHeading("Build In Scene"));

            var palette = new ObjectField("Tile Palette") { objectType = typeof(TilePaletteDefinition), value = _palette, allowSceneObjects = false };
            palette.RegisterValueChangedCallback(change => _palette = change.newValue as TilePaletteDefinition);
            container.Add(palette);

            var cellSize = new FloatField("Cell Size") { value = _cellSize, tooltip = "World units per grid cell." };
            cellSize.RegisterValueChangedCallback(change => _cellSize = Mathf.Max(0.01f, change.newValue));
            container.Add(cellSize);

            var origin = new Vector2Field("Origin") { value = _origin };
            origin.RegisterValueChangedCallback(change => _origin = change.newValue);
            container.Add(origin);

            container.Add(new Button(BuildInScene) { text = "Build In Scene" });
            container.Add(new Button(ClearFromScene) { text = "Clear Generated From Scene" });

            container.Add(InspectorElements.CreateHeading("Template"));
            _templateSection = new VisualElement();
            container.Add(_templateSection);
        }

        private VisualElement BuildViewToolbar() {
            var toolbar = new Toolbar();

            ToolbarToggle Toggle(string text, bool value, Action<bool> set) {
                var toggle = new ToolbarToggle { text = text, value = value };
                toggle.RegisterValueChangedCallback(change => {
                    set(change.newValue);
                    _schematic.MarkDirtyRepaint();
                });
                return toggle;
            }

            toolbar.Add(Toggle("Rooms", true, value => _schematic.ShowRooms = value));
            toolbar.Add(Toggle("Connections", true, value => _schematic.ShowConnections = value));
            toolbar.Add(Toggle("Placements", true, value => _schematic.ShowPlacements = value));
            toolbar.Add(Toggle("Reachable", true, value => _schematic.ShowReachable = value));
            toolbar.Add(new ToolbarSpacer { flex = true });
            toolbar.Add(new ToolbarButton(() => _schematic.Fit()) { text = "Fit" });

            return toolbar;
        }

        private void OnEnable() {
            EditorApplication.projectChanged += UpdateInstallButton;
        }

        private void OnDisable() {
            EditorApplication.projectChanged -= UpdateInstallButton;
        }

        // The button is only offered while there are no platformer defaults in the project.
        private void UpdateInstallButton() {
            if (_installButton != null) { InspectorElements.SetVisible(_installButton, !PlatformerDefaultsInstaller.IsInstalled); }
        }

        private void RebuildTemplateSection() {
            if (_templateSection == null) { return; }

            _templateSection.Clear();
            if (_template == null) {
                _templateSection.Add(InspectorElements.CreateHelpBox("Pick a generation template, or install the platformer defaults (Window > BBUnity > Generation > Install) to get one to start from.", HelpBoxMessageType.Info));
                return;
            }

            _templateSection.Add(new InspectorElement(_template));
        }

        private static float CellSizeOf(GenerationTemplateDefinition template) {
            var platformer = template as PlatformerLevelTemplateDefinition;
            return platformer != null && platformer.PlatformerParameters != null ? platformer.PlatformerParameters.Movement.CellSize : 1f;
        }

        private void Generate() {
            if (_template == null) {
                _summary.text = "Pick a template first.";
                return;
            }

            if (_randomSeed) {
                _seed = new System.Random().Next(1, 1000000);
                _seedField.SetValueWithoutNotify(_seed);
            }

            var overrides = new ParameterOverrideSet();
            foreach (KeyValuePair<string, Pinned> pinned in _pinned) {
                if (!pinned.Value.Enabled) { continue; }

                overrides.SetInt(pinned.Key, Mathf.RoundToInt(pinned.Value.Value));
                overrides.SetFloat(pinned.Key, pinned.Value.Value);
            }

            _result = _template.Generate(new GenerationOptions { Seed = _seed, Overrides = overrides });

            _schematic.SetResult(_result, ComputeReachable(), refit: true);
            RebuildParameters();
            RebuildSummary();
        }

        private HashSet<Vector2Int> ComputeReachable() {
            var platformer = _template as PlatformerLevelTemplateDefinition;
            if (platformer == null || platformer.PlatformerParameters == null || _result.Grid == null) { return null; }

            try {
                PlatformerGenerationParameters parameters = platformer.PlatformerParameters;
                PlacementRecord spawn = _result.Placements.Find(placement => placement.Placeable == parameters.PlayerSpawn);
                if (spawn == null) { return null; }

                return new PlatformerReachability(_result.Grid.Main, parameters.Movement.Resolve()).ReachableFrom(new[] { spawn.Cell });
            } catch (Exception exception) {
                Debug.LogWarning("BBUnity: could not work out reachability for the preview: " + exception.Message);
                return null;
            }
        }

        private void RebuildSummary() {
            int errors = 0, warnings = 0;
            foreach (GenerationIssue issue in _result.Issues) {
                if (issue.Severity == IssueSeverity.Error) { errors++; } else if (issue.Severity == IssueSeverity.Warning) { warnings++; }
            }

            _summary.text = $"Seed {_result.Seed}  |  {_result.Rooms.Rooms.Count} rooms  |  {_result.Placements.Count} placed  |  {errors} errors, {warnings} warnings";

            _issues.Clear();
            foreach (GenerationIssue issue in _result.Issues) {
                HelpBoxMessageType type = issue.Severity == IssueSeverity.Error ? HelpBoxMessageType.Error
                    : issue.Severity == IssueSeverity.Warning ? HelpBoxMessageType.Warning : HelpBoxMessageType.Info;

                string where = issue.Cell.HasValue ? $" (cell {issue.Cell.Value.x}, {issue.Cell.Value.y})" : string.Empty;
                _issues.Add(InspectorElements.CreateHelpBox(issue.Message + where, type));
            }
        }

        // The parameters the last run resolved, each with a switch to pin it to a fixed value.
        private void RebuildParameters() {
            _parameters.Clear();
            if (_result.Parameters.Count == 0) { return; }

            _parameters.Add(InspectorElements.CreateHeading("Pin Parameters"));

            foreach (ResolvedParameter parameter in _result.Parameters) {
                if (!_pinned.TryGetValue(parameter.Key, out Pinned pinned)) {
                    pinned = new Pinned { Value = parameter.Value };
                    _pinned[parameter.Key] = pinned;
                }

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;

                var toggle = new Toggle { value = pinned.Enabled };
                toggle.RegisterValueChangedCallback(change => pinned.Enabled = change.newValue);
                row.Add(toggle);

                var label = new Label(parameter.Key);
                label.style.flexGrow = 1;
                label.style.flexBasis = 0;
                row.Add(label);

                float shown = pinned.Enabled ? pinned.Value : parameter.Value;
                if (parameter.IsInt) {
                    var field = new IntegerField { value = Mathf.RoundToInt(shown) };
                    field.style.width = 70;
                    field.RegisterValueChangedCallback(change => { pinned.Value = change.newValue; toggle.value = true; });
                    row.Add(field);
                } else {
                    var field = new FloatField { value = shown };
                    field.style.width = 70;
                    field.RegisterValueChangedCallback(change => { pinned.Value = change.newValue; toggle.value = true; });
                    row.Add(field);
                }

                _parameters.Add(row);
            }
        }

        private void BuildInScene() {
            if (_result == null || _result.Grid == null) {
                _summary.text = "Generate a level first.";
                return;
            }

            ClearFromScene();

            GeneratedLevelRoot root = LevelBuilder.Build(
                _result,
                new GridToWorld(_origin, _cellSize),
                _template != null ? _template.name : "Generated",
                null,
                new EditorElementInstantiator(),
                _palette
            );

            Selection.activeGameObject = root.gameObject;
            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
        }

        private void ClearFromScene() {
            foreach (GeneratedLevelRoot existing in FindObjectsByType<GeneratedLevelRoot>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                Undo.DestroyObjectImmediate(existing.gameObject);
            }
        }
    }
}
