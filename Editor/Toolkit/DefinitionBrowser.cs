using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BBUnity.Editor.Toolkit {

    /// <summary>
    /// The shell shared by the definition-managing windows: a searchable list of every asset of one
    /// type in the project on the left (optionally grouped), and the selected asset on the right
    /// under a header with its name and Ping / Duplicate / Delete. The type-specific fields come
    /// from a callback. Creating one asks where to save, defaulting to where the existing ones are.
    /// </summary>
    internal class DefinitionBrowser<T> : VisualElement where T : ScriptableObject {

        private readonly string _typeLabel;
        private readonly Func<T, string> _label;
        private readonly Func<T, string> _group;
        private readonly Func<T, Color?> _swatch;
        private readonly Func<T, SerializedObject, VisualElement> _createDetails;

        private readonly TextField _search = new TextField();
        private readonly VisualElement _list = new VisualElement();
        private readonly VisualElement _details = new VisualElement();
        private readonly Dictionary<T, Button> _entries = new Dictionary<T, Button>();

        private T _selected;

        /// <summary>Raised when the selection or anything shown for it changes.</summary>
        public event Action Changed;

        public DefinitionBrowser(string typeLabel, Func<T, string> label, Func<T, string> group, Func<T, SerializedObject, VisualElement> createDetails, Func<T, Color?> swatch = null) {
            _typeLabel = typeLabel;
            _label = label;
            _group = group;
            _createDetails = createDetails;
            _swatch = swatch;

            style.flexGrow = 1;
            style.flexDirection = FlexDirection.Row;

            var left = new VisualElement();
            left.style.width = 230;
            left.style.flexShrink = 0;
            left.style.paddingLeft = left.style.paddingRight = left.style.paddingTop = left.style.paddingBottom = 6;
            left.style.borderRightWidth = 1;
            left.style.borderRightColor = new Color(0f, 0f, 0f, 0.4f);

            _search.label = string.Empty;
            _search.tooltip = "Filter by name";
            _search.RegisterValueChangedCallback(_ => RebuildList());
            left.Add(_search);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            scroll.Add(_list);
            left.Add(scroll);

            left.Add(new Button(CreateNew) { text = $"+ New {typeLabel}" });
            Add(left);

            var right = new ScrollView();
            right.style.flexGrow = 1;
            right.style.paddingLeft = right.style.paddingRight = right.style.paddingTop = 6;
            right.Add(_details);
            Add(right);

            RegisterCallback<AttachToPanelEvent>(_ => { EditorApplication.projectChanged += Reload; Reload(); });
            RegisterCallback<DetachFromPanelEvent>(_ => EditorApplication.projectChanged -= Reload);
        }

        public T Selected {
            get { return _selected; }
        }

        public string SelectedGuid {
            get { return _selected != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_selected)) : string.Empty; }
        }

        public void SelectByGuid(string guid) {
            if (string.IsNullOrEmpty(guid)) { return; }

            var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            if (asset != null) { Select(asset); }
        }

        public void Select(T asset) {
            _selected = asset;
            RebuildList();
            RebuildDetails();
            Changed?.Invoke();
        }

        public void Reload() {
            if (_selected == null) { _selected = FindAll().FirstOrDefault(); }

            RebuildList();
            RebuildDetails();
        }

        private static List<T> FindAll() {
            return AssetDatabase.FindAssets("t:" + typeof(T).Name)
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(asset => asset != null)
                .ToList();
        }

        private void RebuildList() {
            _list.Clear();
            _entries.Clear();

            string filter = _search.value?.Trim() ?? string.Empty;
            IEnumerable<T> shown = FindAll()
                .Where(asset => filter.Length == 0 || _label(asset).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(asset => _group != null ? _group(asset) : string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(asset => _label(asset), StringComparer.OrdinalIgnoreCase);

            string currentGroup = null;
            foreach (T asset in shown) {
                string group = _group != null ? _group(asset) : null;
                if (group != null && group != currentGroup) {
                    var header = new Label(group);
                    header.style.unityFontStyleAndWeight = FontStyle.Bold;
                    header.style.marginTop = 6;
                    header.style.opacity = 0.7f;
                    _list.Add(header);
                    currentGroup = group;
                }

                _list.Add(CreateEntry(asset));
            }
        }

        private Button CreateEntry(T asset) {
            T captured = asset;
            var button = new Button(() => Select(captured));
            button.style.unityTextAlign = TextAnchor.MiddleLeft;
            button.style.flexDirection = FlexDirection.Row;
            button.style.alignItems = Align.Center;
            if (asset == _selected) { button.style.unityFontStyleAndWeight = FontStyle.Bold; }

            Color? color = _swatch?.Invoke(asset);
            if (color.HasValue) {
                var swatch = new VisualElement();
                swatch.name = "swatch";
                swatch.style.width = swatch.style.height = 10;
                swatch.style.marginRight = 6;
                swatch.style.backgroundColor = new Color(color.Value.r, color.Value.g, color.Value.b, 1f);
                button.Add(swatch);
            }

            button.Add(new Label(_label(asset)));
            _entries[asset] = button;
            return button;
        }

        // Updates just the selected entry's text and colour, so typing a name doesn't rebuild the list.
        private void RefreshSelectedEntry() {
            if (_selected == null || !_entries.TryGetValue(_selected, out Button button)) { return; }

            button.Q<Label>().text = _label(_selected);

            Color? color = _swatch?.Invoke(_selected);
            VisualElement swatch = button.Q("swatch");
            if (color.HasValue && swatch != null) { swatch.style.backgroundColor = new Color(color.Value.r, color.Value.g, color.Value.b, 1f); }

            Changed?.Invoke();
        }

        private void RebuildDetails() {
            _details.Clear();

            if (_selected == null) {
                _details.Add(InspectorElements.CreateHelpBox($"There are no {_typeLabel.ToLowerInvariant()}s yet. Use + New {_typeLabel} to create one.", HelpBoxMessageType.Info));
                return;
            }

            T asset = _selected;
            var serializedObject = new SerializedObject(asset);

            _details.Add(CreateHeader(asset));

            VisualElement fields = _createDetails(asset, serializedObject);
            fields.Bind(serializedObject);
            fields.TrackSerializedObjectValue(serializedObject, _ => RefreshSelectedEntry());
            _details.Add(fields);
        }

        private VisualElement CreateHeader(T asset) {
            var header = new VisualElement();
            header.style.marginBottom = 6;

            var name = new TextField("Asset Name") { value = asset.name, isDelayed = true };
            name.RegisterValueChangedCallback(change => {
                if (string.IsNullOrWhiteSpace(change.newValue)) { name.SetValueWithoutNotify(asset.name); return; }

                string error = AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(asset), change.newValue.Trim());
                if (!string.IsNullOrEmpty(error)) { Debug.LogWarning("BBUnity: " + error); }

                RebuildList();
                RefreshSelectedEntry();
            });
            header.Add(name);

            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.Add(new Button(() => EditorGUIUtility.PingObject(asset)) { text = "Ping" });
            buttons.Add(new Button(() => Duplicate(asset)) { text = "Duplicate" });
            buttons.Add(new Button(() => Delete(asset)) { text = "Delete" });
            header.Add(buttons);

            return header;
        }

        private void CreateNew() {
            string path = EditorUtility.SaveFilePanelInProject($"New {_typeLabel}", $"New {_typeLabel}", "asset", "Choose where to save it.", DefaultFolder());
            if (string.IsNullOrEmpty(path)) { return; }

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            Select(asset);
        }

        private void Duplicate(T asset) {
            string source = AssetDatabase.GetAssetPath(asset);
            string target = AssetDatabase.GenerateUniqueAssetPath(Path.ChangeExtension(source, null) + " Copy.asset");

            if (AssetDatabase.CopyAsset(source, target)) {
                Select(AssetDatabase.LoadAssetAtPath<T>(target));
            }
        }

        private void Delete(T asset) {
            if (!EditorUtility.DisplayDialog($"Delete {_typeLabel}", $"Delete '{asset.name}'? Anything that uses it will lose the reference.", "Delete", "Cancel")) { return; }

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
            _selected = null;
            Reload();
            Changed?.Invoke();
        }

        // Next to the assets of this type that already exist, so a project's definitions stay together.
        private static string DefaultFolder() {
            T first = FindAll().FirstOrDefault();
            if (first == null) { return "Assets"; }

            string folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(first));
            return string.IsNullOrEmpty(folder) ? "Assets" : folder.Replace('\\', '/');
        }
    }
}
