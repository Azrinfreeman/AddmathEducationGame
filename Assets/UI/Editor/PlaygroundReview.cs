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
using UnityEngine.UI;

namespace FunMath.Editor
{
    /// <summary>Repeatable MainMenu lifecycle and layout checks on the temporary Play-mode scene.</summary>
    public static class PlaygroundReview
    {
        public static string Status { get; private set; } = "Idle";
        private static IEnumerator run;
        private static int language, motion, voice;
        private static bool background;
        private static readonly List<string> checks = new List<string>();
        private const string Output = "docs/tutorial/review/playground/";

        [MenuItem("Fun Math/UI/Playground/4 - Verify MainMenu (Play mode)")]
        public static void Start()
        {
            if (!EditorApplication.isPlaying || run != null || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenu")
                throw new InvalidOperationException("Enter Play mode in MainMenu; run only one review at a time.");
            language = PlayerPrefs.GetInt("EnglishLang", 0); motion = PlayerPrefs.GetInt("UI.ReduceMotion", 0); voice = PlayerPrefs.GetInt("UI.Voice", 1);
            background = Application.runInBackground; Application.runInBackground = true;
            checks.Clear(); Directory.CreateDirectory(Output); Status = "Running";
            run = Verify(); EditorApplication.update += Tick; EditorApplication.playModeStateChanged += Changed;
        }
        private static void Changed(PlayModeStateChange state) { if (state == PlayModeStateChange.ExitingPlayMode) Finish("Stopped"); }
        private static void Tick()
        {
            try { if (!run.MoveNext()) Finish("Passed"); }
            catch (Exception error) { checks.Add("FAIL\t" + error.Message); Finish("Failed"); Debug.LogException(error); }
        }
        private static void Finish(string status)
        {
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= Changed;
            PlayerPrefs.SetInt("EnglishLang", language); AdventureSettings.SetReduceMotion(motion == 1); AdventureSettings.SetVoice(voice == 1);
            if (LanguageController.instance) LanguageController.instance.ToEnglish = language == 1;
            var backdrop = UnityEngine.Object.FindAnyObjectByType<AnimatedBackground>();
            if (backdrop) backdrop.SendMessage("OnApplicationFocus", Application.isFocused);
            Application.runInBackground = background; File.WriteAllLines(Output + "mainmenu-checks.tsv", checks);
            Status = status + " (" + checks.Count + " checks)"; run = null; Debug.Log("Playground review: " + Status);
        }
        private static void Check(bool condition, string detail)
        {
            if (!condition) throw new InvalidOperationException(detail);
            checks.Add("PASS\t" + detail);
        }
        private static IEnumerator Verify()
        {
            var backdrop = UnityEngine.Object.FindAnyObjectByType<AnimatedBackground>();
            // Automated Editor runs may have no focused Game view. Simulate foreground
            // explicitly; lifecycle checks below verify both focus states.
            backdrop.SendMessage("OnApplicationFocus", true);
            var owner = backdrop.modalOwner;
            var shell = backdrop.transform.parent;
            var more = shell.Find("SafeArea/More").GetComponent<Button>();
            var settings = shell.Find("PlaygroundModals/ParentSettings");
            var morePanel = shell.Find("PlaygroundModals/MorePanel");
            var grid = shell.Find("SafeArea/Activities");
            Check(grid.GetComponentsInChildren<NavigationState>().Length == 4, "Four primary activities");
            Check(shell.GetComponentsInChildren<NavigationState>(true).Length == 7, "All seven original routes retained");
            Check(shell.GetComponentsInChildren<OwlFloat>(true).All(o => !o.enabled), "Legacy owl float disabled");
            AdventureSettings.SetReduceMotion(false);
            EventSystem.current.SetSelectedGameObject(more.gameObject); more.onClick.Invoke(); yield return null;
            Check(owner.HasModal && backdrop.IsStill, "More blocks ambience");
            Check(owner.content.All(g => !g.interactable && !g.blocksRaycasts), "Main content input suspended");
            Check(EventSystem.current.currentSelectedGameObject.transform.IsChildOf(morePanel), "Selection confined to More");
            settings.gameObject.SetActive(true); yield return null;
            Check(EventSystem.current.currentSelectedGameObject.transform.IsChildOf(settings), "Nested settings owns selection");
            settings.gameObject.SetActive(false); yield return null;
            Check(owner.HasModal && backdrop.IsStill, "Closing one of two modals keeps ambience blocked");
            morePanel.GetComponent<ModalInputScope>().Close(); yield return null;
            Check(!owner.HasModal && !backdrop.IsStill, "Closing last modal releases ambience");
            Check(EventSystem.current.currentSelectedGameObject == more.gameObject, "More restores opener focus");
            AdventureSettings.SetReduceMotion(true);
            var stillPosition = backdrop.clouds[0].anchoredPosition;
            double until = EditorApplication.timeSinceStartup + .4;
            while (EditorApplication.timeSinceStartup < until) yield return null;
            Check(backdrop.IsStill && backdrop.clouds[0].anchoredPosition == stillPosition, "Reduced motion stops clouds immediately");
            var restScale = backdrop.discovery.localScale; backdrop.Discover(); yield return null;
            Check(backdrop.discovery.localScale == restScale && backdrop.discoveryCaption.activeSelf, "Discovery retains static feedback with reduced motion");
            more.onClick.Invoke(); morePanel.GetComponent<ModalInputScope>().Close(); yield return null;
            Check(backdrop.IsStill, "Closing modal cannot restart reduced motion");
            AdventureSettings.SetReduceMotion(false); AdventureSettings.SetVoice(true);
            PlayerPrefs.SetInt("EnglishLang", 1); backdrop.guide.Speak();
            if (backdrop.guide.englishRecording)
            {
                Check(backdrop.guide.IsPlaying && backdrop.IsStill, "Accepted recording blocks ambience immediately");
                AdventureSettings.SetVoice(false); yield return null;
                Check(!backdrop.guide.IsPlaying && !backdrop.IsStill, "Voice off stops playback and releases its blocker");
            }
            AdventureSettings.SetVoice(true);
            var resolution = typeof(AdventurePreview).GetMethod("SetResolution", BindingFlags.NonPublic | BindingFlags.Static);
            int[,] sizes = { {540,960}, {960,540}, {360,800}, {800,360}, {768,1024}, {1024,768} };
            for (int i = 0; i < sizes.GetLength(0); i++)
            {
                resolution.Invoke(null, new object[] { sizes[i,0], sizes[i,1], "Playground " + sizes[i,0] + "x" + sizes[i,1] });
                for (int lang = 0; lang < 2; lang++)
                {
                    PlayerPrefs.SetInt("EnglishLang", lang); if (LanguageController.instance) LanguageController.instance.ToEnglish = lang == 1;
                    backdrop.guide.RefreshCaption();
                    Status = "Capturing " + sizes[i,0] + "x" + sizes[i,1] + " language " + lang;
                    until = EditorApplication.timeSinceStartup + .45;
                    int frame = Time.frameCount;
                    while (EditorApplication.timeSinceStartup < until || Time.frameCount < frame + 4) yield return null;
                    Canvas.ForceUpdateCanvases();
                    foreach (var text in grid.GetComponentsInChildren<TMP_Text>())
                    {
                        text.ForceMeshUpdate(); Check(!text.isTextOverflowing, "Activity text fits " + sizes[i,0] + "x" + sizes[i,1] + "/" + lang + ": " + text.text);
                    }
                    AdventurePreview.CaptureGameView(Output + "mainmenu-" + sizes[i,0] + "x" + sizes[i,1] + "-" + (lang == 0 ? "bm" : "en") + ".png");
                }
            }
            Status = "Observing 30-second ambient cycle";
            backdrop.SendMessage("OnApplicationFocus", true);
            float minimum = backdrop.clouds[0].anchoredPosition.x, maximum = minimum;
            until = EditorApplication.timeSinceStartup + 30;
            while (EditorApplication.timeSinceStartup < until)
            {
                // CaptureGameView can return focus to another Editor window asynchronously.
                backdrop.SendMessage("OnApplicationFocus", true);
                float position = backdrop.clouds[0].anchoredPosition.x;
                minimum = Mathf.Min(minimum, position); maximum = Mathf.Max(maximum, position);
                yield return null;
            }
            Check(!backdrop.IsStill && maximum - minimum > 1,
                "Cloud drift observed over 30 seconds in simulated foreground (range " + (maximum - minimum) + ")");
            backdrop.SendMessage("OnApplicationFocus", false);
            Check(backdrop.IsStill, "Losing app focus stops ambience");
            backdrop.SendMessage("OnApplicationFocus", true);
            Check(!backdrop.IsStill, "Returning app focus releases its blocker");
            Check(UnityEngine.Object.FindObjectsByType<AnimatedBackground>().Length == 1, "One backdrop after modal and layout changes");
        }
    }
}
