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
using UnityEngine.UI;

namespace FunMath.Editor
{
    public static class LessonUIBuilder
    {
        private static AdventureTheme theme;

        [MenuItem("Fun Math/UI/7 - Upgrade lesson layouts")]
        public static void Upgrade()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the open scene first.");
            theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>("Assets/UI/Resources/AdventureTheme.asset");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var rows = new List<string> { "scene\tpages\tcallbacksBefore\tcallbacksAfter\tmissingActionTargetsBefore\tmissingActionTargetsAfter\tavailableBranchMissingTargets\trepairedFinalNavigation\tspriteLessOpaqueImagesBefore" };
            try
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Lessons" }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    // The four lesson hubs already have their own responsive shell.
                    if (path.Split('/').Length < 5) continue;
                    var scene = EditorSceneManager.OpenScene(path);
                    var check = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<CheckPages>(true)).Single();
                    var canvas = check.GetComponentInParent<Canvas>();
                    var buttons = canvas.GetComponentsInChildren<Button>(true);
                    int before = buttons.Sum(b => b.onClick.GetPersistentEventCount());
                    int missingBefore = buttons.Sum(b => Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Count(i => !b.onClick.GetPersistentTarget(i)));
                    int emptyImages = check.GetComponentsInChildren<Image>(true).Count(i => !i.sprite && i.color.a > .9f);
                    if (!canvas.GetComponent<LessonScreenLayout>())
                    {
                        BuildFrame(canvas, check);
                        foreach (Transform page in check.transform) BuildPage(page);
                    }
                    RefreshOwnedLayout(canvas, check);
                    int removedCalls;
                    bool repaired = RepairFinalNavigation(check, out removedCalls);
                    EditorSceneManager.SaveScene(scene);
                    buttons = canvas.GetComponentsInChildren<Button>(true);
                    int after = buttons.Sum(b => b.onClick.GetPersistentEventCount());
                    if (before - removedCalls != after) throw new InvalidOperationException("Unexpected lesson callback count change: " + path);
                    int missing = buttons.Sum(b => Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Count(i => !b.onClick.GetPersistentTarget(i)));
                    int availableMissing = buttons.Where(AvailableBranch).Sum(b => Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Count(i => !b.onClick.GetPersistentTarget(i)));
                    rows.Add(path + "\t" + check.transform.childCount + "\t" + before + "\t" + after + "\t" + missingBefore + "\t" + missing + "\t" + availableMissing + "\t" + repaired + "\t" + emptyImages);
                }
                File.WriteAllLines("docs/tutorial/lesson-ui-audit.tsv", rows);
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        private static void BuildFrame(Canvas canvas, CheckPages check)
        {
            var oldTitle = canvas.transform.Find("Title");
            var titles = Titles(canvas.gameObject.scene.name);
            var heading = Label("LessonHeading", canvas.transform, titles.Item1, titles.Item2, 64);
            oldTitle.gameObject.SetActive(false);
            var layout = canvas.gameObject.AddComponent<LessonScreenLayout>();
            layout.pages = (RectTransform)check.transform;
            layout.heading = heading.rectTransform;
            layout.back = canvas.transform.Find("exitButton") as RectTransform;
            layout.settings = canvas.transform.Find("settingDrop") as RectTransform;
            SkinButton(layout.back.GetComponent<Button>(), "Keluar", "Exit", -1);
            if (layout.settings)
            {
                layout.settings.localScale = Vector3.one;
                var gear = layout.settings.Find("SettingProfile") as RectTransform;
                if (gear)
                {
                    gear.pivot = Vector2.one * .5f;
                    gear.localScale = Vector3.one;
                    QuizPageLayout.Place(gear, 0, 0, 1, 1);
                    SkinButton(gear.GetComponent<Button>(), "Ibu bapa", "Parents", -1);
                }
                var duplicate = layout.settings.Find("settingDrop");
                if (duplicate) duplicate.gameObject.SetActive(false);
            }
            foreach (Transform child in canvas.transform)
            {
                if (!child.name.StartsWith("Prompted", StringComparison.Ordinal)) continue;
                var body = child.Find("Image");
                if (body && !body.GetComponent<ModalBodyFit>()) body.gameObject.AddComponent<ModalBodyFit>();
            }
        }

        private static bool AvailableBranch(Button button)
        {
            if (!button.enabled) return false;
            for (var node = button.transform; node; node = node.parent)
                if (!node.gameObject.activeSelf && !node.GetComponent<LessonPageLayout>()) return false;
            return true;
        }

        private static Tuple<string, string> Titles(string scene)
        {
            if (scene.Contains("Pengalaman")) return Tuple.Create("Pengalaman pranombor", "Pre-number skills");
            if (scene.Contains("memahamisiri")) return Tuple.Create("Memahami siri nombor", "Number sequences");
            if (scene.Contains("OperasiNombor")) return Tuple.Create("Operasi nombor", "Number operations");
            if (scene.Contains("Kenali")) return Tuple.Create("Kenali minit", "Understanding minutes");
            if (scene.Contains("Sebut")) return Tuple.Create("Sebut dan tulis waktu", "Say and write the time");
            if (scene.Contains("Rekod")) return Tuple.Create("Rekod waktu", "Record time");
            if (scene.Contains("Pekaitan")) return Tuple.Create("Perkaitan waktu", "Time relationships");
            if (scene.Contains("Perimeter")) return Tuple.Create("Perimeter", "Perimeter");
            if (scene.Contains("Luas")) return Tuple.Create("Luas", "Area");
            return Tuple.Create("Isi padu", "Volume");
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static TMP_Text Label(string name, Transform parent, string malay, string english, float size)
        {
            var text = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            StyleText(text, size, 30);
            text.text = PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? english : malay;
            if (malay != english)
            {
                var language = text.gameObject.AddComponent<BilingualLabel>();
                language.malay = malay;
                language.english = english;
            }
            return text;
        }

        private static void StyleText(TMP_Text text, float maximum, float minimum)
        {
            text.font = theme.bodyFont;
            text.color = theme.ink;
            text.enableAutoSizing = true;
            text.fontSizeMin = minimum;
            text.fontSizeMax = Mathf.Max(minimum, maximum);
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
        }

        private static void SkinButton(Button button, string bm, string en, int section)
        {
            foreach (Transform child in button.transform) child.gameObject.SetActive(false);
            var oldLanguage = button.GetComponent<ChangeLanguage>();
            if (oldLanguage) oldLanguage.enabled = false;
            button.transform.localScale = Vector3.one;
            button.transform.localRotation = Quaternion.identity;
            var image = button.GetComponent<Image>() ?? button.gameObject.AddComponent<Image>();
            button.targetGraphic = image;
            image.raycastTarget = true;
            button.colors = ColorBlock.defaultColorBlock;
            image.sprite = theme.roundedPanel;
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            var surface = button.GetComponent<ThemedSurface>() ?? button.gameObject.AddComponent<ThemedSurface>();
            surface.theme = theme;
            surface.section = section;
            surface.Apply();
            var label = button.transform.Find("ActionLabel")?.GetComponent<TMP_Text>();
            if (!label) label = Label("ActionLabel", button.transform, bm, en, 42);
            label.gameObject.SetActive(true);
            StyleText(label, 42, 30);
            var language = label.GetComponent<BilingualLabel>() ?? label.gameObject.AddComponent<BilingualLabel>();
            language.malay = bm;
            language.english = en;
            label.text = PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? en : bm;
            QuizPageLayout.Place(label.rectTransform, .04f, .04f, .96f, .96f);
        }

        private static void BuildPage(Transform page)
        {
            var layout = page.gameObject.AddComponent<LessonPageLayout>();
            QuizPageLayout.Place(page as RectTransform, 0, 0, 1, 1);
            layout.question = page.Find("Questions") as RectTransform;
            layout.next = page.Find("nextButton") as RectTransform;
            layout.previous = page.Find("previousButton") as RectTransform;
            layout.answerGroups = page.Cast<Transform>().Where(t => t.name.StartsWith("Answers", StringComparison.Ordinal)).Cast<RectTransform>().ToArray();
            if (layout.next) SkinButton(layout.next.GetComponent<Button>(), "Seterusnya", "Next", 0);
            if (layout.previous) SkinButton(layout.previous.GetComponent<Button>(), "Sebelum", "Previous", -1);
            // Only the old decorative scale-in is replaced. Other content Animators are retained.
            var animator = page.GetComponent<Animator>();
            if (animator && animator.runtimeAnimatorController && animator.runtimeAnimatorController.animationClips
                .SelectMany(AnimationUtility.GetCurveBindings).All(b => b.propertyName.StartsWith("m_LocalScale", StringComparison.Ordinal)))
                animator.enabled = false;

            var area = Rect("DiagramArea", page);
            var background = area.gameObject.AddComponent<Image>();
            background.sprite = theme.roundedPanel;
            background.type = Image.Type.Sliced;
            background.color = theme.cream;
            background.raycastTarget = false;
            var areaTheme = area.gameObject.AddComponent<ThemedSurface>();
            areaTheme.theme = theme;
            areaTheme.section = -1;
            layout.diagramArea = area;
            var content = Rect("LessonContent", area);
            content.sizeDelta = layout.designSize;
            layout.diagramContent = content;

            var cards = page.Cast<Transform>().Where(t => t != area && t.GetComponentsInChildren<TMP_Text>(true).Any(x => x.name == "textLabel")).ToArray();
            foreach (var card in cards)
            {
                foreach (var text in card.GetComponentsInChildren<TMP_Text>(true).Where(t => t.name == "textLabel")) text.gameObject.SetActive(false);
                foreach (var slider in card.GetComponentsInChildren<Slider>(true)) slider.gameObject.SetActive(false);
                card.SetParent(content, false);
                var rect = (RectTransform)card;
                rect.pivot = Vector2.one * .5f;
                rect.anchorMin = rect.anchorMax = Vector2.one * .5f;
                rect.anchoredPosition = Vector2.zero;
                foreach (var text in card.GetComponentsInChildren<TMP_Text>(true))
                    if (text.name != "textLabel") StyleText(text, Mathf.Max(36, text.fontSize), 24);
            }
            // Sprite-less containers should not obscure their own text with a white rectangle.
            foreach (var image in content.GetComponentsInChildren<Image>(true))
            {
                if (!image.sprite && !image.GetComponent<Button>()) image.color = Color.clear;
                if (!image.GetComponent<Button>()) image.raycastTarget = false;
            }
            var questionImage = layout.question.GetComponentInChildren<Image>(true);
            if (questionImage)
            {
                questionImage.sprite = theme.roundedPanel;
                questionImage.type = Image.Type.Sliced;
                questionImage.color = theme.cream;
            }
            var hints = new List<RectTransform>();
            if (questionImage)
            {
                foreach (Transform child in questionImage.transform.Cast<Transform>().ToArray())
                {
                    if (child.GetComponent<TMP_Text>() || !child.GetComponent<ChangeLanguage>()) continue;
                    var texts = child.GetComponentsInChildren<TMP_Text>(true);
                    if (texts.Length < 2) continue;
                    var hint = Label("ReadingHint", page, texts[0].text, texts[1].text, 36);
                    hints.Add(hint.rectTransform);
                    child.gameObject.SetActive(false);
                }
            }
            layout.hints = hints.ToArray();
            foreach (var text in layout.question.GetComponentsInChildren<TMP_Text>(true)) StyleText(text, 48, 28);
            var actions = page.GetComponentsInChildren<Button>(true).Where(b => b.transform != layout.next && b.transform != layout.previous
                && b.onClick.GetPersistentEventCount() > 0 && !b.GetComponentInParent<VideoController>()).ToArray();
            layout.actions = actions.Select(b => (RectTransform)b.transform).ToArray();
            foreach (var action in actions)
            {
                bool ar = Enumerable.Range(0, action.onClick.GetPersistentEventCount()).Any(i => action.onClick.GetPersistentMethodName(i) == "AddARScene");
                action.transform.SetParent(page, false);
                SkinButton(action, ar ? "Cuba AR" : "Semak", ar ? "Try AR" : "Check", ar ? 2 : 1);
            }
            if (actions.Any(a => a.GetComponent<AddScene>()))
                foreach (var hint in hints)
                    SetModalText(hint.GetComponent<TMP_Text>(), "Tekan Cuba AR untuk meneroka.", "Choose Try AR to explore.", 36);
            layout.progressLabel = Label("LessonProgress", page, "", "", 38);
            layout.progressBar = CreateProgressBar(page);
            foreach (var group in layout.answerGroups)
                foreach (var text in group.GetComponentsInChildren<TMP_Text>(true)) StyleText(text, 46, 28);
        }

        private static Slider CreateProgressBar(Transform parent)
        {
            var root = Rect("LessonProgressBar", parent);
            var background = root.gameObject.AddComponent<Image>();
            background.sprite = theme.roundedPanel;
            background.type = Image.Type.Sliced;
            background.color = new Color32(200, 222, 222, 255);
            background.raycastTarget = false;
            var fill = Rect("Fill", root);
            QuizPageLayout.Place(fill, 0, 0, 1, 1);
            var image = fill.gameObject.AddComponent<Image>();
            image.sprite = theme.roundedPanel;
            image.type = Image.Type.Sliced;
            image.color = theme.sections[1];
            image.raycastTarget = false;
            var slider = root.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.interactable = false;
            return slider;
        }

        private static bool RepairFinalNavigation(CheckPages check, out int removedCalls)
        {
            removedCalls = 0;
            var last = check.transform.Cast<Transform>().FirstOrDefault(p => p.name == check.transform.childCount.ToString());
            if (!last) return false;
            var layout = last.GetComponent<LessonPageLayout>();
            if (!layout || !layout.next) return false;
            var next = layout.next.GetComponent<Button>();
            bool broken = Enumerable.Range(0, next.onClick.GetPersistentEventCount()).Any(i => !next.onClick.GetPersistentTarget(i));
            if (!broken) return false;
            // Some copied lessons lost the Check target as well. Restore the existing
            // confirmation only when its own callbacks already lead to the completion panel.
            var canvas = check.GetComponentInParent<Canvas>();
            var confirmation = canvas.transform.Find("Prompted (1)");
            var completion = canvas.transform.Find("Prompted (2)");
            bool hasCompletionRoute = confirmation && completion && confirmation.GetComponentsInChildren<Button>(true).Any(b =>
                Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Any(i => b.onClick.GetPersistentTarget(i) == completion.gameObject && b.onClick.GetPersistentMethodName(i) == "SetActive"));
            if (hasCompletionRoute)
            {
                foreach (var action in layout.actions.Select(a => a.GetComponent<Button>()))
                    for (int i = 0; i < action.onClick.GetPersistentEventCount(); i++)
                        if (!action.onClick.GetPersistentTarget(i) && action.onClick.GetPersistentMethodName(i) == "SetActive")
                            UnityEventTools.RegisterBoolPersistentListener(action.onClick, i, confirmation.gameObject.SetActive, true);
            }
            var finish = layout.actions.Select(a => a.GetComponent<Button>()).FirstOrDefault(b => b && b.gameObject.activeSelf &&
                Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Any(i => b.onClick.GetPersistentTarget(i) && b.onClick.GetPersistentMethodName(i) == "SetActive"));
            if (!finish) return false;
            removedCalls = next.onClick.GetPersistentEventCount() - finish.onClick.GetPersistentEventCount();
            next.onClick = finish.onClick;
            var label = next.GetComponentInChildren<BilingualLabel>(true);
            if (label) { label.malay = "Semak"; label.english = "Check"; }
            finish.gameObject.SetActive(false);
            return true;
        }

        private static void RefreshOwnedLayout(Canvas canvas, CheckPages check)
        {
            foreach (var layout in check.GetComponentsInChildren<LessonPageLayout>(true))
            {
                foreach (RectTransform card in layout.diagramContent)
                {
                    card.pivot = Vector2.one * .5f;
                    card.anchoredPosition = Vector2.zero;
                }
                QuizPageLayout.Place(layout.progressBar.fillRect, 0, 0, 1, 1);
                if (!layout.diagramArea.GetComponent<ThemedSurface>())
                {
                    var surface = layout.diagramArea.gameObject.AddComponent<ThemedSurface>();
                    surface.theme = theme;
                    surface.section = -1;
                    surface.Apply();
                }
                if (layout.actions.Any(a => a.GetComponent<AddScene>()))
                    foreach (var hint in layout.hints)
                        SetModalText(hint.GetComponent<TMP_Text>(), "Tekan Cuba AR untuk meneroka.", "Choose Try AR to explore.", 36);
            }
            var screen = canvas.GetComponent<LessonScreenLayout>();
            var gear = screen.settings ? screen.settings.Find("SettingProfile") : null;
            if (gear && !gear.Find("ActionLabel")) SkinButton(gear.GetComponent<Button>(), "Ibu bapa", "Parents", -1);
            var duplicate = screen.settings ? screen.settings.Find("settingDrop") : null;
            if (duplicate) duplicate.gameObject.SetActive(false);
            foreach (Transform root in canvas.transform)
            {
                if (!root.name.StartsWith("Prompted", StringComparison.Ordinal)) continue;
                var body = root.Find("Image");
                if (!body) continue;
                var yes = body.Find("YesBtn")?.GetComponent<Button>();
                var no = body.Find("NoBtn")?.GetComponent<Button>();
                if (!yes || !no) continue;
                bool completed = root.name == "Prompted (2)";
                bool leaving = root.name == "Prompted";
                SkinButton(yes, completed ? "Ulang" : leaving ? "Keluar" : "Selesai", completed ? "Replay" : leaving ? "Exit" : "Finish", 0);
                SkinButton(no, completed ? "Kembali" : "Sambung", completed ? "Back" : "Continue", -1);
                QuizPageLayout.Place((RectTransform)yes.transform, .06f, .04f, .47f, .16f);
                QuizPageLayout.Place((RectTransform)no.transform, .53f, .04f, .94f, .16f);
                var labels = body.Cast<Transform>().Select(t => t.GetComponent<TMP_Text>()).Where(t => t).ToArray();
                if (labels.Length < 2) continue;
                SetModalText(labels[0], completed ? "Tahniah!" : leaving ? "Mahu keluar?" : "Sudah selesai?",
                    completed ? "Well done!" : leaving ? "Leave this lesson?" : "Ready to finish?", 60);
                SetModalText(labels[1], completed ? "Kamu sudah menghabiskan pelajaran ini." : "Kamu boleh kembali untuk menyemak pelajaran.",
                    completed ? "You've finished this lesson." : "You can go back to review the lesson.", 40);
                QuizPageLayout.Place(labels[0].rectTransform, .06f, .36f, .94f, .47f);
                QuizPageLayout.Place(labels[1].rectTransform, .06f, .18f, .94f, .34f);
            }
        }

        private static void SetModalText(TMP_Text label, string malay, string english, float size)
        {
            StyleText(label, size, 30);
            var language = label.GetComponent<BilingualLabel>() ?? label.gameObject.AddComponent<BilingualLabel>();
            language.malay = malay;
            language.english = english;
            label.text = PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? english : malay;
        }
    }
}
