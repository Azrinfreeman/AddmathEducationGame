using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FunMath.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FunMath.Editor
{
    /// <summary>Repeatable Play-mode checks for recording coverage and narration playback controls.</summary>
    public static class LessonNarrationVerification
    {
        private static IEnumerator checks;
        private static int language;
        private static int voiceSetting;
        private static float masterVolume;
        private static float effectsVolume;
        private static bool background;
        private static readonly string[] PreferenceKeys = { "EnglishLang", "UI.Voice", "MasterVolume", "sfx" };
        private static bool[] existingKeys;
        private static readonly List<string> rows = new List<string>();
        public static string Status { get; private set; } = "Idle";

        [MenuItem("Fun Math/UI/Review/Verify lesson narration (Play mode)")]
        public static void Start()
        {
            if (!EditorApplication.isPlaying || checks != null)
                throw new InvalidOperationException("Enter Play mode first; only one narration verification can run.");
            existingKeys = PreferenceKeys.Select(PlayerPrefs.HasKey).ToArray();
            language = PlayerPrefs.GetInt("EnglishLang", 0);
            voiceSetting = PlayerPrefs.GetInt("UI.Voice", 1);
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1);
            effectsVolume = PlayerPrefs.GetFloat("sfx", 1);
            background = Application.runInBackground;
            Application.runInBackground = true;
            PlayerPrefs.SetInt("UI.Voice", 1);
            EditorApplication.isPaused = false;
            rows.Clear();
            rows.Add("scene\tpage\tlang\ttextHash\trecordingMatches");
            Status = "Running";
            checks = Verify();
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayModeChanged;
        }

        private static void Tick()
        {
            try { if (!checks.MoveNext()) Finish("Passed"); }
            catch (Exception exception) { Finish("FAILED: " + exception.Message); Debug.LogException(exception); }
        }

        private static void PlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode) Finish("Stopped");
        }

        private static void Finish(string result)
        {
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= PlayModeChanged;
            (checks as IDisposable)?.Dispose();
            checks = null;
            PlayerPrefs.SetInt("EnglishLang", language);
            PlayerPrefs.SetInt("UI.Voice", voiceSetting);
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("sfx", effectsVolume);
            for (int i = 0; i < PreferenceKeys.Length; i++) if (!existingKeys[i]) PlayerPrefs.DeleteKey(PreferenceKeys[i]);
            if (LanguageController.instance) LanguageController.instance.ToEnglish = language == 1;
            Application.runInBackground = background;
            Directory.CreateDirectory("Temp/narration-audit");
            File.WriteAllLines("Temp/narration-audit/runtime.tsv", rows);
            Status = result + " (" + (rows.Count - 1) + " page/language checks)";
            File.WriteAllText("Temp/narration-audit/runtime-status.txt", Status);
            Debug.Log("Narration verification: " + Status);
        }

        private static IEnumerator Verify()
        {
            var scripts = File.ReadAllLines("docs/tutorial/lesson-voice-script.tsv").Skip(1)
                .Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Split('\t')).ToArray();
            foreach (var sceneRows in scripts.GroupBy(r => r[1]))
            {
                var load = EditorSceneManager.LoadSceneAsyncInPlayMode(sceneRows.Key, new LoadSceneParameters(LoadSceneMode.Single));
                while (!load.isDone) yield return null;
                int frame = Time.frameCount;
                while (Time.frameCount < frame + 3) yield return null;
                var voice = UnityEngine.Object.FindFirstObjectByType<LessonVoice>();
                Require(voice != null, "No LessonVoice: " + sceneRows.Key);
                var pages = voice.pages.GetComponentsInChildren<LessonNarration>(true).ToDictionary(p => p.name);
                foreach (Transform child in voice.pages) child.gameObject.SetActive(false);
                foreach (var row in sceneRows)
                {
                    var page = pages[row[3]];
                    page.transform.SetAsFirstSibling();
                    page.gameObject.SetActive(true);
                    PlayerPrefs.SetInt("EnglishLang", row[2] == "en" ? 1 : 0);
                    frame = Time.frameCount;
                    while (Time.frameCount < frame + 3) yield return null;
                    string speech = voice.BuildSpeech();
                    Require(speech == row[5], "Runtime speech differs from export: " + string.Join("/", row.Take(4)));
                    Require(LessonVoice.Hash(speech) == row[4], "Stale fingerprint");
                    Require(voice.RecordingFor(speech) != null, "Missing recording: " + string.Join("/", row.Take(4)));
                    Require(voice.RecordingFor(speech + " edited") == null, "Edited speech accepted a stale recording");
                    rows.Add(sceneRows.Key + "\t" + row[3] + "\t" + row[2] + "\t" + row[4] + "\tTrue");
                    page.gameObject.SetActive(false);
                    Status = "Running (" + (rows.Count - 1) + " checks)";
                }
            }

            // Exercise actual bundled playback and settings against the loaded lesson.
            var current = UnityEngine.Object.FindFirstObjectByType<LessonVoice>();
            var first = current.pages.GetChild(0).gameObject;
            first.SetActive(true);
            PlayerPrefs.SetInt("EnglishLang", 0);
            PlayerPrefs.SetInt("UI.Voice", 1);
            int readyFrame = Time.frameCount;
            while (Time.frameCount < readyFrame + 3) yield return null;
            var source = current.GetComponent<AudioSource>();
            for (int selectedLanguage = 0; selectedLanguage < 2; selectedLanguage++)
            {
                PlayerPrefs.SetInt("EnglishLang", selectedLanguage);
                readyFrame = Time.frameCount;
                while (Time.frameCount < readyFrame + 3) yield return null;
                current.Listen();
                double deadline = EditorApplication.timeSinceStartup + 2;
                while (!source.isPlaying && EditorApplication.timeSinceStartup < deadline) yield return null;
                Require(source.isPlaying, "Listen did not start a recording");
                current.Listen();
                Require(!source.isPlaying, "Second Listen did not stop playback");
            }
            current.Listen();
            PlayerPrefs.SetFloat("MasterVolume", .5f);
            PlayerPrefs.SetFloat("sfx", .4f);
            readyFrame = Time.frameCount;
            while (Time.frameCount < readyFrame + 3) yield return null;
            Require(Mathf.Abs(source.volume - .2f) < .001f, "Playing recording did not follow volume changes");
            Require(source.isPlaying, "Recording stopped unexpectedly during volume update");
            PlayerPrefs.SetInt("EnglishLang", 0);
            readyFrame = Time.frameCount;
            while (Time.frameCount < readyFrame + 3) yield return null;
            Require(!source.isPlaying, "Language change did not stop old recording");
            current.Listen();
            PlayerPrefs.SetInt("UI.Voice", 0);
            readyFrame = Time.frameCount;
            while (Time.frameCount < readyFrame + 3) yield return null;
            Require(!source.isPlaying && !current.listenButton.gameObject.activeSelf, "Voice off did not stop and hide Listen");
            current.Listen();
            Require(!source.isPlaying, "Listen bypassed voice-off setting");
            PlayerPrefs.SetInt("UI.Voice", 1);
            current.Listen();
            first.SetActive(false);
            readyFrame = Time.frameCount;
            while (Time.frameCount < readyFrame + 3) yield return null;
            Require(!source.isPlaying, "Page change did not stop recording");
            first.SetActive(true);
            current.Listen();
            current.gameObject.SetActive(false);
            Require(!source.isPlaying, "Disabling narration did not stop playback");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
