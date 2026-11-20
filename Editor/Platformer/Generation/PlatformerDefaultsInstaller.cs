using System;

using UnityEngine;
using UnityEditor;

using BBUnity.Generation.Grid;
using BBUnity.Platformer.Generation;
using BBUnity.Platformer.Generation.Movement;
using BBUnity.Platformer.Generation.Templates;
using BBUnity.Generation.Rooms;
using BBUnity.Spawning;
using BBUnity.World.Elements;
using BBUnity.World;

namespace BBUnity.Editor.Platformer.Generation {

    /// <summary>
    /// Creates a starting set of platformer generation assets in the project — cell types, room
    /// types, element categories, the player spawn and level exit (prefabs and elements) and a
    /// ready-to-run template. They are ordinary assets: edit, rename or replace them freely. The
    /// user chooses the folder they go in. Running it again only creates what is missing.
    /// </summary>
    internal class PlatformerDefaultsInstaller {

        public const string SuggestedFolder = "Assets/BBUnity/Generation";

        private const string MenuPath = "Window/BBUnity/Generation/Install/Install Platformer Defaults";

        private readonly string _root;

        private PlatformerDefaultsInstaller(string root) {
            _root = root;
        }

        /// <summary>True once a platformer template exists in the project, i.e. the defaults have been installed.</summary>
        public static bool IsInstalled {
            get { return AssetDatabase.FindAssets("t:" + nameof(PlatformerLevelTemplateDefinition)).Length > 0; }
        }

        [MenuItem(MenuPath, false, 100)]
        private static void InstallFromMenu() {
            PlatformerLevelTemplateDefinition template = InstallInteractive();
            if (template == null) { return; }

            Selection.activeObject = template;
            EditorGUIUtility.PingObject(template);
        }

        // Unity menus can't hide an item, so once the defaults exist it is greyed out instead.
        [MenuItem(MenuPath, true)]
        private static bool ValidateInstallFromMenu() {
            return !IsInstalled;
        }

        /// <summary>Asks where to install (the suggested folder, or one the user picks) and installs there. Null if cancelled.</summary>
        public static PlatformerLevelTemplateDefinition InstallInteractive() {
            int choice = EditorUtility.DisplayDialogComplex(
                "Install Platformer Defaults",
                "Create the default cell types, room types, element categories, player spawn, level exit and a starter template.\n\n" +
                $"Suggested folder:\n{SuggestedFolder}",
                "Install Here",
                "Cancel",
                "Choose Folder..."
            );

            if (choice == 1) { return null; }

            string root = SuggestedFolder;
            if (choice == 2) {
                root = ChooseFolder();
                if (root == null) { return null; }
            }

            return Install(root);
        }

        public static PlatformerLevelTemplateDefinition Install(string root) {
            return new PlatformerDefaultsInstaller(root).Run();
        }

        // A folder inside the project's Assets folder, as an asset path; null if cancelled or outside it.
        private static string ChooseFolder() {
            string start = AssetDatabase.IsValidFolder(SuggestedFolder) ? SuggestedFolder : "Assets";
            string chosen = EditorUtility.OpenFolderPanel("Install Platformer Defaults Into", System.IO.Path.GetFullPath(start), string.Empty);
            if (string.IsNullOrEmpty(chosen)) { return null; }

            chosen = chosen.Replace('\\', '/');
            string assets = Application.dataPath;

            if (chosen == assets) { return "Assets"; }
            if (chosen.StartsWith(assets + "/", StringComparison.Ordinal)) { return "Assets" + chosen.Substring(assets.Length); }

            EditorUtility.DisplayDialog("Install Platformer Defaults", "Choose a folder inside this project's Assets folder.", "OK");
            return null;
        }

        private PlatformerLevelTemplateDefinition Run() {
            foreach (string folder in new[] { "Cells", "Rooms", "Categories", "Prefabs", "Elements", "Templates" }) {
                EnsureFolder(folder);
            }

            CellTypeDefinition solid = GetOrCreate<CellTypeDefinition>("Cells/Solid", so => {
                so.FindProperty("_displayName").stringValue = "Solid";
                so.FindProperty("_previewColor").colorValue = new Color(0.36f, 0.37f, 0.44f);
            });
            CellTypeDefinition platform = GetOrCreate<CellTypeDefinition>("Cells/One-Way Platform", so => {
                so.FindProperty("_displayName").stringValue = "One-Way Platform";
                so.FindProperty("_previewColor").colorValue = new Color(0.72f, 0.5f, 0.28f);
                so.FindProperty("_blocksMovement").boolValue = false;
            });

            RoomTypeDefinition start = CreateRoomType("Start", new Color(0.3f, 1f, 0.3f, 0.2f));
            RoomTypeDefinition exit = CreateRoomType("Exit", new Color(1f, 0.3f, 0.3f, 0.2f));
            RoomTypeDefinition combat = CreateRoomType("Combat", new Color(1f, 0.6f, 0.2f, 0.15f));
            CreateRoomType("Treasure", new Color(1f, 0.9f, 0.2f, 0.15f));
            CreateRoomType("Boss", new Color(0.8f, 0.2f, 0.8f, 0.2f));
            CreateRoomType("Secret", new Color(0.4f, 0.6f, 1f, 0.15f));

            LevelElementCategoryDefinition spawnCategory = CreateCategory("Player Spawn", new Color(0.3f, 1f, 0.3f));
            LevelElementCategoryDefinition exitCategory = CreateCategory("Exit", new Color(1f, 0.3f, 0.3f));
            CreateCategory("Enemy", new Color(1f, 0.6f, 0.2f));
            CreateCategory("Item", new Color(1f, 0.9f, 0.2f));
            CreateCategory("Trap", new Color(0.8f, 0.3f, 0.9f));
            CreateCategory("Checkpoint", new Color(0.3f, 0.9f, 1f));

            GameObject spawnPrefab = GetOrCreatePrefab("Player Spawn", go => go.AddComponent<PlayerSpawnNode>());
            GameObject exitPrefab = GetOrCreatePrefab("Level Exit", go => {
                var trigger = go.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = new Vector2(2f, 3f);
                trigger.offset = new Vector2(0f, 1.5f);
                go.AddComponent<LevelTransitionNode>();
            });

            LevelElementDefinition spawn = CreateElement("Player Spawn", spawnCategory, spawnPrefab, new Vector2Int(1, 2), start);
            LevelElementDefinition levelExit = CreateElement("Level Exit", exitCategory, exitPrefab, new Vector2Int(2, 3), exit);

            return GetOrCreateTemplate(solid, platform, start, exit, combat, spawn, levelExit);
        }

        private PlatformerLevelTemplateDefinition GetOrCreateTemplate(CellTypeDefinition solid, CellTypeDefinition platform, RoomTypeDefinition start, RoomTypeDefinition exit, RoomTypeDefinition combat, LevelElementDefinition spawn, LevelElementDefinition levelExit) {
            string path = $"{_root}/Templates/Platformer Level Template.asset";
            var existing = AssetDatabase.LoadAssetAtPath<PlatformerLevelTemplateDefinition>(path);
            if (existing != null) { return existing; }

            var template = ScriptableObject.CreateInstance<PlatformerLevelTemplateDefinition>();
            template.ApplyStandardRecipe(new PlatformerGenerationParameters(PlatformerMovementProfile.Default, solid, platform, start, exit, spawn, levelExit), combat);

            AssetDatabase.CreateAsset(template, path);
            AssetDatabase.SaveAssets();
            return template;
        }

        private RoomTypeDefinition CreateRoomType(string name, Color color) {
            return GetOrCreate<RoomTypeDefinition>("Rooms/" + name, so => {
                so.FindProperty("_displayName").stringValue = name;
                so.FindProperty("_previewColor").colorValue = color;
            });
        }

        private LevelElementCategoryDefinition CreateCategory(string name, Color color) {
            return GetOrCreate<LevelElementCategoryDefinition>("Categories/" + name, so => {
                so.FindProperty("_displayName").stringValue = name;
                so.FindProperty("_previewColor").colorValue = color;
            });
        }

        private LevelElementDefinition CreateElement(string name, LevelElementCategoryDefinition category, GameObject prefab, Vector2Int clearance, RoomTypeDefinition room) {
            return GetOrCreate<LevelElementDefinition>("Elements/" + name, so => {
                so.FindProperty("_displayName").stringValue = name;
                so.FindProperty("_category").objectReferenceValue = category;
                so.FindProperty("_prefab").objectReferenceValue = prefab;
                so.FindProperty("_clearance").vector2IntValue = clearance;

                SerializedProperty rooms = so.FindProperty("_allowedRoomTypes");
                rooms.arraySize = 1;
                rooms.GetArrayElementAtIndex(0).objectReferenceValue = room;
            });
        }

        private GameObject GetOrCreatePrefab(string name, Action<GameObject> configure) {
            string path = $"{_root}/Prefabs/{name}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) { return existing; }

            var temporary = new GameObject(name);
            try {
                configure(temporary);
                return PrefabUtility.SaveAsPrefabAsset(temporary, path);
            } finally {
                UnityEngine.Object.DestroyImmediate(temporary);
            }
        }

        private T GetOrCreate<T>(string relativePath, Action<SerializedObject> configure) where T : ScriptableObject {
            string path = $"{_root}/{relativePath}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) { return existing; }

            var created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, path);

            var serializedObject = new SerializedObject(created);
            configure(serializedObject);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(created);
            AssetDatabase.SaveAssets();
            return created;
        }

        private void EnsureFolder(string name) {
            string parent = "Assets";
            string relative = _root == "Assets" ? name : $"{_root.Substring("Assets/".Length)}/{name}";
            foreach (string segment in relative.Split('/')) {
                string path = parent + "/" + segment;
                if (!AssetDatabase.IsValidFolder(path)) { AssetDatabase.CreateFolder(parent, segment); }
                parent = path;
            }
        }
    }
}
