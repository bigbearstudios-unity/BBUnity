using System.IO;
using System.Text.RegularExpressions;

using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace BBUnity.Editor.Support.Validation.Tests {

    /// <summary>
    /// The 1.2 safety net: the package must have no missing scripts or [SerializeReference] types, and the
    /// scanner that checks it must really find both kinds of break (otherwise a green run means nothing).
    /// </summary>
    public class MissingTypeScannerTests {

        private const string TempFolder = "Assets/BBUnityScannerTests_Temp";

        [SetUp]
        public void SetUp() {
            // Unity logs about the deliberately broken fixtures when it imports them; only the scan's result matters here.
            LogAssert.ignoreFailingMessages = true;
            AssetDatabase.DeleteAsset(TempFolder);
            AssetDatabase.CreateFolder("Assets", "BBUnityScannerTests_Temp");
        }

        [TearDown]
        public void TearDown() {
            LogAssert.ignoreFailingMessages = false;
            AssetDatabase.DeleteAsset(TempFolder);
        }

        [Test]
        public void Package_HasNoMissingScriptsOrManagedReferenceTypes() {
            MissingTypeReport report = MissingTypeScanner.Scan("Packages/BBUnity");

            Assert.IsEmpty(report.Issues, report.ToString());
        }

        [Test]
        public void Scan_UnmodifiedFixtureAssets_ReportsNothing() {
            CreateFixtureAsset(TempFolder + "/Clean.asset");
            CreateFixturePrefab(TempFolder + "/Clean.prefab");

            MissingTypeReport report = MissingTypeScanner.Scan(TempFolder);

            Assert.AreEqual(2, report.AssetsScanned);
            Assert.IsEmpty(report.Issues, report.ToString());
        }

        [Test]
        public void Scan_ManagedReferenceWithRemovedType_IsReported() {
            string path = TempFolder + "/Broken.asset";
            CreateFixtureAsset(path);

            Rewrite(path, text => Regex.Replace(text, @"class: ScannerFixtureImpl,", "class: ScannerFixtureImplRemoved,"));

            MissingTypeReport report = MissingTypeScanner.Scan(TempFolder);

            Assert.AreEqual(1, report.Issues.Count, report.ToString());
            Assert.AreEqual(path, report.Issues[0].Path);
        }

        [Test]
        public void Scan_PrefabWithMissingScript_IsReported() {
            string path = TempFolder + "/Broken.prefab";
            string scriptGuid = CreateFixturePrefab(path);

            Rewrite(path, text => text.Replace("guid: " + scriptGuid, "guid: 0123456789abcdef0123456789abcdef"));

            MissingTypeReport report = MissingTypeScanner.Scan(TempFolder);

            Assert.AreEqual(1, report.Issues.Count, report.ToString());
            Assert.AreEqual(path, report.Issues[0].Path);
        }

        [Test]
        public void Scan_FolderThatDoesNotExist_ReportsNothing() {
            MissingTypeReport report = MissingTypeScanner.Scan("Assets/DoesNotExist");

            Assert.AreEqual(0, report.AssetsScanned);
            Assert.IsEmpty(report.Issues);
        }

        private static void CreateFixtureAsset(string path) {
            var asset = ScriptableObject.CreateInstance<ScannerFixtureAsset>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }

        /// <summary>Saves a prefab with the fixture component and returns that component's script GUID.</summary>
        private static string CreateFixturePrefab(string path) {
            var go = new GameObject("Fixture");
            try {
                var behaviour = go.AddComponent<ScannerFixtureBehaviour>();
                string scriptGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(behaviour)));
                PrefabUtility.SaveAsPrefabAsset(go, path);
                return scriptGuid;
            }
            finally {
                Object.DestroyImmediate(go);
            }
        }

        private static void Rewrite(string assetPath, System.Func<string, string> edit) {
            string fullPath = Path.GetFullPath(assetPath);
            string original = File.ReadAllText(fullPath);
            string edited = edit(original);

            Assert.AreNotEqual(original, edited, "The test's edit did not change the asset text, so it proves nothing.");

            File.WriteAllText(fullPath, edited);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
