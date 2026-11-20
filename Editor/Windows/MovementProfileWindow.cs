using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Generation;
using BBUnity.Editor.Toolkit;
using BBUnity.Generation.Platformer.Movement;
using BBUnity.Generation.Platformer.Templates;

namespace BBUnity.Editor.Windows {

    /// <summary>
    /// Shows and edits how a platformer template thinks the player moves: take the numbers from the
    /// player prefab's movement controller or type them in, then see what they mean in cells — how
    /// high and far a jump reaches — and a picture of the jump arcs.
    /// </summary>
    public class MovementProfileWindow : EditorWindow {

        [SerializeField] private PlatformerLevelTemplateDefinition _template;

        private VisualElement _content;

        [MenuItem("Window/BBUnity/Generation/Movement Profile")]
        private static void Open() {
            GetWindow<MovementProfileWindow>("Movement Profile");
        }

        public void CreateGUI() {
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = root.style.paddingRight = root.style.paddingTop = 6;

            var templateField = new ObjectField("Template") { objectType = typeof(PlatformerLevelTemplateDefinition), value = _template, allowSceneObjects = false };
            templateField.RegisterValueChangedCallback(change => {
                _template = change.newValue as PlatformerLevelTemplateDefinition;
                Rebuild();
            });
            root.Add(templateField);

            _content = new ScrollView();
            _content.style.flexGrow = 1;
            root.Add(_content);

            if (_template == null) {
                string[] guids = AssetDatabase.FindAssets("t:" + nameof(PlatformerLevelTemplateDefinition));
                if (guids.Length > 0) {
                    _template = AssetDatabase.LoadAssetAtPath<PlatformerLevelTemplateDefinition>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    templateField.SetValueWithoutNotify(_template);
                }
            }

            Rebuild();
        }

        private void Rebuild() {
            _content.Clear();

            if (_template == null) {
                _content.Add(InspectorElements.CreateHelpBox("Pick a platformer template (Window > BBUnity > Generation > Install > Install Platformer Defaults creates one).", HelpBoxMessageType.Info));
                return;
            }

            if (_template.PlatformerParameters == null) {
                _content.Add(InspectorElements.CreateHelpBox("This template has no platformer parameters. Open it and set its Parameters type to Platformer Generation Parameters.", HelpBoxMessageType.Warning));
                return;
            }

            var serializedObject = new SerializedObject(_template);

            _content.Add(InspectorElements.CreateHeading("Where The Numbers Come From"));
            var movement = new PropertyField(serializedObject.FindProperty("_parameters._movement"), "Movement Source");
            _content.Add(movement);

            _content.Add(InspectorElements.CreateHeading("What They Mean In Cells"));
            var summary = new Label();
            summary.style.whiteSpace = WhiteSpace.Normal;
            _content.Add(summary);

            var arc = new JumpArcView();
            _content.Add(arc);

            var legend = new Label("Green: a full jump at top speed.  Yellow: a shorter jump.  Blue: the player's body.  Grid squares are cells.");
            legend.style.opacity = 0.7f;
            legend.style.marginTop = 2;
            _content.Add(legend);

            _content.Bind(serializedObject);

            void Update() {
                PlatformerMovementProfile profile = _template.PlatformerParameters.Movement.Resolve();
                arc.SetProfile(profile);

                summary.text =
                    $"Body: {profile.BodyWidth} × {profile.BodyHeight} cells\n" +
                    $"Highest jump: {profile.MaxJumpHeight:0.0} cells — a ledge up to {Mathf.FloorToInt(profile.MaxJumpHeight)} cell(s) higher can be jumped onto\n" +
                    $"Longest jump: {profile.MaxJumpDistance:0.0} cells across level ground — gaps of about {Mathf.Max(0, Mathf.FloorToInt(profile.MaxJumpDistance) - 1)} cell(s) can be crossed\n" +
                    $"Run speed: {profile.RunSpeed:0.0} cells/s    Jump speed: {profile.JumpSpeed:0.0} cells/s    Gravity: {profile.Gravity:0.0} cells/s²    Top fall speed: {profile.MaxFallSpeed:0.0} cells/s";
            }

            Update();
            _content.TrackSerializedObjectValue(serializedObject, _ => Update());
        }
    }
}
