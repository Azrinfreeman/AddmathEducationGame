using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FunMath.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FunMath.Editor
{
    /// <summary>Exercises repaired quiz flow without completing achievements or changing player identity.</summary>
    public static class WiringReview
    {
        private const string Output = "docs/tutorial/review/wiring/";
        private static IEnumerator run;
        private static bool background;
        private static readonly List<string> checks = new List<string>();
        private static readonly Dictionary<string, int?> preferences = new Dictionary<string, int?>();
        public static string Status { get; private set; } = "Idle";

        [MenuItem("Fun Math/UI/Wiring/3 - Verify quiz actions (Play mode)")]
        public static void Start()
        {
            if (!EditorApplication.isPlaying || run != null) throw new InvalidOperationException("Enter Play mode and finish the current review first.");
            preferences.Clear();
            var keys = new List<string> { "QuizScoreEasy", "QuizScoreMedium", "QuizScoreHard", "EnglishLang" };
            string player = PlayerPrefs.GetInt("PlayerID").ToString();
            keys.AddRange(new[] { "Kuiz0_", "Kuiz1_", "Kuiz2_", "High1_", "High2_", "High3_", "Menu1_", "Menu2_", "Menu3_", "Menu4_", "Menu5_", "Menu6_" }.Select(k => k + player));
            foreach (string key in keys) preferences[key] = PlayerPrefs.HasKey(key) ? (int?)PlayerPrefs.GetInt(key) : null;
            background = Application.runInBackground; Application.runInBackground = true;
            checks.Clear(); Directory.CreateDirectory(Output); Status = "Running";
            run = Review(); EditorApplication.update += Tick; EditorApplication.playModeStateChanged += OnPlayMode;
        }
        private static void OnPlayMode(PlayModeStateChange state) { if (state == PlayModeStateChange.ExitingPlayMode) Finish("Stopped"); }
        private static void Tick()
        {
            try { if (!run.MoveNext()) Finish(checks.Any(c => c.StartsWith("FAIL")) ? "Failed" : "Completed"); }
            catch (Exception e) { checks.Add("FAIL\t" + e.Message); Finish("Failed"); Debug.LogException(e); }
        }
        private static void Finish(string status)
        {
            EditorApplication.update -= Tick; EditorApplication.playModeStateChanged -= OnPlayMode;
            (run as IDisposable)?.Dispose(); run = null;
            foreach (var pair in preferences)
                if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
            if (LanguageController.instance) LanguageController.instance.ToEnglish = PlayerPrefs.GetInt("EnglishLang", 0) == 1;
            Application.runInBackground = background;
            File.WriteAllLines(Output + "play-checks.tsv", checks);
            Status = status + " (" + checks.Count + " checks)";
        }
        private static void Check(bool pass, string message) { checks.Add((pass ? "PASS\t" : "FAIL\t") + message); }
        private static IEnumerator Wait()
        {
            double until = EditorApplication.timeSinceStartup + .4; int frame = Time.frameCount;
            while (EditorApplication.timeSinceStartup < until || Time.frameCount < frame + 4) yield return null;
        }
        private static IEnumerator Review()
        {
            foreach (string scene in new[] { "PrasekolahQuiz", "Tahap1Quiz", "Tahap2Quiz" })
            {
                Status = scene;
                var load = SceneManager.LoadSceneAsync(scene); while (!load.isDone) yield return null;
                var wait = Wait(); while (wait.MoveNext()) yield return null;
                var active = SceneManager.GetActiveScene();
                var canvas = active.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<QuizScreenLayout>(true)).First().transform;
                var timer = canvas.GetComponentInChildren<TimerController>(true); timer.isStopping = true;
                var confirmation = canvas.Find("Prompted (1)");
                var result = canvas.Find("Prompted (2)");
                var timeout = canvas.Find("TimeUp");
                Check(timer.prompted == result && timer.timeoutGameobject == timeout, scene + " timer references survive Start");
                foreach (string name in new[] { "IntroPanelParent (1)", "Prompted", "Prompted (1)", "Prompted (2)", "TimeUp" })
                { var panel = canvas.Find(name); if (panel) panel.gameObject.SetActive(false); }
                var quiz = canvas.Find("QuizController");
                var last = quiz.Cast<Transform>().OrderByDescending(p => int.Parse(p.name)).First();
                foreach (Transform page in quiz) page.gameObject.SetActive(page == last);
                // Normal quiz navigation moves the visible question to sibling zero.
                last.SetAsFirstSibling();
                var next = last.Find("nextButton").GetComponent<Button>();
                wait = Wait(); while (wait.MoveNext()) yield return null;
                Check(next.IsActive() && next.IsInteractable(), scene + " Check is visible and interactive on the final page");
                var progress = quiz.GetComponent<CheckQuizNumber>();
                Check(progress.quizNumber == last.name && progress.numberofQuizzes == quiz.childCount, scene + " final-page progress matches normal navigation");
                var resolution = typeof(AdventurePreview).GetMethod("SetResolution", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                int[,] sizes = { {540,960}, {960,540}, {360,800}, {800,360}, {768,1024}, {1024,768} };
                for (int size = 0; size < sizes.GetLength(0); size++)
                    for (int language = 0; language < 2; language++)
                    {
                        resolution.Invoke(null, new object[] { sizes[size,0], sizes[size,1], "Playground " + sizes[size,0] + "x" + sizes[size,1] });
                        PlayerPrefs.SetInt("EnglishLang", language);
                        if (LanguageController.instance) LanguageController.instance.ToEnglish = language == 1;
                        wait = Wait(); while (wait.MoveNext()) yield return null;
                        var label = next.transform.Find("SubmitLabel").GetComponent<TMPro.TMP_Text>(); label.ForceMeshUpdate();
                        Check(!label.isTextOverflowing && label.text == (language == 0 ? "Semak" : "Check"), scene + " final-action caption fits " + sizes[size,0] + "x" + sizes[size,1] + "/" + language);
                        AdventurePreview.CaptureGameView(Output + scene + "-" + sizes[size,0] + "x" + sizes[size,1] + "-" + language + ".png");
                    }
                EventSystem.current.SetSelectedGameObject(next.gameObject);
                next.onClick.Invoke(); wait = Wait(); while (wait.MoveNext()) yield return null;
                Check(confirmation.gameObject.activeSelf && last.gameObject.activeSelf && !result.gameObject.activeSelf, scene + " final arrow opens confirmation over last page");
                var no = confirmation.GetComponentsInChildren<Button>(true).First(b => b.name == "NoBtn");
                no.onClick.Invoke(); wait = Wait(); while (wait.MoveNext()) yield return null;
                Check(!confirmation.gameObject.activeSelf && last.gameObject.activeSelf, scene + " cancelling retains last page");
                Check(EventSystem.current.currentSelectedGameObject == next.gameObject, scene + " cancelling restores final-arrow focus");
                var controller = quiz.GetComponent<QuizController>();
                Array.Clear(controller.noQuiz, 0, controller.noQuiz.Length); controller.noQuiz[0] = true;
                next.onClick.Invoke(); wait = Wait(); while (wait.MoveNext()) yield return null;
                var yes = confirmation.GetComponentsInChildren<Button>(true).First(b => b.name == "YesBtn");
                yes.onClick.Invoke(); wait = Wait(); while (wait.MoveNext()) yield return null;
                Check(!confirmation.gameObject.activeSelf && result.gameObject.activeSelf && controller.score == 1, scene + " existing confirmation calculates score and opens result");
                AdventurePreview.CaptureGameView(Output + scene + "-result.png");
                result.gameObject.SetActive(false); timeout.gameObject.SetActive(false);
                timer.timer = .01f; timer.isSwitch = false; timer.isStopping = false;
                wait = Wait(); while (wait.MoveNext()) yield return null;
                Check(timer.isSwitch && result.gameObject.activeSelf && timeout.gameObject.activeSelf && !confirmation.gameObject.activeSelf, scene + " timeout opens result and TimeUp without submit confirmation");
                result.gameObject.SetActive(false); timeout.gameObject.SetActive(false); timer.isStopping = true;
                foreach (string name in new[] { "AudioPanelParent", "ProfilePanelParent" })
                {
                    var owner = canvas.Find(name).GetComponent<AudioController>();
                    owner.gameObject.SetActive(true);
                    wait = Wait(); while (wait.MoveNext()) yield return null;
                    var button = canvas.GetComponentsInChildren<Button>(true).First(b => Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Any(i => b.onClick.GetPersistentTarget(i) == owner && b.onClick.GetPersistentMethodName(i) == "isShowing"));
                    owner.animator.SetBool("isClosing", true); button.onClick.Invoke();
                    Check(!owner.animator.GetBool("isClosing"), scene + " " + name + " dropdown invokes its existing animator owner");
                    owner.isHiding();
                }
            }
            var home = SceneManager.LoadSceneAsync("MainMenu"); while (!home.isDone) yield return null;
        }
    }
}
