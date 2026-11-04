using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FunMath.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FunMath.Editor
{
    /// <summary>Explicit bulk import and export. Routine narration edits can also be made in the page Inspector.</summary>
    public static class LessonNarrationEditor
    {
        private const string Input = "docs/tutorial/lesson-voice-overrides.tsv";
        private const string Output = "docs/tutorial/lesson-voice-script.tsv";

        [MenuItem("Fun Math/UI/16 - Import authored lesson narration")]
        public static void Import()
        {
            RequireEditMode();
            // Validate every row before opening or saving a scene.
            var rows = ReadRows(Input);
            var paths = LessonPaths();
            var scenePaths = paths.ToDictionary(Path.GetFileNameWithoutExtension);
            var keys = new HashSet<string>();
            foreach (var row in rows)
            {
                if (row.Length != 4 || !scenePaths.ContainsKey(row[0]) || !int.TryParse(row[1], out int page)
                    || page < 1 || (row[2] != "ms" && row[2] != "en") || string.IsNullOrWhiteSpace(row[3]))
                    throw new InvalidDataException("Invalid narration row: " + string.Join(" | ", row));
                if (!keys.Add(row[0] + "\t" + row[1] + "\t" + row[2]))
                    throw new InvalidDataException("Duplicate narration row: " + row[0] + " page " + row[1]);
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            int count = 0;
            try
            {
                // Preflight page names across all affected scenes before making changes.
                foreach (var group in rows.GroupBy(r => r[0]))
                {
                    var scene = EditorSceneManager.OpenScene(scenePaths[group.Key]);
                    var pages = Pages(scene);
                    foreach (var row in group)
                        if (!pages.Any(p => p.name == row[1]))
                            throw new InvalidDataException("Missing page " + row[1] + " in " + group.Key);
                }
                foreach (var group in rows.GroupBy(r => r[0]))
                {
                    var scene = EditorSceneManager.OpenScene(scenePaths[group.Key]);
                    var pages = Pages(scene).ToDictionary(p => p.name);
                    foreach (var row in group)
                    {
                        var page = pages[row[1]];
                        var narration = page.GetComponent<LessonNarration>();
                        if (!narration) narration = page.gameObject.AddComponent<LessonNarration>();
                        if (row[2] == "en") narration.english = row[3].Trim();
                        else narration.malay = row[3].Trim();
                        EditorUtility.SetDirty(narration);
                        count++;
                    }
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Debug.Log("Imported " + count + " authored page/language narrations. Export before generating clips.");
        }

        [MenuItem("Fun Math/UI/17 - Export authored lesson narration")]
        public static void Export()
        {
            RequireEditMode();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var rows = new List<string> { "scene\tscenePath\tlang\tpage\thash\ttext" };
            var imports = new List<string> { "scene\tpage\tlang\tspoken text" };
            try
            {
                foreach (var path in LessonPaths())
                {
                    var scene = EditorSceneManager.OpenScene(path);
                    foreach (var page in Pages(scene).OrderBy(p => int.Parse(p.name)))
                    {
                        var narration = page.GetComponent<LessonNarration>();
                        foreach (bool english in new[] { false, true })
                        {
                            string text = narration ? narration.TextFor(english) : "";
                            if (string.IsNullOrWhiteSpace(text))
                                throw new InvalidDataException("Missing authored narration: " + path + " page " + page.name);
                            text = text.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
                            string lang = english ? "en" : "ms";
                            string name = Path.GetFileNameWithoutExtension(path);
                            rows.Add(string.Join("\t", name, path, lang, page.name, LessonVoice.Hash(text), text));
                            imports.Add(string.Join("\t", name, page.name, lang, text));
                        }
                    }
                }
                // Only replace the export after every page has passed validation.
                File.WriteAllLines(Output, rows, new System.Text.UTF8Encoding(false));
                File.WriteAllLines(Input, imports, new System.Text.UTF8Encoding(false));
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Debug.Log("Exported " + (rows.Count - 1) + " narrations and refreshed the bulk-edit TSV.");
        }

        private static List<string[]> ReadRows(string path)
        {
            return File.ReadAllLines(path).Skip(1).Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#"))
                .Select(l => l.Split('\t')).ToList();
        }

        private static string[] LessonPaths()
        {
            return AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Lessons" })
                .Select(AssetDatabase.GUIDToAssetPath).Where(p => p.Split('/').Length >= 5).OrderBy(p => p).ToArray();
        }

        private static LessonPageLayout[] Pages(UnityEngine.SceneManagement.Scene scene)
        {
            return scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<LessonPageLayout>(true)).ToArray();
        }

        private static void RequireEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before importing or exporting narration.");
        }
    }
}
