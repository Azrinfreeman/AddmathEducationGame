using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FunMath.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace FunMath.Editor
{
    /// <summary>Focused presentation regression; never writes achievement or player keys.</summary>
    public static class PathRewardReview
    {
        private const string Output = "docs/tutorial/review/playground/path-rewards/";
        private static IEnumerator run;
        private static int language;
        private static bool background;
        private static readonly List<string> checks = new List<string>();
        public static string Status { get; private set; } = "Idle";

        [MenuItem("Fun Math/UI/Playground/5 - Verify paths and rewards (Play mode)")]
        public static void Start()
        {
            if (!EditorApplication.isPlaying || run != null) throw new InvalidOperationException("Enter Play mode first; run one review at a time.");
            language = PlayerPrefs.GetInt("EnglishLang", 0);
            background = Application.runInBackground; Application.runInBackground = true;
            Directory.CreateDirectory(Output); checks.Clear(); run = Review();
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayMode;
            Status = "Running";
        }
        private static void OnPlayMode(PlayModeStateChange state) { if (state == PlayModeStateChange.ExitingPlayMode) Finish("Stopped"); }
        private static void Tick()
        {
            try { if (!run.MoveNext()) Finish(checks.Any(c => c.StartsWith("FAIL")) ? "Failed" : "Completed"); }
            catch (Exception e) { checks.Add("FAIL\t" + e); Finish("Failed"); Debug.LogException(e); }
        }
        private static void Finish(string status)
        {
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlayMode;
            (run as IDisposable)?.Dispose(); run = null;
            PlayerPrefs.SetInt("EnglishLang", language);
            if (LanguageController.instance) LanguageController.instance.ToEnglish = language == 1;
            Application.runInBackground = background;
            File.WriteAllLines(Output + "checks.tsv", checks);
            Status = status + " (" + checks.Count + " checks)";
        }
        private static void Check(bool pass, string message) { checks.Add((pass ? "PASS\t" : "FAIL\t") + message); }
        private static IEnumerator Settle()
        {
            double until = EditorApplication.timeSinceStartup + .3;
            int frame = Time.frameCount;
            while (EditorApplication.timeSinceStartup < until || Time.frameCount < frame + 4) yield return null;
            Canvas.ForceUpdateCanvases();
        }
        private static IEnumerator Review()
        {
            string[] scenes = { "PrasekolahLesson", "Tahap1Lesson", "Tahap2Lesson", "BadgesPage", "CreditPage", "PenghargaanPage" };
            int[,] sizes = { {540,960}, {960,540}, {360,800}, {800,360}, {768,1024}, {1024,768} };
            var resolution = typeof(AdventurePreview).GetMethod("SetResolution", BindingFlags.NonPublic | BindingFlags.Static);
            foreach (string scene in scenes)
            {
                var load = SceneManager.LoadSceneAsync(scene); while (!load.isDone) yield return null;
                var wait = Settle(); while (wait.MoveNext()) yield return null;
                var active = SceneManager.GetActiveScene();
                var path = active.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<LessonPathLayout>(true)).FirstOrDefault();
                var shelf = active.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BadgeShelf>(true)).FirstOrDefault();
                for (int i = 0; i < sizes.GetLength(0); i++)
                {
                    resolution.Invoke(null, new object[] { sizes[i,0], sizes[i,1], "Playground " + sizes[i,0] + "x" + sizes[i,1] });
                    for (int lang = 0; lang < 2; lang++)
                    {
                        string view = scene + "-" + sizes[i,0] + "x" + sizes[i,1] + "-" + lang;
                        Status = view;
                        PlayerPrefs.SetInt("EnglishLang", lang);
                        if (LanguageController.instance) LanguageController.instance.ToEnglish = lang == 1;
                        wait = Settle(); while (wait.MoveNext()) yield return null;
                        var shell = active.GetRootGameObjects().First(r => r.name == (path ? "AdventureShell" : "SpecialtyShell"));
                        foreach (var text in shell.GetComponentsInChildren<TMP_Text>().Where(t => !t.GetComponentInParent<UnityEngine.UI.ScrollRect>()))
                        {
                            text.ForceMeshUpdate(); Check(!text.isTextOverflowing, view + " text fits: " + text.name);
                        }
                        if (path)
                        {
                            var bounds = ((RectTransform)path.transform).rect;
                            // Includes edges and permits sub-pixel float error after Canvas scaling.
                            bounds.xMin -= 1; bounds.yMin -= 1; bounds.xMax += 1; bounds.yMax += 1;
                            for (int n = 0; n < path.nodes.Length; n++)
                            {
                                var node = path.nodes[n]; var corners = new Vector3[4]; node.GetWorldCorners(corners);
                                Check(corners.All(c => bounds.Contains(path.transform.InverseTransformPoint(c))), view + " node in trail: " + n);
                                Check(node.GetComponent<UnityEngine.UI.Button>().interactable == node.GetComponent<NavigationState>().source.IsInteractable(), view + " source availability: " + n);
                                for (int other = 0; other < n; other++)
                                {
                                    var previous = path.nodes[other];
                                    var a = new Rect(node.anchoredPosition - new Vector2(0,node.sizeDelta.y), node.sizeDelta);
                                    var b = new Rect(previous.anchoredPosition - new Vector2(0,previous.sizeDelta.y), previous.sizeDelta);
                                    Check(!a.Overlaps(b), view + " separate nodes: " + n + "/" + other);
                                }
                            }
                        }
                        AdventurePreview.CaptureGameView(Output + view + ".png");
                        if (shelf)
                        {
                            var opener = shelf.entries[0].button;
                            EventSystem.current.SetSelectedGameObject(opener.gameObject);
                            opener.onClick.Invoke(); wait = Settle(); while (wait.MoveNext()) yield return null;
                            var owner = ModalInputOwner.ForScene(active);
                            Check(owner.HasModal && shelf.details.activeSelf, view + " details own modal input");
                            foreach (var text in shelf.details.GetComponentsInChildren<TMP_Text>())
                            { text.ForceMeshUpdate(); Check(!text.isTextOverflowing, view + " detail text fits: " + text.name); }
                            AdventurePreview.CaptureGameView(Output + view + "-details.png");
                            shelf.details.GetComponent<ModalInputScope>().Close(); wait = Settle(); while (wait.MoveNext()) yield return null;
                            Check(!owner.HasModal && EventSystem.current.currentSelectedGameObject == opener.gameObject, view + " Back restores focus");
                        }
                    }
                }
                if (shelf)
                {
                    var entry = shelf.entries[0]; var original = entry.progressKeys;
                    try
                    {
                        entry.progressKeys = Array.Empty<string>(); shelf.Refresh();
                        Check(entry.artwork.color == shelf.earnedTint && entry.status.text.Contains("Earned"), "Completed badge uses earned caption and tint");
                        entry.progressKeys = new[] { "PathRewardReviewMissing_" + Guid.NewGuid() }; shelf.Refresh();
                        Check(entry.artwork.color == shelf.exploringTint && entry.status.text.Contains("0 / 1"), "Unfinished badge shows exact progress and muted artwork");
                    }
                    finally { entry.progressKeys = original; shelf.Refresh(); }
                }
            }
            SceneManager.LoadScene("MainMenu");
        }
    }
}
