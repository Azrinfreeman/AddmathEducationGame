using System;
using System.Linq;
using FunMath.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FunMath.Editor
{
    public static class QuizUIBuilder
    {
        [MenuItem("Fun Math/UI/5 - Upgrade quiz layouts and result feedback")]
        public static void Upgrade()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the open scene before migration.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>("Assets/UI/Resources/AdventureTheme.asset");
            try
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" }))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var scene = EditorSceneManager.OpenScene(path);
                    bool changed = false;
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        foreach (var quiz in root.GetComponentsInChildren<QuizController>(true))
                        {
                            if (!path.StartsWith("Assets/Scenes/Quizzes/", StringComparison.Ordinal)) continue;
                            UpgradeQuiz(quiz, theme);
                            changed = true;
                        }
                        foreach (var score in root.GetComponentsInChildren<LoadScore>(true))
                        {
                            if (!path.StartsWith("Assets/Scenes/Quizzes/", StringComparison.Ordinal)) continue;
                            AddCelebration(score.gameObject, theme);
                            changed = true;
                        }
                        foreach (var transform in root.GetComponentsInChildren<RectTransform>(true))
                        {
                            if (transform.name != "winPanel") continue;
                            AddCelebration(transform.gameObject, theme);
                            changed = true;
                        }
                        foreach (var game in root.GetComponentsInChildren<GamePraController>(true))
                        {
                            AddCelebration(game.transform.GetChild(13).gameObject, theme);
                            changed = true;
                        }
                    }
                    if (changed) EditorSceneManager.SaveScene(scene);
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        private static void UpgradeQuiz(QuizController quiz, AdventureTheme theme)
        {
            var canvas = quiz.GetComponentInParent<Canvas>();
            var screen = GetOrAdd<QuizScreenLayout>(canvas.gameObject);
            screen.pages = (RectTransform)quiz.transform;
            screen.title = canvas.transform.Find("Title") as RectTransform;
            screen.timer = screen.title.Find("Timer") as RectTransform;
            screen.exit = canvas.transform.Find("exitButton") as RectTransform;
            screen.timer.localScale = Vector3.one;
            var timerBackground = screen.timer.GetChild(0).GetComponent<Image>();
            timerBackground.sprite = theme.roundedPanel;
            timerBackground.color = theme.cream;
            foreach (Transform child in screen.title)
            {
                if (child == screen.timer) continue;
                if (child.name != "QuizTitle") child.gameObject.SetActive(false);
            }
            var heading = screen.title.Find("QuizTitle")?.GetComponent<TMP_Text>();
            if (!heading)
            {
                heading = new GameObject("QuizTitle", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
                heading.transform.SetParent(screen.title, false);
            }
            QuizPageLayout.Place(heading.rectTransform, 0, 0, .68f, 1);
            ConfigureText(heading, theme, 58);
            var label = GetOrAdd<BilingualLabel>(heading.gameObject);
            string level = quiz.gameObject.scene.name.Replace("Quiz", "");
            label.malay = level == "Prasekolah" ? "Kuiz Prasekolah" : "Kuiz " + level.Replace("Tahap", "Tahap ");
            label.english = level == "Prasekolah" ? "Preschool Quiz" : "Level " + level.Replace("Tahap", "") + " Quiz";
            heading.text = PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? label.english : label.malay;
            screen.exit.GetComponent<Image>().preserveAspect = true;
            foreach (var controller in canvas.GetComponentsInChildren<PraKuizController>(true))
            {
                var body = controller.transform.Find("Panel/AudioPanel");
                if (body) GetOrAdd<ModalBodyFit>(body.gameObject);
            }
            foreach (Transform page in quiz.transform)
            {
                QuizPageLayout.Place(page as RectTransform, 0, 0, 1, 1);
                var layout = GetOrAdd<QuizPageLayout>(page.gameObject);
                layout.illustration = page.GetChild(0) as RectTransform;
                layout.question = page.Find("Questions") as RectTransform;
                layout.answers = page.Find("Answers") as RectTransform;
                if (!layout.question || !layout.answers)
                    throw new InvalidOperationException("Unsupported quiz page: " + quiz.gameObject.scene.path + "/" + page.name);
                layout.next = page.Find("nextButton") as RectTransform;
                layout.previous = page.Find("previousButton") as RectTransform;
                if (layout.next) layout.next.GetComponent<Image>().preserveAspect = true;
                if (layout.previous) layout.previous.GetComponent<Image>().preserveAspect = true;
                if (!layout.illustrationContent)
                {
                    var content = new GameObject("IllustrationContent", typeof(RectTransform)).GetComponent<RectTransform>();
                    content.SetParent(layout.illustration, false);
                    content.sizeDelta = Vector2.one * layout.illustrationDesignSize;
                    layout.illustrationContent = content;
                    foreach (Transform child in layout.illustration.Cast<Transform>().ToArray())
                    {
                        if (child == content) continue;
                        if (child.name == "textLabel") { layout.progressLabel = child as RectTransform; child.SetParent(page, false); }
                        else if (child.name == "Slider") { layout.progressBar = child as RectTransform; child.SetParent(page, false); }
                        else child.SetParent(content, false);
                    }
                }
                foreach (var image in layout.illustrationContent.GetComponentsInChildren<Image>(true))
                    image.raycastTarget = false;
                if (layout.progressBar) layout.progressBar.GetComponent<Slider>().interactable = false;
                foreach (var text in layout.question.GetComponentsInChildren<TMP_Text>(true)) ConfigureText(text, theme, 54);
                var questionImage = layout.question.GetComponentInChildren<Image>(true);
                if (questionImage)
                {
                    questionImage.sprite = theme.roundedPanel;
                    questionImage.type = Image.Type.Sliced;
                    questionImage.color = theme.cream;
                }
                foreach (var text in layout.answers.GetComponentsInChildren<TMP_Text>(true)) ConfigureText(text, theme, 54);
                var status = page.Find("AnswerStatus")?.GetComponent<TMP_Text>();
                if (!status)
                {
                    status = new GameObject("AnswerStatus", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
                    status.transform.SetParent(page, false);
                    QuizPageLayout.Place(status.rectTransform, .25f, .01f, .75f, .11f);
                    ConfigureText(status, theme, 38);
                    status.text = "";
                }
                var feedback = GetOrAdd<QuizAnswerFeedback>(page.gameObject);
                feedback.selection = layout.answers.GetComponent<buttonSelectedController>();
                feedback.caption = status;
            }
        }

        private static void ConfigureText(TMP_Text text, AdventureTheme theme, float size)
        {
            text.font = theme.bodyFont;
            text.color = theme.ink;
            text.enableAutoSizing = true;
            text.fontSizeMin = size * .7f;
            text.fontSizeMax = size;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.margin = Vector4.one * 10;
        }

        internal static void AddCelebration(GameObject target, AdventureTheme theme)
        {
            if (target.GetComponent<ResultCelebration>()) return;
            var celebration = target.AddComponent<ResultCelebration>();
            celebration.pieces = new RectTransform[18];
            for (int i = 0; i < celebration.pieces.Length; i++)
            {
                var piece = new GameObject("Confetti " + (i + 1), typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                piece.SetParent(target.transform, false);
                piece.sizeDelta = new Vector2(16, 28);
                var image = piece.GetComponent<Image>();
                image.color = theme.sections[i % theme.sections.Length];
                image.raycastTarget = false;
                celebration.pieces[i] = piece;
                piece.gameObject.SetActive(false);
            }
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component ? component : target.AddComponent<T>();
        }
    }
}
