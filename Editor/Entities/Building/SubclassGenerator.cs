using System;
using System.IO;
using System.Text;

using UnityEditor;

namespace BBUnity.Editor.Entities.Building {

    /// <summary>
    /// Backs a SubclassSlot's "Create Subclass..." button: lets the user pick where the new
    /// class should live (Unity's own Save panel, scoped to Assets), then writes a minimal
    /// subclass — correct using statements and inheritance already in place — and imports it.
    ///
    /// The namespace is inferred from the chosen folder's path relative to Assets/ (dropping a
    /// leading "Scripts" segment, the near-universal Unity convention of not including it in the
    /// namespace) — hand-edit the generated file's namespace afterward if a project's own
    /// convention differs.
    /// </summary>
    public static class SubclassGenerator {

        public static void CreateSubclass(Type baseType, string suggestedClassName) {
            if (baseType == null) { throw new ArgumentNullException(nameof(baseType)); }

            string path = EditorUtility.SaveFilePanelInProject(
                "Create Subclass",
                suggestedClassName,
                "cs",
                $"Choose where to create the new subclass of {baseType.Name}.");

            if (string.IsNullOrEmpty(path)) { return; }

            string className = Path.GetFileNameWithoutExtension(path);
            if (!IsValidIdentifier(className)) {
                EditorUtility.DisplayDialog("Invalid Class Name", $"'{className}' is not a valid C# identifier.", "OK");
                return;
            }

            string targetNamespace = NamespaceFromFolder(Path.GetDirectoryName(path));

            File.WriteAllText(path, BuildFileContent(className, baseType, targetNamespace));

            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
        }

        private static bool IsValidIdentifier(string name) {
            if (string.IsNullOrEmpty(name) || !(char.IsLetter(name[0]) || name[0] == '_')) { return false; }

            foreach (char c in name) {
                if (!char.IsLetterOrDigit(c) && c != '_') { return false; }
            }

            return true;
        }

        private static string NamespaceFromFolder(string folder) {
            folder = folder.Replace('\\', '/');

            const string assetsPrefix = "Assets/";
            string relative = folder.StartsWith(assetsPrefix) ? folder.Substring(assetsPrefix.Length) : folder;

            string[] segments = relative.Split('/');
            int start = (segments.Length > 0 && segments[0] == "Scripts") ? 1 : 0;

            return string.Join(".", segments, start, segments.Length - start).Trim('.');
        }

        private static string BuildFileContent(string className, Type baseType, string targetNamespace) {
            var builder = new StringBuilder();
            bool hasNamespace = !string.IsNullOrEmpty(targetNamespace);
            string indent = hasNamespace ? "    " : "";

            builder.AppendLine("using UnityEngine;");
            if (!string.IsNullOrEmpty(baseType.Namespace) && baseType.Namespace != targetNamespace) {
                builder.AppendLine();
                builder.AppendLine($"using {baseType.Namespace};");
            }
            builder.AppendLine();

            if (hasNamespace) {
                builder.AppendLine($"namespace {targetNamespace} {{");
                builder.AppendLine();
            }

            builder.AppendLine($"{indent}public class {className} : {baseType.Name} {{");
            builder.AppendLine();
            builder.AppendLine($"{indent}}}");

            if (hasNamespace) {
                builder.AppendLine("}");
            }

            return builder.ToString();
        }
    }
}
