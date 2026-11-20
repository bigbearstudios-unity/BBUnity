using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEditor;
using UnityEngine;

namespace BBUnity.Editor.Support.Validation {

    /// <summary>One prefab or asset that cannot load all of its data.</summary>
    public readonly struct MissingTypeIssue {

        public readonly string Path;
        public readonly string Detail;

        public MissingTypeIssue(string path, string detail) {
            Path = path;
            Detail = detail;
        }
    }

    /// <summary>The result of a <see cref="MissingTypeScanner"/> run.</summary>
    public sealed class MissingTypeReport {

        public int AssetsScanned;
        public readonly List<MissingTypeIssue> Issues = new List<MissingTypeIssue>();

        public override string ToString() {
            var text = new StringBuilder();
            text.AppendLine($"{Issues.Count} broken assets of {AssetsScanned} scanned.");
            foreach (MissingTypeIssue issue in Issues) {
                text.AppendLine($"  {issue.Path}: {issue.Detail}");
            }
            return text.ToString();
        }
    }

    /// <summary>
    /// Finds prefabs and assets that have a component with a missing script, or a [SerializeReference]
    /// entry whose type was renamed, moved or removed. Scenes are not opened, so they are not scanned.
    /// </summary>
    public static class MissingTypeScanner {

        public static MissingTypeReport Scan(params string[] folders) {
            var report = new MissingTypeReport();

            var existing = new List<string>();
            foreach (string folder in folders) {
                if (AssetDatabase.IsValidFolder(folder)) { existing.Add(folder); }
            }
            if (existing.Count == 0) { return report; }

            foreach (string guid in AssetDatabase.FindAssets(string.Empty, existing.ToArray())) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string extension = Path.GetExtension(path).ToLowerInvariant();

                if (extension == ".prefab") {
                    report.AssetsScanned++;
                    ScanPrefab(path, report);
                }
                else if (extension == ".asset") {
                    report.AssetsScanned++;
                    ScanAsset(path, report);
                }
            }

            return report;
        }

        private static void ScanPrefab(string path, MissingTypeReport report) {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (root == null) {
                report.Issues.Add(new MissingTypeIssue(path, "the prefab could not be loaded"));
                return;
            }

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) {
                GameObject go = child.gameObject;

                int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                if (missing > 0) {
                    report.Issues.Add(new MissingTypeIssue(path, $"'{go.name}' has {missing} missing script(s)"));
                }

                foreach (Component component in go.GetComponents<Component>()) {
                    if (component is MonoBehaviour && SerializationUtility.HasManagedReferencesWithMissingTypes(component)) {
                        report.Issues.Add(new MissingTypeIssue(path, $"'{go.name}' has a {component.GetType().Name} with a missing [SerializeReference] type"));
                    }
                }
            }
        }

        private static void ScanAsset(string path, MissingTypeReport report) {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path)) {
                if (asset == null) {
                    report.Issues.Add(new MissingTypeIssue(path, "an object in the asset has a missing script"));
                }
                else if (SerializationUtility.HasManagedReferencesWithMissingTypes(asset)) {
                    report.Issues.Add(new MissingTypeIssue(path, $"'{asset.name}' ({asset.GetType().Name}) has a missing [SerializeReference] type"));
                }
            }
        }
    }
}
