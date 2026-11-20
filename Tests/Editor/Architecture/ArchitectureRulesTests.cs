using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace BBUnity.Editor.Architecture.Tests {
    /// <summary>
    /// The architecture rules checker. It holds the assembly graph from Architecture Decisions.md
    /// section 4 and fails when an asmdef or a source file breaks a rule. Fix a failure by moving
    /// code or adding an abstraction, never by adding an upward reference. Test code is not
    /// scanned for the source rules, because tests have to reset Time.timeScale.
    /// </summary>
    public class ArchitectureRulesTests {

        private const string Foundation = "Unity.BBUnity.Foundation";
        private const string Core = "Unity.BBUnity";
        private const string Rpg = "Unity.BBUnity.Rpg";
        private const string Prefix = "Unity.BBUnity.";

        // Assembly name -> layer. Mirrors the table in Architecture Decisions.md section 4:
        // an assembly may reference only assemblies with a smaller layer number.
        private static readonly Dictionary<string, int> Layers = new Dictionary<string, int> {
            { Foundation, 0 },
            { "Unity.BBUnity.Generation", 0 },
            { Core, 1 },
            { "Unity.BBUnity.2D", 2 },
            { "Unity.BBUnity.3D", 2 },
            { Rpg, 2 },
            { "Unity.BBUnity.Vendor.Cinemachine", 2 },
            { "Unity.BBUnity.Vendor.Addressables", 2 },
            { "Unity.BBUnity.Vendor.Urp", 2 },
            { "Unity.BBUnity.Vendor.Steam", 2 },
            { "Unity.BBUnity.TurnBased", 3 },
            { "Unity.BBUnity.Shooter", 3 },
            { "Unity.BBUnity.Platformer", 3 },
            { "Unity.BBUnity.Crafting", 4 },
            { "VampireEmpire", 5 },
        };

        // Foundation may not reference these Unity packages either (rule 2).
        private static readonly string[] FoundationForbidden = { "Unity.InputSystem", "Unity.TextMeshPro" };

        // Source rule allowlist: file (path relative to the project) -> why, and the item that removes it.
        private class Allowed {
            public string Reason;
            public string RemovedBy;
        }

        private static readonly Dictionary<string, Allowed> RandomAllowlist = new Dictionary<string, Allowed> {
            { "Packages/BBUnity/Runtime/Audio/Sound.cs", new Allowed {
                Reason = "cosmetic variation, never part of the simulation", RemovedBy = "never (allowed by Todos 1.6)" } },
            { "Packages/BBUnity/Runtime/Combat/HitEffects/ScreenShakeEffect.cs", new Allowed {
                Reason = "cosmetic shake direction", RemovedBy = "Todos 9.5 (moves to the cosmetic stream)" } },
            { "Packages/BBUnity/Runtime/Magnitudes/StaticRangeDamageValue.cs", new Allowed {
                Reason = "damage roll uses UnityEngine.Random", RemovedBy = "Todos 4.4 (uses Game.Session.Random)" } },
        };

        // The one file allowed to write Time.timeScale.
        private const string TimeScaleOwner = "Packages/BBUnity/Runtime/Controllers/TimeController.cs";

        private static readonly Regex RandomUse = new Regex(
            @"\bUnityEngine\.Random\b|(?<![\w.])Random\.(Range|value|insideUnit\w*|onUnit\w*|rotation\w*|InitState|state|ColorHSV)\b");
        private static readonly Regex TimeScaleWrite = new Regex(@"\bTime\.timeScale\s*(=|\+=|-=|\*=|/=)(?!=)");
        private static readonly Regex DontDestroy = new Regex(@"\bDontDestroyOnLoad\b");
        private static readonly Regex CameraMain = new Regex(@"\bCamera\.main\b");

        [Serializable]
        private class AsmdefData {
            public string name;
            public string[] references;
            public string[] includePlatforms;
        }

        private class Asmdef {
            public string Name;
            public string Path;
            public string[] References;
            public bool IsEditorOrTest;
        }

        private static string ProjectRoot {
            get { return Directory.GetParent(Application.dataPath).FullName; }
        }

        private static string Relative(string fullPath) {
            return fullPath.Substring(ProjectRoot.Length + 1).Replace('\\', '/');
        }

        // Runtime and editor code of the package, and the game's scripts. Tests are not included.
        private static IEnumerable<string> SourceRoots() {
            yield return Path.Combine(ProjectRoot, "Packages/BBUnity/Runtime");
            yield return Path.Combine(ProjectRoot, "Packages/BBUnity/Editor");
            yield return Path.Combine(ProjectRoot, "Assets/Scripts");
            yield return Path.Combine(ProjectRoot, "Assets/Editor");
        }

        private static IEnumerable<string> AsmdefRoots() {
            yield return Path.Combine(ProjectRoot, "Packages/BBUnity");
            yield return Path.Combine(ProjectRoot, "Assets");
        }

        private static List<Asmdef> ReadAsmdefs() {
            var result = new List<Asmdef>();
            foreach (string root in AsmdefRoots()) {
                if (!Directory.Exists(root)) continue;
                foreach (string path in Directory.GetFiles(root, "*.asmdef", SearchOption.AllDirectories)) {
                    if (path.Contains("/_Old_/")) continue;
                    var data = JsonUtility.FromJson<AsmdefData>(File.ReadAllText(path));
                    bool editorOnly = data.includePlatforms != null
                        && data.includePlatforms.Length == 1 && data.includePlatforms[0] == "Editor";
                    result.Add(new Asmdef {
                        Name = data.name,
                        Path = Relative(path),
                        References = data.references ?? new string[0],
                        IsEditorOrTest = editorOnly || data.name.EndsWith(".Editor") || data.name.Contains(".Tests"),
                    });
                }
            }
            return result;
        }

        private static List<Asmdef> RuntimeAsmdefs() {
            return ReadAsmdefs().Where(a => !a.IsEditorOrTest).ToList();
        }

        private static bool IsBBUnity(string name) {
            return name == Core || name.StartsWith(Prefix);
        }

        private static bool IsVendor(string name) {
            return name.StartsWith("Unity.BBUnity.Vendor.");
        }

        private static string StripComments(string source) {
            source = Regex.Replace(source, @"/\*.*?\*/", m => new string('\n', m.Value.Count(c => c == '\n')), RegexOptions.Singleline);
            return Regex.Replace(source, @"//[^\n]*", "");
        }

        private static List<string> Scan(Regex pattern, Func<string, bool> skip) {
            var hits = new List<string>();
            foreach (string root in SourceRoots()) {
                if (!Directory.Exists(root)) continue;
                foreach (string path in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)) {
                    string relative = Relative(path);
                    if (relative.Contains("/_Old_/") || skip(relative)) continue;
                    if (pattern.IsMatch(StripComments(File.ReadAllText(path)))) hits.Add(relative);
                }
            }
            return hits;
        }

        private static string Join(IEnumerable<string> lines) {
            return "\n  " + string.Join("\n  ", lines);
        }

        // ----- Assembly graph -----

        [Test]
        public void EveryRuntimeAssembly_IsInTheAssemblyGraph() {
            var unknown = RuntimeAsmdefs().Where(a => !Layers.ContainsKey(a.Name)).Select(a => a.Name + " (" + a.Path + ")");
            Assert.IsEmpty(unknown, "Add these to the table in Architecture Decisions.md section 4 and to Layers here:" + Join(unknown));
        }

        [Test]
        public void References_PointToALowerLayerOnly() {
            var errors = new List<string>();
            foreach (Asmdef asm in RuntimeAsmdefs()) {
                int layer;
                if (!Layers.TryGetValue(asm.Name, out layer)) continue;
                foreach (string reference in asm.References) {
                    int referenced;
                    if (!Layers.TryGetValue(reference, out referenced)) continue;
                    if (referenced >= layer) {
                        errors.Add(asm.Name + " (layer " + layer + ") references " + reference + " (layer " + referenced + ")");
                    }
                }
            }
            Assert.IsEmpty(errors, "Upward or sideways reference:" + Join(errors));
        }

        [Test]
        public void Foundation_ReferencesNothing() {
            var errors = new List<string>();
            foreach (Asmdef asm in RuntimeAsmdefs().Where(a => a.Name == Foundation)) {
                errors.AddRange(asm.References.Where(r => IsBBUnity(r) || FoundationForbidden.Contains(r)));
            }
            Assert.IsEmpty(errors, "Foundation references no other BBUnity assembly, InputSystem or TextMeshPro:" + Join(errors));
        }

        [Test]
        public void Core_ReferencesOnlyFoundationAndGeneration() {
            var errors = new List<string>();
            foreach (Asmdef asm in RuntimeAsmdefs().Where(a => a.Name == Core)) {
                errors.AddRange(asm.References.Where(r => IsBBUnity(r) && r != Foundation && r != "Unity.BBUnity.Generation"));
            }
            Assert.IsEmpty(errors, "Core references no dimension, Rpg, game type or vendor assembly:" + Join(errors));
        }

        [Test]
        public void GameTypesAndPacks_DoNotReferenceRpg() {
            var errors = new List<string>();
            foreach (Asmdef asm in RuntimeAsmdefs().Where(a => Layers.ContainsKey(a.Name) && Layers[a.Name] >= 3 && a.Name != "VampireEmpire")) {
                if (asm.References.Contains(Rpg)) errors.Add(asm.Name);
            }
            Assert.IsEmpty(errors, "Games reference Rpg; game types do not:" + Join(errors));
        }

        [Test]
        public void OnlyTheGame_ReferencesVendorAssemblies() {
            var errors = new List<string>();
            foreach (Asmdef asm in RuntimeAsmdefs().Where(a => a.Name != "VampireEmpire")) {
                errors.AddRange(asm.References.Where(IsVendor).Select(r => asm.Name + " -> " + r));
            }
            Assert.IsEmpty(errors, "Nothing but the game and the tests references a vendor assembly:" + Join(errors));
        }

        [Test]
        public void VendorAssemblies_AreSwitchedOnByAVersionDefine() {
            var errors = new List<string>();
            foreach (Asmdef asm in RuntimeAsmdefs().Where(a => IsVendor(a.Name))) {
                string json = File.ReadAllText(Path.Combine(ProjectRoot, asm.Path));
                if (!Regex.IsMatch(json, @"""defineConstraints""\s*:\s*\[\s*""[^""]+""")) {
                    errors.Add(asm.Name);
                }
            }
            Assert.IsEmpty(errors, "Vendor assemblies need a defineConstraint:" + Join(errors));
        }

        // ----- Source rules -----

        [Test]
        public void UnityRandom_IsOnlyUsedWhereAllowed() {
            List<string> hits = Scan(RandomUse, path => RandomAllowlist.ContainsKey(path));
            Assert.IsEmpty(hits, "UnityEngine.Random is not allowed in gameplay code. Use RandomService (Todos 2.x):" + Join(hits));
        }

        [Test]
        public void RandomAllowlist_HasNoStaleEntries() {
            List<string> hits = Scan(RandomUse, path => !RandomAllowlist.ContainsKey(path));
            var stale = RandomAllowlist.Where(e => !hits.Contains(e.Key))
                .Select(e => e.Key + " (removed by " + e.Value.RemovedBy + ")");
            Assert.IsEmpty(stale, "These files no longer use UnityEngine.Random; delete their allowlist entries:" + Join(stale));
        }

        [Test]
        public void RandomAllowlist_NamesTheItemThatRemovesEachEntry() {
            var errors = RandomAllowlist.Where(e => string.IsNullOrEmpty(e.Value.RemovedBy) || string.IsNullOrEmpty(e.Value.Reason))
                .Select(e => e.Key);
            Assert.IsEmpty(errors, "Each allowlist entry needs a reason and the item that removes it:" + Join(errors));
        }

        [Test]
        public void TimeScale_IsOnlyWrittenByTimeController() {
            List<string> hits = Scan(TimeScaleWrite, path => path == TimeScaleOwner);
            Assert.IsEmpty(hits, "Time.timeScale is written only by TimeController:" + Join(hits));
        }

        [Test]
        public void DontDestroyOnLoad_IsNotUsed() {
            List<string> hits = Scan(DontDestroy, path => false);
            Assert.IsEmpty(hits, "DontDestroyOnLoad is not allowed. Use Lifetime and the session shell:" + Join(hits));
        }

        [Test]
        public void CameraMain_IsNotUsed() {
            List<string> hits = Scan(CameraMain, path => false);
            Assert.IsEmpty(hits, "Camera.main is not allowed. Use the camera rig:" + Join(hits));
        }

        // ----- The checker checks itself -----

        [Test]
        public void SourceRules_MatchTheirTargetsAndSkipComments() {
            Assert.IsTrue(RandomUse.IsMatch("var x = Random.Range(0, 3);"));
            Assert.IsTrue(RandomUse.IsMatch("UnityEngine.Random.value"));
            Assert.IsFalse(RandomUse.IsMatch("var r = new System.Random(4); r.Next();"));
            Assert.IsTrue(TimeScaleWrite.IsMatch("Time.timeScale = 0f;"));
            Assert.IsTrue(TimeScaleWrite.IsMatch("Time.timeScale *= 2;"));
            Assert.IsFalse(TimeScaleWrite.IsMatch("if (Time.timeScale == 0f) {}"));
            Assert.IsFalse(TimeScaleWrite.IsMatch("float s = Time.timeScale;"));
            Assert.IsFalse(RandomUse.IsMatch(StripComments("// Random.Range(0, 1)\n/* Random.value */")));
        }
    }
}
