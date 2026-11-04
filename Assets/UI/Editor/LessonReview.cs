using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FunMath.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FunMath.Editor
{
    /// <summary>Captures every lesson page without saving changes to scenes or player progress.</summary>
    public static class LessonReview
    {
        private static IEnumerator review;
        private static int originalLanguage;
        private static bool originalBackground;
        private static readonly List<string> rows = new List<string>();
        private static readonly List<string> voiceRows = new List<string>();
        public static string Status { get; private set; } = "Idle";

        [MenuItem("Fun Math/UI/Review/Capture all lesson pages (Play mode)")]
        public static void Start()
        {
            if (!EditorApplication.isPlaying || review != null)
                throw new InvalidOperationException("Enter Play mode first; only one review can run.");
            originalLanguage = PlayerPrefs.GetInt("EnglishLang", 0);
            originalBackground = Application.runInBackground;
            Application.runInBackground = true;
            EditorApplication.isPaused = false;
            Directory.CreateDirectory("Temp/lesson-review");
            rows.Clear();
            rows.Add("scene\tpage\tlanguage\torientation\tvisibleText\toverflow\tcontentImagesWithoutSprites\tcapture\tvoiceHash\trecordingMatches");
            voiceRows.Clear();
            voiceRows.Add("scene\tscenePath\tlang\tpage\thash\ttext");
            review = CapturePages();
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode) Finish("Stopped");
        }

        private static void Tick()
        {
            try { if (!review.MoveNext()) Finish("Completed"); }
            catch (Exception exception) { Finish("Failed: " + exception.Message); Debug.LogException(exception); }
        }

        private static void Finish(string status)
        {
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            PlayerPrefs.SetInt("EnglishLang", originalLanguage);
            if (LanguageController.instance) LanguageController.instance.ToEnglish = originalLanguage == 1;
            Application.runInBackground = originalBackground;
            Directory.CreateDirectory("Temp/lesson-review");
            File.WriteAllLines("Temp/lesson-review/results.tsv", rows);
            File.WriteAllLines("Temp/lesson-review/voice-script.tsv", voiceRows);
            Status = status + " (" + (rows.Count - 1) + " captures)";
            review = null;
            Debug.Log("Lesson review: " + Status);
        }

        private static IEnumerator CapturePages()
        {
            var paths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Lessons" })
                .Select(AssetDatabase.GUIDToAssetPath).Where(p => p.Split('/').Length >= 5).OrderBy(p => p).ToArray();
            var setResolution = typeof(AdventurePreview).GetMethod("SetResolution", BindingFlags.NonPublic | BindingFlags.Static);
            for (int sceneIndex = 0; sceneIndex < paths.Length; sceneIndex++)
            {
                var load = EditorSceneManager.LoadSceneAsyncInPlayMode(paths[sceneIndex], new LoadSceneParameters(LoadSceneMode.Single));
                while (!load.isDone) yield return null;
                double ready = EditorApplication.timeSinceStartup + .3;
                while (EditorApplication.timeSinceStartup < ready) yield return null;
                var check = UnityEngine.Object.FindFirstObjectByType<CheckPages>();
                if (!check) throw new InvalidOperationException("No lesson pages: " + paths[sceneIndex]);
                var pages = check.GetComponentsInChildren<LessonPageLayout>(true).OrderBy(p => int.Parse(p.name)).ToArray();
                foreach (var page in pages) page.gameObject.SetActive(false);
                for (int orientation = 0; orientation < 2; orientation++)
                {
                    setResolution.Invoke(null, new object[] { orientation == 0 ? 960 : 540, orientation == 0 ? 540 : 960, orientation == 0 ? "Review landscape" : "Review portrait" });
                    for (int language = 0; language < 2; language++)
                    {
                        PlayerPrefs.SetInt("EnglishLang", language);
                        if (LanguageController.instance) LanguageController.instance.ToEnglish = language == 1;
                        foreach (var page in pages)
                        {
                            Status = sceneIndex + 1 + "/" + paths.Length + " " + page.name + " " + language + " " + orientation;
                            page.transform.SetAsFirstSibling();
                            page.gameObject.SetActive(true);
                            int frame = Time.frameCount;
                            ready = EditorApplication.timeSinceStartup + .15;
                            while (Time.frameCount < frame + 5 || EditorApplication.timeSinceStartup < ready) yield return null;
                            Canvas.ForceUpdateCanvases();
                            var texts = page.GetComponentsInChildren<TMP_Text>().Where(t => t.enabled && t.color.a > .01f).ToArray();
                            foreach (var text in texts) text.ForceMeshUpdate();
                            string file = string.Format("Temp/lesson-review/{0:D2}-{1:D2}-{2}-{3}.png", sceneIndex, int.Parse(page.name), language, orientation);
                            AdventurePreview.CaptureGameView(file);
                            string overflow = string.Join(" | ", texts.Where(t => t.isTextOverflowing).Select(t => t.name + ": " + t.text.Replace('\n', ' ').Replace('\t', ' ')));
                            int emptyImages = page.diagramContent.GetComponentsInChildren<UnityEngine.UI.Image>().Count(i => i.enabled && i.color.a > .01f && !i.sprite && !i.GetComponent<UnityEngine.UI.Button>());
                            var voice = UnityEngine.Object.FindFirstObjectByType<LessonVoice>();
                            string speech = voice ? voice.BuildSpeech() : "";
                            string hash = LessonVoice.Hash(speech);
                            bool matches = voice && voice.RecordingFor(speech);
                            rows.Add(paths[sceneIndex] + "\t" + page.name + "\t" + language + "\t" + orientation + "\t" + texts.Length + "\t" + overflow + "\t" + emptyImages + "\t" + file + "\t" + hash + "\t" + matches);
                            if (orientation == 0)
                                voiceRows.Add(Path.GetFileNameWithoutExtension(paths[sceneIndex]) + "\t" + paths[sceneIndex] + "\t" + (language == 1 ? "en" : "ms") + "\t" + page.name + "\t" + hash + "\t" + speech.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' '));
                            page.gameObject.SetActive(false);
                        }
                    }
                }
                File.WriteAllLines("Temp/lesson-review/results.tsv", rows);
            }
        }
    }
}

