using System.IO;
using System.Linq;
using FunMath.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FunMath.Editor
{
    /// <summary>Adds the 3-2-1-Go countdown to timed game scenes. Safe to re-run: scenes that already have it are skipped.</summary>
    public static class GameExtrasBuilder
    {
        [MenuItem("Fun Math/UI/10 - Add game countdown")]
        public static void AddCountdown()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play mode first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>("Assets/UI/Resources/AdventureTheme.asset");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            int added = 0;
            try
            {
                foreach (string path in Directory.GetFiles("Assets/Scenes/Games", "*.unity", SearchOption.AllDirectories).Select(p => p.Replace('\\', '/')))
                {
                    var scene = EditorSceneManager.OpenScene(path);
                    var roots = scene.GetRootGameObjects();
                    var shell = roots.FirstOrDefault(g => g.name == "GameShell");
                    // Only the timed games start their clock from the intro; others have nothing to count down to.
                    if (!shell || roots.SelectMany(g => g.GetComponentsInChildren<TimeGameController>(true)).Count() == 0) continue;
                    if (shell.GetComponentInChildren<GameCountdown>(true)) continue;

                    var root = new GameObject("Countdown", typeof(RectTransform), typeof(CanvasGroup)).GetComponent<RectTransform>();
                    root.SetParent(shell.transform, false);
                    root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.offsetMin = root.offsetMax = Vector2.zero;
                    var dim = root.gameObject.AddComponent<Image>(); dim.color = new Color(.12f, .22f, .28f, .35f); dim.raycastTarget = true;
                    var number = new GameObject("Number", typeof(RectTransform)).GetComponent<RectTransform>();
                    number.SetParent(root, false);
                    number.anchorMin = new Vector2(.2f, .25f); number.anchorMax = new Vector2(.8f, .75f); number.offsetMin = number.offsetMax = Vector2.zero;
                    var text = number.gameObject.AddComponent<TextMeshProUGUI>();
                    text.font = theme.displayFont; text.fontSize = 300; text.fontStyle = FontStyles.Bold;
                    text.enableAutoSizing = true; text.fontSizeMin = 80; text.fontSizeMax = 300;
                    text.alignment = TextAlignmentOptions.Center; text.color = theme.ink; text.raycastTarget = false; // dark ink reads on both the pale game backgrounds and the dim

                    var countdown = root.gameObject.AddComponent<GameCountdown>();
                    countdown.label = text; countdown.theme = theme;
                    // Drawn above the intro/pause dialogs only while it is showing; hidden otherwise.
                    root.SetAsLastSibling();
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    added++;
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Debug.Log("Game countdown added to " + added + " scene(s).");
        }

        [MenuItem("Fun Math/UI/11 - Add game feedback")]
        public static void AddFeedback()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play mode first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>("Assets/UI/Resources/AdventureTheme.asset");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            int added = 0;
            try
            {
                foreach (string path in Directory.GetFiles("Assets/Scenes/Games", "*.unity", SearchOption.AllDirectories).Select(p => p.Replace('\\', '/')))
                {
                    var scene = EditorSceneManager.OpenScene(path);
                    var shell = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "GameShell");
                    var hud = shell ? shell.GetComponent<GameHud>() : null;
                    // Only games with a score/answer event to react to (answers or rocket).
                    if (!hud || (!hud.answers && !hud.rocket) || shell.GetComponentInChildren<GameFeedback>(true)) continue;

                    var safeArea = shell.transform.Find("SafeArea");
                    var root = Rect("Feedback", safeArea ? safeArea : shell.transform, Vector2.zero, Vector2.one);
                    var feedback = root.gameObject.AddComponent<GameFeedback>();
                    feedback.answers = hud.answers; feedback.rocket = hud.rocket; feedback.theme = theme;
                    feedback.origin = shell.GetComponentInChildren<GameChromeLayout>(true).progress;

                    var pop = Rect("Plus", root, new Vector2(.5f, .5f), new Vector2(.5f, .5f));
                    pop.sizeDelta = new Vector2(220, 110);
                    var popText = pop.gameObject.AddComponent<TextMeshProUGUI>();
                    popText.font = theme.displayFont; popText.fontSize = 84; popText.fontStyle = FontStyles.Bold;
                    popText.alignment = TextAlignmentOptions.Center; popText.color = theme.ink; popText.raycastTarget = false;
                    pop.gameObject.SetActive(false);
                    feedback.popLabel = popText;

                    feedback.confetti = new RectTransform[10];
                    for (int i = 0; i < 10; i++)
                    {
                        var piece = Rect("Confetti" + i, root, new Vector2(.5f, .5f), new Vector2(.5f, .5f));
                        piece.sizeDelta = new Vector2(26, 26);
                        var image = piece.gameObject.AddComponent<Image>();
                        image.sprite = theme.roundedPanel; image.type = Image.Type.Simple; image.raycastTarget = false;
                        image.color = theme.sections[i % theme.sections.Length];
                        piece.gameObject.SetActive(false);
                        feedback.confetti[i] = piece;
                    }

                    // Transient owl cheer, centred just above the bottom edge so it does not take permanent space.
                    var cheer = Rect("OwlCheer", root, new Vector2(.5f, 0), new Vector2(.5f, 0));
                    cheer.pivot = new Vector2(.5f, 0); cheer.sizeDelta = new Vector2(520, 150); cheer.anchoredPosition = new Vector2(0, 90);
                    var bubble = cheer.gameObject.AddComponent<Image>();
                    bubble.sprite = theme.roundedPanel; bubble.type = Image.Type.Sliced; bubble.color = theme.cream; bubble.raycastTarget = false;
                    var group = cheer.gameObject.AddComponent<CanvasGroup>(); group.alpha = 0; group.blocksRaycasts = false; group.interactable = false;
                    var owl = Rect("Owl", cheer, new Vector2(0, .5f), new Vector2(0, .5f));
                    owl.pivot = new Vector2(0, .5f); owl.sizeDelta = new Vector2(130, 130); owl.anchoredPosition = new Vector2(14, 0);
                    var owlImage = owl.gameObject.AddComponent<Image>(); owlImage.sprite = theme.owl; owlImage.preserveAspect = true; owlImage.raycastTarget = false;
                    var words = Rect("Words", cheer, Vector2.zero, Vector2.one);
                    words.offsetMin = new Vector2(160, 10); words.offsetMax = new Vector2(-20, -10);
                    var wordsText = words.gameObject.AddComponent<TextMeshProUGUI>();
                    wordsText.font = theme.displayFont; wordsText.fontSize = 56; wordsText.fontStyle = FontStyles.Bold;
                    wordsText.enableAutoSizing = true; wordsText.fontSizeMin = 28; wordsText.fontSizeMax = 56;
                    wordsText.alignment = TextAlignmentOptions.Center; wordsText.color = theme.ink; wordsText.raycastTarget = false;
                    feedback.cheer = group; feedback.cheerLabel = wordsText;

                    // Drawn last so the +1 and confetti sit above the HUD chips (it never takes input).
                    root.SetAsLastSibling();
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    added++;
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Debug.Log("Game feedback added to " + added + " scene(s).");
        }

        /// <summary>
        /// The rocket level was authored for a 1080-unit-wide portrait canvas. On a wider canvas the bubble lanes
        /// stayed in the left third while the rocket started at screen centre and the walls sat at the far edges.
        /// This pins the bubble container and both side walls to a centred 1080-unit column. On a 1080-wide canvas
        /// every position is identical to before, so portrait is unchanged. Safe to re-run.
        /// </summary>
        [MenuItem("Fun Math/UI/12 - Centre rocket playfield")]
        public static void CentreRocketPlayfield()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play mode first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const float authoredWidth = 1080f;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/Games/GamePrasekolah/GamePrasekolah.unity");
                var game = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<GamePraController>(true)).FirstOrDefault();
                if (!game) { Debug.LogWarning("GamePraController not found."); return; }
                var root = game.transform;
                var bubbles = (RectTransform)root.GetChild(4);
                if (Mathf.Approximately(bubbles.anchorMin.x, .5f)) { Debug.Log("Rocket playfield is already centred."); return; }

                // Bubble lanes: a stretched container becomes a centred fixed-width one (width = authored canvas + its inset).
                bubbles.anchorMin = new Vector2(.5f, bubbles.anchorMin.y);
                bubbles.anchorMax = new Vector2(.5f, bubbles.anchorMax.y);
                bubbles.sizeDelta = new Vector2(authoredWidth + bubbles.sizeDelta.x, bubbles.sizeDelta.y);

                // Side walls: re-anchor from the canvas edges to the centre, keeping the portrait position.
                Pin(root.Find("w"), 0f, authoredWidth);
                Pin(root.Find("borderright"), 1f, authoredWidth);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("Rocket playfield centred.");
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        private static void Pin(Transform wall, float oldAnchorX, float authoredWidth)
        {
            if (!wall) return;
            var rect = (RectTransform)wall;
            float shift = (oldAnchorX - .5f) * authoredWidth;
            rect.anchorMin = new Vector2(.5f, rect.anchorMin.y);
            rect.anchorMax = new Vector2(.5f, rect.anchorMax.y);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x + shift, rect.anchoredPosition.y);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }
    }
}
