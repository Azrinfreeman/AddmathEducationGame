using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FunMath.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace FunMath.Editor
{
    /// <summary>Repairs only recognized legacy defects. Unknown actions remain in the report.</summary>
    public static class LegacyActionRepair
    {
        private const string Output = "docs/tutorial/review/wiring/";
        private static string[] paths;
        private static int index;
        private static bool apply;
        private static SceneSetup[] setup;
        private static readonly List<string> decisions = new List<string>();
        private static readonly List<string> summary = new List<string>();
        private static readonly List<string> audit = new List<string>();
        public static string Status { get; private set; } = "Idle";

        [MenuItem("Fun Math/UI/Wiring/1 - Inspect known legacy actions")]
        public static void Inspect() { Start(false); }
        [MenuItem("Fun Math/UI/Wiring/2 - Repair known legacy actions")]
        public static void Repair() { Start(true); }

        private static IEnumerable<T> All<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<T>(true));
        private static string Hierarchy(Transform item) => item.parent ? Hierarchy(item.parent) + "/" + item.name : item.name;

        private static void Start(bool write)
        {
            if (EditorApplication.isPlaying || paths != null) throw new InvalidOperationException("Exit Play mode and finish the current wiring review first.");
            if (SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the current scene first. Preserve a file snapshot before repairs.");
            Directory.CreateDirectory(Output); setup = EditorSceneManager.GetSceneManagerSetup();
            paths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" }).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
            index = 0; apply = write;
            decisions.Clear(); decisions.Add("scene\tcontrol\tmethod\tdecision");
            summary.Clear(); summary.Add("scene\tmissingBefore\tmissingAfter\ttransformIdentitiesPreserved");
            audit.Clear(); audit.Add("scene\tbuttons\twithoutFeedback\tmissingFonts\tmissingScripts\tmissingActionTargets\tresponsiveShell");
            Status = "Running"; EditorApplication.update += Tick;
        }

        private static int Missing(Scene scene) => All<Button>(scene).Sum(b => Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Count(i => !b.onClick.GetPersistentTarget(i)));
        private static string[] Identities(Scene scene) => All<Transform>(scene).Select(t => GlobalObjectId.GetGlobalObjectIdSlow(t).ToString()).OrderBy(id => id).ToArray();

        private static void Tick()
        {
            try
            {
                if (index == paths.Length) { Finish("Completed"); return; }
                string path = paths[index++]; Status = index + "/" + paths.Length + " " + path;
                var scene = EditorSceneManager.OpenScene(path);
                int before = Missing(scene); var identities = Identities(scene);
                bool changed = RepairScene(scene, apply);
                if (identities.Except(Identities(scene)).Any()) throw new InvalidOperationException("Existing object identity changed: " + path);
                if (apply && changed) { EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); }
                int after = Missing(scene);
                summary.Add(path + "\t" + before + "\t" + after + "\tTrue");
                var buttons = All<Button>(scene).ToArray();
                audit.Add(path + "\t" + buttons.Length + "\t" + buttons.Count(b => !b.GetComponent<KidButton>()) + "\t" + All<TMP_Text>(scene).Count(t => !t.font) + "\t" + All<Transform>(scene).Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)) + "\t" + after + "\t" + All<Canvas>(scene).Any(c => c.name == "AdventureShell"));
            }
            catch (Exception e) { decisions.Add("ERROR\t" + e.Message); Finish("Failed"); Debug.LogException(e); }
        }

        private static void Finish(string status)
        {
            EditorApplication.update -= Tick;
            string prefix = apply ? "repair" : "inspect";
            File.WriteAllLines(Output + prefix + "-decisions.tsv", decisions);
            File.WriteAllLines(Output + prefix + "-summary.tsv", summary);
            if (apply && status == "Completed") File.WriteAllLines("docs/tutorial/ui-scene-audit.tsv", audit);
            paths = null; Status = status;
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }

        public static bool RepairScene(Scene scene, bool write)
        {
            bool changed = false;
            foreach (var timer in All<TimerController>(scene))
            {
                var canvas = timer.GetComponentInParent<Canvas>();
                var result = canvas ? canvas.transform.Find("Prompted (2)") : null;
                var timeout = canvas ? canvas.transform.Find("TimeUp") : null;
                if (result && timeout && (timer.prompted != result || timer.timeoutGameobject != timeout))
                {
                    decisions.Add(scene.path + "\t" + Hierarchy(timer.transform) + "\ttimeout\tBind existing results and TimeUp panels");
                    if (write) { timer.prompted = result; timer.timeoutGameobject = timeout; EditorUtility.SetDirty(timer); changed = true; }
                }
            }
            foreach (var button in All<Button>(scene))
            {
                var serialized = new SerializedObject(button);
                var calls = serialized.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
                var canvas = button.GetComponentInParent<Canvas>(true);
                var page = button.transform.parent;
                int pageNumber;
                int lastPage = page && page.parent ? page.parent.Cast<Transform>().Select(t => int.TryParse(t.name, out int number) ? number : 0).DefaultIfEmpty().Max() : 0;
                bool terminalQuiz = scene.path.StartsWith("Assets/Scenes/Quizzes/") && button.name == "nextButton" && int.TryParse(page.name, out pageNumber) && pageNumber == lastPage;
                if (terminalQuiz && calls.arraySize == 3 && !button.onClick.GetPersistentTarget(0) && !button.onClick.GetPersistentTarget(1) && button.onClick.GetPersistentTarget(2) == button.transform.parent.gameObject)
                {
                    var confirmation = canvas.transform.Find("Prompted (1)");
                    if (confirmation)
                    {
                        decisions.Add(scene.path + "\t" + Hierarchy(button.transform) + "\tfinal-page\tOpen existing submit confirmation; retain page for cancellation");
                        if (write)
                        {
                            for (int i = 2; i >= 0; i--) UnityEventTools.RemovePersistentListener(button.onClick, i);
                            UnityEventTools.AddBoolPersistentListener(button.onClick, confirmation.gameObject.SetActive, true);
                            if (!button.transform.Find("SubmitLabel")) ConfigureFinalAction(button);
                            EditorUtility.SetDirty(button); changed = true;
                        }
                        continue;
                    }
                }
                if (terminalQuiz && button.onClick.GetPersistentEventCount() == 1 && button.onClick.GetPersistentMethodName(0) == "SetActive" && button.onClick.GetPersistentTarget(0) && button.onClick.GetPersistentTarget(0).name == "Prompted (1)" && !button.transform.Find("SubmitLabel"))
                {
                    decisions.Add(scene.path + "\t" + Hierarchy(button.transform) + "\tfinal-action\tShow Check/Semak on the existing final-page control");
                    if (write) { ConfigureFinalAction(button); changed = true; }
                }
                for (int i = calls.arraySize - 1; i >= 0; i--)
                {
                    if (button.onClick.GetPersistentTarget(i)) continue;
                    var call = calls.GetArrayElementAtIndex(i);
                    string type = call.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue;
                    string method = call.FindPropertyRelative("m_MethodName").stringValue;
                    UnityEngine.Object target = null;
                    string decision = "UNRESOLVED";
                    if (type.StartsWith("AudioController,") && method == "isShowing" && (button.name == "Dropdown" || button.name == "Dropdown (1)"))
                    {
                        string name = button.name == "Dropdown" ? "AudioPanelParent" : "ProfilePanelParent";
                        var owners = All<AudioController>(scene).Where(a => a.name == name && a.GetComponent<Animator>()).ToArray();
                        if (owners.Length == 1) { target = owners[0]; decision = "Reconnect unique existing " + name; }
                    }
                    else if (type.StartsWith("UnityEngine.Video.VideoPlayer,") && (method == "Play" || method == "Stop"))
                    {
                        var players = All<VideoPlayer>(scene).Where(v => v.name == "video").ToArray();
                        if (players.Length == 1) { target = players[0]; decision = "Reconnect unique existing video player"; }
                    }
                    bool obsoleteGame = scene.path.Contains("/Games/WhackAMole/") && button.name == "executeButton" && !button.gameObject.activeSelf && method == "loadScene" && call.FindPropertyRelative("m_Arguments.m_StringArgument").stringValue == "AddingGame";
                    bool obsoleteCredits = (scene.name == "CreditPage" || scene.name == "PenghargaanPage") && canvas && !canvas.gameObject.activeSelf && button.name == "nextButton" && (method == "SetActive" || method == "SetAsFirstSibling");
                    bool empty = string.IsNullOrEmpty(type) && string.IsNullOrEmpty(method);
                    bool obsoleteHelp = scene.path == "Assets/Scenes/Games/AddingGae/Level1.unity" && button.name == "YesBtn" && !button.gameObject.activeSelf && Hierarchy(button.transform).Contains("/HowToPlay/") && (method == "QuitApp" || method == "SetActive");
                    bool obsoleteQuizCopy = scene.path == "Assets/Scenes/Games/GamePrasekolah/GamePrasekolah.unity" && method == "CollectAndCheck" && type.StartsWith("QuizController,") && !All<QuizController>(scene).Any() && button.onClick.GetPersistentEventCount() == 3 && button.onClick.GetPersistentTarget(2) && button.onClick.GetPersistentTarget(2).name == "EndPanel";
                    bool remove = obsoleteGame || obsoleteCredits || obsoleteHelp || obsoleteQuizCopy || empty;
                    if (remove) decision = "Remove obsolete no-op on retained legacy control";
                    decisions.Add(scene.path + "\t" + Hierarchy(button.transform) + "\t" + method + "\t" + decision);
                    if (!write) continue;
                    if (target)
                    {
                        call.FindPropertyRelative("m_Target").objectReferenceValue = target;
                        serialized.ApplyModifiedProperties(); changed = true;
                    }
                    else if (remove)
                    {
                        UnityEventTools.RemovePersistentListener(button.onClick, i);
                        serialized.Update(); changed = true;
                    }
                }
                if (write && changed) EditorUtility.SetDirty(button);
            }
            return changed;
        }

        private static void ConfigureFinalAction(Button button)
        {
            var theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>("Assets/UI/Resources/AdventureTheme.asset");
            button.gameObject.SetActive(true);
            var image = button.GetComponent<Image>();
            image.sprite = theme.roundedPanel; image.type = Image.Type.Sliced; image.preserveAspect = false; image.color = theme.sections[2];
            var surface = button.GetComponent<ThemedSurface>();
            if (surface) { surface.section = 2; surface.Apply(); }
            var rect = new GameObject("SubmitLabel", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(button.transform, false); rect.anchorMin = new Vector2(.04f, .08f); rect.anchorMax = new Vector2(.96f, .92f); rect.offsetMin = rect.offsetMax = Vector2.zero;
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = theme.displayFont; label.text = "Semak"; label.color = theme.ink;
            label.fontSize = label.fontSizeMax = 38; label.fontSizeMin = 28; label.enableAutoSizing = true;
            label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            var bilingual = rect.gameObject.AddComponent<BilingualLabel>(); bilingual.malay = "Semak"; bilingual.english = "Check";
        }
    }
}
