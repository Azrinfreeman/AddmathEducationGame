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

namespace FunMath.Editor
{
    /// <summary>Converts untouched games once. Converted scenes remain manually editable.</summary>
    public static class GameUIBuilder
    {
        private static AdventureTheme theme;

        [MenuItem("Fun Math/UI/8 - Upgrade gameplay HUDs and dialogs")]
        public static void Upgrade()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the open scene first.");
            theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>("Assets/UI/Resources/AdventureTheme.asset");
            if (!theme) throw new InvalidOperationException("Build the shared adventure assets first.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var audit = new List<string> { "scene\tconverted\tcallbacksBefore\tcallbacksAfter\tmissingAvailableBefore\tmissingAvailableAfter\tcolliderGeometryPreserved" };
            try
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Games" }))
                {
                    var scene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid));
                    var canvas = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>(true)).First(c => c.name == "Canvas");
                    int before = CallbackCount(canvas);
                    int missingBefore = MissingAvailable(canvas);
                    string geometry = ColliderGeometry(canvas);
                    bool converted = !scene.GetRootGameObjects().Any(g => g.name == "GameShell");
                    if (converted)
                    {
                        Build(canvas, scene);
                        EditorSceneManager.SaveScene(scene);
                    }
                    audit.Add(string.Join("\t", scene.path, converted, before, CallbackCount(canvas), missingBefore,
                        MissingAvailable(canvas), geometry == ColliderGeometry(canvas)));
                }
                Directory.CreateDirectory("docs/tutorial");
                File.WriteAllLines("docs/tutorial/game-ui-audit.tsv", audit);
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        private static int CallbackCount(Canvas canvas) => canvas.GetComponentsInChildren<Button>(true).Sum(b => b.onClick.GetPersistentEventCount());

        private static int MissingAvailable(Canvas canvas)
        {
            return canvas.GetComponentsInChildren<Button>(true).Where(b =>
            {
                for (var t = b.transform; t && t != canvas.transform; t = t.parent)
                    if (!t.gameObject.activeSelf && t.name != "Panel" && !t.GetComponent<Animator>()) return false;
                return true;
            }).Sum(b => Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Count(i => !b.onClick.GetPersistentTarget(i)));
        }

        private static string ColliderGeometry(Canvas canvas)
        {
            return string.Join("|", canvas.GetComponentsInChildren<Collider2D>(true).Select(c =>
                AnimationUtility.CalculateTransformPath(c.transform, canvas.transform) + ":" + c.transform.localPosition + ":" +
                c.transform.localScale + ":" + ((RectTransform)c.transform).sizeDelta + ":" + c.offset));
        }

        private static void Build(Canvas original, Scene scene)
        {
            var answers = original.GetComponentInChildren<Game1Controller>(true);
            var rocket = original.GetComponent<GamePraController>();
            var scales = original.GetComponentInChildren<ScaleCounterController>(true);
            var timer = original.GetComponentInChildren<TimeGameController>(true);
            bool algorithm = original.GetComponentInChildren<AlgoController>(true);
            string bm = answers ? "Cari jawapan" : rocket ? "Roket nombor" : scales ? "Imbangkan bola" : "Jejak bentuk";
            string en = answers ? "Find the answer" : rocket ? "Number rocket" : scales ? "Balance the balls" : "Shape trail";
            string hintBm = answers ? "Tekan jawapan yang betul sebelum masa tamat." : rocket ? "Tahan Kiri atau Kanan. Bawa roket ke jawapan yang betul." :
                scales ? "Tarik bola ke dua bekas. Jadikan jumlahnya sama." : "Tarik anak panah ke petak. Tekan Jalan untuk mengikut laluan.";
            string hintEn = answers ? "Tap the correct answers before time runs out." : rocket ? "Hold Left or Right. Move the rocket to the correct answer." :
                scales ? "Drag the balls into both baskets. Make their totals equal." : "Drag arrows into the slots. Choose Run to follow your route.";

            // Keep original callback objects and all physics-driven parent/child positions.
            foreach (var home in original.GetComponentsInChildren<Button>(true).Where(b => b.name == "homeButton")) home.gameObject.SetActive(false);
            foreach (var settings in original.GetComponentsInChildren<ButtonSettingAnimation>(true)) settings.gameObject.SetActive(false);
            var header = original.transform.Find("Header");
            if (header)
                foreach (var label in header.GetComponentsInChildren<TMP_Text>(true).Where(t => t.name.StartsWith("title", StringComparison.Ordinal))) label.enabled = false;
            if (timer)
            {
                timer.prompted = original.transform.Find("againPanel")?.GetComponent<Animator>();
                timer.timeoutGameobject = original.transform.Find("TimeUp");
                MarkEdited(timer);
                foreach (var graphic in timer.GetComponentsInChildren<Graphic>(true)) graphic.enabled = false;
                if (timer.timeoutGameobject)
                    foreach (var score in timer.timeoutGameobject.GetComponentsInChildren<LoadScore>(true)) score.enabled = false;
            }
            if (answers) ConfigureAnswerBoard(answers);

            var shell = new GameObject("GameShell", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(shell, scene);
            var canvas = shell.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = shell.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var safe = Rect("SafeArea", shell.transform);
            safe.gameObject.AddComponent<SafeAreaPanel>();
            var layout = safe.gameObject.AddComponent<GameChromeLayout>();
            var pause = shell.AddComponent<GamePause>();
            var pauseButton = Button("Pause", safe, "Rehat", "Pause", -1);
            pause.pauseControl = pauseButton;
            UnityEventTools.AddPersistentListener(pauseButton.onClick, pause.Pause);
            layout.pause = (RectTransform)pauseButton.transform;
            layout.heading = Label("GameTitle", safe, bm, en, 58).rectTransform;
            if (!algorithm)
            {
                var chip = Rect("Progress", safe);
                Surface(chip.gameObject, -1, false);
                var caption = Label("Caption", chip, answers ? "Dijumpai" : rocket ? "Soalan" : "Bola", answers ? "Found" : rocket ? "Question" : "Balls", 28);
                QuizPageLayout.Place(caption.rectTransform, .05f, .56f, .95f, .96f);
                var value = Label("Value", chip, "", "", 46);
                UnityEngine.Object.DestroyImmediate(value.GetComponent<BilingualLabel>());
                QuizPageLayout.Place(value.rectTransform, .05f, .02f, .95f, .62f);
                layout.progress = chip;
                var hud = shell.AddComponent<GameHud>();
                hud.answers = answers; hud.rocket = rocket; hud.timer = timer; hud.scales = scales; hud.progress = value;
                if (timer || rocket)
                {
                    var second = Rect("Secondary", safe);
                    Surface(second.gameObject, timer ? 0 : 3, false);
                    var caption2 = Label("Caption", second, timer ? "Masa" : "Markah", timer ? "Time" : "Score", 28);
                    QuizPageLayout.Place(caption2.rectTransform, .05f, .56f, .95f, .96f);
                    var value2 = Label("Value", second, "", "", 46);
                    UnityEngine.Object.DestroyImmediate(value2.GetComponent<BilingualLabel>());
                    QuizPageLayout.Place(value2.rectTransform, .05f, .02f, .95f, .62f);
                    hud.secondary = value2;
                    layout.secondary = second;
                }
            }
            layout.hint = Label("GameHint", safe, hintBm, hintEn, 32).rectTransform;
            if (algorithm) ConfigureAlgorithmActions(original, safe, layout);
            if (rocket) original.transform.Find("Question").gameObject.AddComponent<RocketQuestionLayout>();
            if (scales) ConfigureBalanceExtras(original, safe, layout);
            var overlay = Rect("PauseOverlay", shell.transform);
            var shade = overlay.gameObject.AddComponent<Image>();
            shade.color = new Color(0.08f, .15f, .18f, .78f);
            var card = Rect("PauseCard", overlay);
            ConfigureBody(card);
            LabelAt("Title", card, "Mari berehat", "Take a break", 64, .06f, .82f, .94f, .96f);
            LabelAt("Instructions", card, hintBm, hintEn, 40, .08f, .65f, .92f, .79f);
            var resume = Button("Resume", card, "Sambung", "Resume", 1);
            var replay = Button("Replay", card, "Mula semula", "Start again", 0);
            var menu = Button("Games", card, "Permainan", "Games", -1);
            QuizPageLayout.Place((RectTransform)resume.transform, .08f, .44f, .92f, .62f);
            QuizPageLayout.Place((RectTransform)replay.transform, .08f, .24f, .92f, .42f);
            QuizPageLayout.Place((RectTransform)menu.transform, .08f, .04f, .92f, .22f);
            UnityEventTools.AddPersistentListener(resume.onClick, pause.Resume);
            UnityEventTools.AddPersistentListener(replay.onClick, pause.Replay);
            UnityEventTools.AddPersistentListener(menu.onClick, pause.BackToGames);
            card.GetComponent<GameModalFocus>().firstAction = resume;
            pause.pausePanel = overlay.gameObject;
            overlay.gameObject.SetActive(false);

            if (rocket)
            {
                var directions = original.GetComponentInChildren<ButtonController>(true);
                foreach (bool right in new[] { false, true })
                {
                    string name = right ? "RightArrow" : "LeftArrow";
                    var source = directions.transform.Find(name).GetComponent<Button>();
                    source.gameObject.SetActive(false);
                    var control = Button(name, safe, right ? "Kanan >" : "< Kiri", right ? "Right >" : "< Left", 2);
                    var bridge = control.gameObject.AddComponent<RocketDirectionButton>();
                    bridge.directions = directions; bridge.right = right;
                    if (right) layout.right = (RectTransform)control.transform; else layout.left = (RectTransform)control.transform;
                }
                overlay.SetAsLastSibling();
            }

            foreach (Transform root in original.transform)
            {
                if (!root.GetComponent<Animator>()) continue;
                bool intro = root.name.StartsWith("IntroPanel", StringComparison.Ordinal) || root.name == "HowToPlay";
                bool win = root.name.Equals("winPanel", StringComparison.OrdinalIgnoreCase) || root.name == "EndPanel";
                bool retry = root.name == "failPanel" || root.name == "againPanel" || root.name == "LosePanel";
                bool loading = root.name == "Loading";
                if (!intro && !win && !retry && !loading) continue;
                if (win) QuizUIBuilder.AddCelebration(root.gameObject, theme);
                var body = root.Find("Panel/AudioPanel") ?? root.Find("Image");
                if (!body) continue;
                root.localScale = Vector3.one;
                QuizPageLayout.Place((RectTransform)root, 0, 0, 1, 1);
                MarkEdited(root);
                var actions = body.Cast<Transform>().Select(t => t.GetComponent<Button>()).Where(b => b && b.gameObject.activeSelf &&
                    Enumerable.Range(0, b.onClick.GetPersistentEventCount()).Any(i => b.onClick.GetPersistentTarget(i) &&
                        (b.onClick.GetPersistentMethodName(i) == "loadScene" || b.onClick.GetPersistentMethodName(i) == "StartTime" ||
                         b.onClick.GetPersistentMethodName(i) == "isHiding" || b.onClick.GetPersistentMethodName(i) == "SetActive"))).ToArray();
                var dynamicScore = body.GetComponentsInChildren<loadLevel>(true).FirstOrDefault();
                foreach (Transform child in body) child.gameObject.SetActive(false);
                ConfigureBody((RectTransform)body);
                var modalCanvas = root.gameObject.AddComponent<Canvas>();
                modalCanvas.overrideSorting = true; modalCanvas.sortingOrder = 200;
                root.gameObject.AddComponent<GraphicRaycaster>();
                var background = root.Find("Panel") ?? root.Find("Prompt");
                if (background && background.GetComponent<Image>())
                {
                    background.GetComponent<Image>().color = new Color(.08f, .15f, .18f, .78f);
                    background.GetComponent<Image>().raycastTarget = true;
                }
                LabelAt("GameDialogTitle", body, intro ? "Cara bermain" : win ? "Tahniah!" : loading ? "Ikut laluan..." : "Cuba lagi!",
                    intro ? "How to play" : win ? "Well done!" : loading ? "Following your route..." : "Try again!", 64, .06f, .78f, .94f, .94f);
                var owl = Rect("OwlGuide", body);
                QuizPageLayout.Place(owl, .35f, .52f, .65f, .77f);
                var owlImage = owl.gameObject.AddComponent<Image>();
                owlImage.sprite = theme.owl; owlImage.preserveAspect = true; owlImage.raycastTarget = false;
                string detailBm = intro ? hintBm : win ? "Usaha yang hebat. Mari terus belajar!" : loading ? "Lihat pemain mengikut arahan kamu." :
                    algorithm ? "Ubah laluan kamu dan cuba sekali lagi." : scales ? "Cuba agihkan bola supaya jumlahnya sama." : "Kamu boleh cuba sekali lagi.";
                string detailEn = intro ? hintEn : win ? "Great effort. Let's keep learning!" : loading ? "Watch your player follow the arrows." :
                    algorithm ? "Change your route and try once more." : scales ? "Try sharing the balls into equal totals." : "You can have another go.";
                LabelAt("GameDialogHint", body, detailBm, detailEn, 40, .07f, dynamicScore ? .25f : .27f, .93f, dynamicScore ? .38f : .50f);
                if (dynamicScore)
                {
                    dynamicScore.gameObject.SetActive(true);
                    Style(dynamicScore.GetComponent<TMP_Text>(), 46);
                    QuizPageLayout.Place((RectTransform)dynamicScore.transform, .08f, .39f, .92f, .50f);
                }
                for (int i = 0; i < actions.Length; i++)
                {
                    var action = actions[i];
                    RemoveMissingCalls(action);
                    action.gameObject.SetActive(true);
                    string destination = Destination(action);
                    if (!string.IsNullOrEmpty(destination) && !EditorBuildSettings.scenes.Any(s => s.enabled && string.Equals(Path.GetFileNameWithoutExtension(s.path), destination, StringComparison.OrdinalIgnoreCase)))
                    {
                        // The balance prototype has no Level2 scene. Return to its game menu.
                        var controller = original.GetComponent<SceneController>();
                        for (int call = 0; call < action.onClick.GetPersistentEventCount(); call++)
                            if (action.onClick.GetPersistentMethodName(call) == "loadScene")
                                UnityEventTools.RegisterStringPersistentListener(action.onClick, call, controller.loadScene, "GameMenu");
                        destination = "GameMenu";
                    }
                    Skin(action, intro ? "Mari main!" : destination == "GameMenu" ? "Permainan" : destination == scene.name ? "Mula semula" : "Seterusnya",
                        intro ? "Let's play!" : destination == "GameMenu" ? "Games" : destination == scene.name ? "Start again" : "Next", i == 0 ? 1 : -1);
                    MarkEdited(action);
                    float width = .84f / Mathf.Max(1, actions.Length);
                    QuizPageLayout.Place((RectTransform)action.transform, .08f + i * width, .05f, .08f + (i + 1) * width - .02f, .23f);
                }
                body.GetComponent<GameModalFocus>().firstAction = actions.FirstOrDefault();
            }
        }

        public static void ConfigureAlgorithmActions(Canvas original, Transform safe, GameChromeLayout layout)
        {
            if (!theme) theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>("Assets/UI/Resources/AdventureTheme.asset");
            var algorithm = original.GetComponentInChildren<AlgoController>(true);
            foreach (bool run in new[] { true, false })
            {
                var source = algorithm.transform.Find(run ? "executeButton" : "ResetB").GetComponent<Button>();
                foreach (var graphic in source.GetComponentsInChildren<Graphic>(true)) graphic.enabled = false;
                var navigation = source.navigation; navigation.mode = Navigation.Mode.None; source.navigation = navigation;
                var button = Button(run ? "Run" : "ClearRoute", safe, run ? "Jalan" : "Padam", run ? "Run" : "Clear", run ? 1 : -1);
                button.onClick = source.onClick;
                button.gameObject.AddComponent<NavigationState>().source = source;
                if (run) layout.primaryAction = (RectTransform)button.transform; else layout.secondaryAction = (RectTransform)button.transform;
            }
        }

        public static void ConfigureBalanceExtras(Canvas original, Transform safe, GameChromeLayout layout)
        {
            if (!theme) theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>("Assets/UI/Resources/AdventureTheme.asset");
            var title = original.transform.Find("body/Title");
            if (title) title.gameObject.SetActive(false);
            var oldInfo = original.transform.Find("infoButton");
            if (oldInfo) oldInfo.gameObject.SetActive(false);
            var help = Button("Help", safe, "Cara main", "How to play", -1);
            UnityEventTools.AddBoolPersistentListener(help.onClick, original.transform.Find("HowToPlay").gameObject.SetActive, true);
            layout.help = (RectTransform)help.transform;
        }

        public static void ConfigureAnswerBoard(Game1Controller answers)
        {
            if (!theme) theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>("Assets/UI/Resources/AdventureTheme.asset");
            var layout = answers.GetComponent<GameAnswerBoardLayout>() ?? answers.gameObject.AddComponent<GameAnswerBoardLayout>();
            layout.targets = answers.transform.Cast<Transform>().Where(t => t.GetComponentInChildren<ButtonChoiceController>(true)).Select(t => (RectTransform)t).ToArray();
            layout.answerLabels = layout.targets.SelectMany(t => t.GetComponentsInChildren<TMP_Text>(true)).ToArray();
            layout.answerInk = theme.ink;
            layout.question = (RectTransform)answers.transform.Find("Questions");
            if (!layout.question) return;
            layout.captions = layout.question.Cast<Transform>().Where(t => t.GetComponent<TMP_Text>()).Select(t => (RectTransform)t).ToArray();
            var image = layout.question.GetComponentInChildren<Image>(true);
            layout.illustration = image ? image.rectTransform : null;
            if (image) { image.preserveAspect = true; image.raycastTarget = false; image.gameObject.SetActive(true); }
            var language = layout.question.GetComponent<ChangeLanguage>();
            if (language && layout.captions.Length == 2)
            {
                language.malay = layout.captions[0]; language.english = layout.captions[1];
                MarkEdited(language);
            }
            foreach (var text in answers.GetComponentsInChildren<TMP_Text>(true)) Style(text, 52);
        }

        private static void RemoveMissingCalls(Button button)
        {
            for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                if (!button.onClick.GetPersistentTarget(i)) UnityEventTools.RemovePersistentListener(button.onClick, i);
        }

        private static string Destination(Button button)
        {
            var serialized = new SerializedObject(button);
            var calls = serialized.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            for (int i = 0; i < calls.arraySize; i++)
            {
                var call = calls.GetArrayElementAtIndex(i);
                if (call.FindPropertyRelative("m_MethodName").stringValue == "loadScene")
                    return call.FindPropertyRelative("m_Arguments.m_StringArgument").stringValue;
            }
            return "";
        }

        private static void ConfigureBody(RectTransform body)
        {
            body.anchorMin = body.anchorMax = body.pivot = Vector2.one * .5f;
            body.anchoredPosition = Vector2.zero;
            body.sizeDelta = Vector2.one * 900;
            body.localScale = Vector3.one;
            Surface(body.gameObject, -1, true);
            var fit = body.GetComponent<ModalBodyFit>() ?? body.gameObject.AddComponent<ModalBodyFit>();
            fit.screenFraction = .84f;
            fit.respectSafeArea = true;
            if (!body.GetComponent<GameModalFocus>()) body.gameObject.AddComponent<GameModalFocus>();
            MarkEdited(body);
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            QuizPageLayout.Place(rect, 0, 0, 1, 1);
            return rect;
        }

        private static void Surface(GameObject target, int section, bool raycast)
        {
            var image = target.GetComponent<Image>() ?? target.AddComponent<Image>();
            image.sprite = theme.roundedPanel; image.type = Image.Type.Sliced; image.raycastTarget = raycast;
            var surface = target.GetComponent<ThemedSurface>() ?? target.AddComponent<ThemedSurface>();
            surface.theme = theme; surface.section = section; surface.Apply();
            MarkEdited(image);
        }

        private static void Style(TMP_Text label, float size)
        {
            label.font = theme.displayFont; label.fontSize = size;
            label.fontStyle = FontStyles.Bold;
            label.fontSizeMin = Mathf.Min(28, size); label.fontSizeMax = size; label.enableAutoSizing = true;
            label.color = theme.ink; label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
        }

        private static TMP_Text Label(string name, Transform parent, string bm, string en, float size)
        {
            var label = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            Style(label, size);
            var language = label.gameObject.AddComponent<BilingualLabel>();
            language.malay = bm; language.english = en;
            label.text = PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? en : bm;
            return label;
        }

        private static void LabelAt(string name, Transform parent, string bm, string en, float size, float x0, float y0, float x1, float y1)
        {
            QuizPageLayout.Place(Label(name, parent, bm, en, size).rectTransform, x0, y0, x1, y1);
        }

        private static Button Button(string name, Transform parent, string bm, string en, int section)
        {
            var button = Rect(name, parent).gameObject.AddComponent<Button>();
            Skin(button, bm, en, section);
            return button;
        }

        private static void Skin(Button button, string bm, string en, int section)
        {
            foreach (Transform child in button.transform) child.gameObject.SetActive(false);
            Surface(button.gameObject, section, true);
            button.transition = Selectable.Transition.ColorTint;
            button.colors = ColorBlock.defaultColorBlock;
            button.targetGraphic = button.GetComponent<Image>();
            button.image.overrideSprite = null;
            var feedback = button.GetComponent<KidButton>() ?? button.gameObject.AddComponent<KidButton>();
            feedback.theme = theme;
            var outline = button.GetComponent<Outline>() ?? button.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(.12f, .22f, .28f, .45f);
            outline.effectDistance = new Vector2(2, -3);
            var label = Label("ActionLabel", button.transform, bm, en, 46);
            QuizPageLayout.Place(label.rectTransform, .04f, .04f, .96f, .96f);
            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.Automatic;
            button.navigation = navigation;
            MarkEdited(button);
        }

        private static void MarkEdited(UnityEngine.Object target)
        {
            EditorUtility.SetDirty(target);
            if (PrefabUtility.IsPartOfPrefabInstance(target)) PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }
    }
}
