using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using BBUnity.Editor.Generation;
using BBUnity.Editor.Toolkit;
using BBUnity.Gameplay.Levels.Elements;

namespace BBUnity.Editor.Windows {

    /// <summary>
    /// Manages everything generation can place in a level (spawn points, exits, enemies, items,
    /// traps...) and the categories they are grouped by, without going through the Inspector.
    /// </summary>
    public class LevelElementsWindow : EditorWindow {

        [SerializeField] private string _selectedElement;
        [SerializeField] private string _selectedCategory;
        [SerializeField] private bool _showCategories;

        private DefinitionBrowser<LevelElementDefinition> _elements;
        private DefinitionBrowser<LevelElementCategoryDefinition> _categories;

        [MenuItem("Window/BBUnity/Generation/Level Elements")]
        private static void Open() {
            GetWindow<LevelElementsWindow>("Level Elements");
        }

        public void CreateGUI() {
            VisualElement root = rootVisualElement;

            _elements = new DefinitionBrowser<LevelElementDefinition>(
                "Level Element",
                element => element.DisplayName,
                element => element.Category != null ? element.Category.DisplayName : "No Category",
                DefinitionDetails.CreateElement,
                element => element.Category != null ? element.Category.PreviewColor : (Color?)null
            );

            _categories = new DefinitionBrowser<LevelElementCategoryDefinition>(
                "Category",
                category => category.DisplayName,
                null,
                DefinitionDetails.CreateCategory,
                category => category.PreviewColor
            );

            _elements.SelectByGuid(_selectedElement);
            _categories.SelectByGuid(_selectedCategory);
            _elements.Changed += () => _selectedElement = _elements.SelectedGuid;
            _categories.Changed += () => _selectedCategory = _categories.SelectedGuid;

            var toolbar = new Toolbar();
            var elementsTab = new ToolbarToggle { text = "Elements", value = !_showCategories };
            var categoriesTab = new ToolbarToggle { text = "Categories", value = _showCategories };

            elementsTab.RegisterValueChangedCallback(change => { if (change.newValue) { categoriesTab.SetValueWithoutNotify(false); ShowTab(false); } else { elementsTab.SetValueWithoutNotify(true); } });
            categoriesTab.RegisterValueChangedCallback(change => { if (change.newValue) { elementsTab.SetValueWithoutNotify(false); ShowTab(true); } else { categoriesTab.SetValueWithoutNotify(true); } });
            toolbar.Add(elementsTab);
            toolbar.Add(categoriesTab);

            root.Add(toolbar);
            root.Add(_elements);
            root.Add(_categories);

            ShowTab(_showCategories);
        }

        private void ShowTab(bool categories) {
            _showCategories = categories;

            InspectorElements.SetVisible(_elements, !categories);
            InspectorElements.SetVisible(_categories, categories);

            // The element list is grouped and coloured by category, which may have just been edited.
            if (!categories) { _elements.Reload(); }
        }
    }
}
