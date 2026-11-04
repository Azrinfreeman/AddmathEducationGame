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
    /// <summary>Captures screen layouts without clicking answers or saving scene changes.</summary>
    public static class ScreenReview
    {
        private static IEnumerator review;
        private static int originalLanguage;
        private static bool originalBackground;
        private static readonly List<string> rows = new List<string>();
        public static string Status { get; private set; } = "Idle";

        [MenuItem("Fun Math/UI/Review/Capture other screens (Play mode)")]
        public static void Start()
        {
            if (!EditorApplication.isPlaying || review != null)
                throw new InvalidOperationException("Enter Play mode first; only one screen review can run.");
            originalLanguage = PlayerPrefs.GetInt("EnglishLang", 0);
            originalBackground = Application.runInBackground;
            Application.runInBackground = true;
            Directory.CreateDirectory("Temp/screen-review");
            rows.Clear();
            rows.Add("scene\tlanguage\torientation\tstate\toverflow\tcapture");
            review = CaptureScreens();
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
            Time.timeScale = 1;
            File.WriteAllLines("Temp/screen-review/results.tsv", rows);
            Status = status + " (" + (rows.Count - 1) + " captures)";
            review = null;
            Debug.Log("Screen review: " + Status);
        }

        private static IEnumerator CaptureScreens()
        {
            // Loading redirects automatically; its destination is reviewed separately.
            var paths = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path)
                .Where(p => !p.Contains("/Loading/") && !(p.Contains("/Lessons/") && p.Split('/').Length >= 5)).ToArray();
            var resolution = typeof(AdventurePreview).GetMethod("SetResolution", BindingFlags.NonPublic | BindingFlags.Static);
            for (int index = 0; index < paths.Length; index++)
            {
                Time.timeScale = 1;
                var load = EditorSceneManager.LoadSceneAsyncInPlayMode(paths[index], new LoadSceneParameters(LoadSceneMode.Single));
                while (!load.isDone) yield return null;
                double ready = EditorApplication.timeSinceStartup + .6;
                while (EditorApplication.timeSinceStartup < ready) yield return null;
                // Tool focus changes would otherwise open Pause while a screenshot is being prepared.
                // This affects the temporary Play-mode copy only.
                foreach (var pause in UnityEngine.Object.FindObjectsByType<GamePause>(FindObjectsSortMode.None))
                {
                    pause.Resume();
                    UnityEngine.Object.DestroyImmediate(pause);
                }
                Time.timeScale = 0;
                var modals = UnityEngine.Object.FindObjectsByType<GameModalFocus>(FindObjectsSortMode.None);
                int states = paths[index].Contains("/Games/") && modals.Length > 0 ? 2 : 1;
                for (int state = 0; state < states; state++)
                {
                    // A board preview checks geometry only; it does not claim to play the game.
                    if (state == 1) foreach (var modal in modals) if (modal) modal.gameObject.SetActive(false);
                    for (int orientation = 0; orientation < 2; orientation++)
                    {
                        resolution.Invoke(null, new object[] { orientation == 0 ? 960 : 540, orientation == 0 ? 540 : 960,
                            orientation == 0 ? "Review landscape" : "Review portrait" });
                        for (int language = 0; language < 2; language++)
                        {
                            PlayerPrefs.SetInt("EnglishLang", language);
                            if (LanguageController.instance) LanguageController.instance.ToEnglish = language == 1;
                            Status = (index + 1) + "/" + paths.Length + " " + language + " " + orientation + " " + state;
                            int frame = Time.frameCount;
                            ready = EditorApplication.timeSinceStartup + .3;
                            while (Time.frameCount < frame + 5 || EditorApplication.timeSinceStartup < ready) yield return null;
                            Canvas.ForceUpdateCanvases();
                            var texts = UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)
                                .Where(t => t.enabled && t.color.a > .01f && t.GetComponentsInParent<CanvasGroup>().All(g => g.alpha > .01f)).ToArray();
                            foreach (var text in texts) text.ForceMeshUpdate();
                            string file = string.Format("Temp/screen-review/{0:D2}-{1}-{2}-{3}.png", index, language, orientation, state);
                            AdventurePreview.CaptureGameView(file);
                            string overflow = string.Join(" | ", texts.Where(t => t.isTextOverflowing).Select(t =>
                                t.name + ": " + t.text.Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ')));
                            rows.Add(paths[index] + "\t" + language + "\t" + orientation + "\t" +
                                (state == 0 ? "initial" : "board-preview") + "\t" + overflow + "\t" + file);
                        }
                    }
                }
                File.WriteAllLines("Temp/screen-review/results.tsv", rows);
            }
        }
    }
}
