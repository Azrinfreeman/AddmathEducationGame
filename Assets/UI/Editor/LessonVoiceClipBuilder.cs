using System.Collections.Generic;
using System.IO;
using System.Linq;
using FunMath.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FunMath.Editor
{
    /// <summary>
    /// Links the generated lesson recordings (Assets/Audio/LessonVoice/&lt;ms|en&gt;/&lt;hash&gt;.mp3) to each lesson scene's
    /// LessonVoice component, using docs/tutorial/lesson-voice-script.tsv. Also applies lean import settings to the clips.
    /// Re-runnable: it replaces the clip list each time.
    /// </summary>
    public static class LessonVoiceClipBuilder
    {
        private const string Script = "docs/tutorial/lesson-voice-script.tsv";
        private const string Folder = "Assets/Audio/LessonVoice";

        [MenuItem("Fun Math/UI/15 - Assign lesson voice clips")]
        public static void Assign()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play mode first.");
            if (!File.Exists(Script)) { Debug.LogError("Missing " + Script + ". Run the page-text extraction first."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            ConfigureImports();

            // scenePath -> page -> (ms hash, en hash)
            var table = new Dictionary<string, Dictionary<string, string[]>>();
            foreach (var line in File.ReadAllLines(Script).Skip(1))
            {
                var c = line.Split('\t');
                if (c.Length < 6 || c[0].StartsWith("#")) continue;
                string scenePath = c[1], lang = c[2], page = c[3], hash = c[4];
                if (!table.TryGetValue(scenePath, out var pages)) table[scenePath] = pages = new Dictionary<string, string[]>();
                if (!pages.TryGetValue(page, out var hashes)) pages[page] = hashes = new string[2];
                hashes[lang == "en" ? 1 : 0] = hash;
            }

            var setup = EditorSceneManager.GetSceneManagerSetup();
            int scenes = 0, linked = 0, missing = 0;
            try
            {
                foreach (var kv in table)
                {
                    var scene = EditorSceneManager.OpenScene(kv.Key);
                    var voice = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<LessonVoice>(true)).FirstOrDefault();
                    if (voice == null) { Debug.LogWarning("No LessonVoice in " + kv.Key); continue; }
                    var entries = new List<LessonVoice.PageClips>();
                    foreach (var page in kv.Value)
                    {
                        var entry = new LessonVoice.PageClips { page = page.Key };
                        entry.malayHash = page.Value[0]; entry.englishHash = page.Value[1];
                        entry.malay = Load("ms", page.Value[0]); entry.english = Load("en", page.Value[1]);
                        if (entry.malay) linked++; else missing++;
                        if (entry.english) linked++; else missing++;
                        entries.Add(entry);
                    }
                    voice.clips = entries.ToArray();
                    EditorUtility.SetDirty(voice);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    scenes++;
                }
            }
            finally { if (setup != null && setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Debug.Log("Lesson voice clips linked in " + scenes + " scene(s): " + linked + " clips linked, " + missing + " missing.");
        }

        private static AudioClip Load(string lang, string hash)
        {
            if (string.IsNullOrEmpty(hash)) return null;
            return AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + "/" + lang + "/" + hash + ".mp3");
        }

        private static void ConfigureImports()
        {
            int changed = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { Folder }))
            {
                var importer = (AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                var settings = importer.defaultSampleSettings;
                bool dirty = !importer.forceToMono || settings.preloadAudioData
                    || settings.loadType != AudioClipLoadType.CompressedInMemory || settings.compressionFormat != AudioCompressionFormat.Vorbis;
                if (!dirty) continue;
                importer.forceToMono = true;           // voice does not need stereo
                settings.preloadAudioData = false;     // load on first Listen, not when the scene opens
                importer.loadInBackground = true;
                settings.loadType = AudioClipLoadType.CompressedInMemory;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = .5f;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
                changed++;
            }
            Debug.Log("Lesson voice clip import settings updated on " + changed + " clip(s).");
        }
    }
}
