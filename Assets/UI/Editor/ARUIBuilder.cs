using System.IO;
using System.Linq;
using FunMath.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using Image = UnityEngine.UI.Image;

namespace FunMath.Editor
{
    /// <summary>
    /// Adds the friendly AR overlay (back pill, owl hint, scan frame, "card found" message) to every AR scene.
    /// The scene's own logic is untouched: tracking, the video surface, Play/Stop and the exit dialog keep working.
    /// Only the old grey X and the "SCAN KAD" backdrop/text are hidden. Scenes that already have an ARShell are skipped.
    /// </summary>
    public static class ARUIBuilder
    {
        private static AdventureTheme theme;

        [MenuItem("Fun Math/UI/13 - Upgrade AR screens")]
        public static void Upgrade()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play mode first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>("Assets/UI/Resources/AdventureTheme.asset");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            int done = 0;
            try
            {
                foreach (string path in Directory.GetFiles("Assets/Scenes/AR", "*.unity", SearchOption.AllDirectories).Select(p => p.Replace('\\', '/')))
                {
                    var scene = EditorSceneManager.OpenScene(path);
                    var roots = scene.GetRootGameObjects();
                    if (roots.Any(g => g.name == "ARShell")) continue;
                    var legacy = roots.FirstOrDefault(g => g.name == "Canvas");
                    var imageTarget = roots.SelectMany(g => g.GetComponentsInChildren<ImageTargetBehaviour>(true)).FirstOrDefault();
                    if (!legacy || !imageTarget) { Debug.LogWarning("Skipped " + path + ": no legacy Canvas or image target."); continue; }

                    var exit = FindDeep(legacy.transform, "exit");
                    var confirm = FindDeep(legacy.transform, "IntroPanelParent (1)");
                    var scanGroup = FindDeep(legacy.transform, "1");
                    var exitButton = exit ? exit.GetComponent<Button>() : null;
                    if (!exitButton) { Debug.LogWarning("Skipped " + path + ": no exit button."); continue; }

                    var shell = Rect("ARShell", null, Vector2.zero, Vector2.one);
                    var canvas = shell.gameObject.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 90;
                    var scaler = shell.gameObject.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080, 1080); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                    shell.gameObject.AddComponent<GraphicRaycaster>();
                    var safe = Rect("SafeArea", shell, Vector2.zero, Vector2.one);
                    safe.gameObject.AddComponent<SafeAreaPanel>();
                    var layout = safe.gameObject.AddComponent<ARChromeLayout>();

                    // Back pill: presses the scene's original exit button (same confirmation dialog).
                    var back = Button("Back", safe, "Kembali", "Back");
                    var backLogic = safe.gameObject.AddComponent<ARBackButton>();
                    backLogic.legacyExit = exitButton; backLogic.confirmDialog = confirm ? confirm.gameObject : null;
                    backLogic.overlay = safe.gameObject.AddComponent<CanvasGroup>();
                    UnityEventTools.AddPersistentListener(back.onClick, backLogic.Press);
                    layout.back = (RectTransform)back.transform;

                    // Scan hint: owl and a short instruction, shown while the card is not in view.
                    var hint = Rect("Hint", safe, Vector2.zero, Vector2.one);
                    Surface(hint, -1);
                    var hintGroup = hint.gameObject.AddComponent<CanvasGroup>(); hintGroup.blocksRaycasts = false; hintGroup.interactable = false;
                    var owl = Rect("Owl", hint, new Vector2(.02f, .08f), new Vector2(.22f, .92f));
                    var owlImage = owl.gameObject.AddComponent<Image>(); owlImage.sprite = theme.owl; owlImage.preserveAspect = true; owlImage.raycastTarget = false;
                    Label("Words", hint, "Halakan kamera pada kad", "Point the camera at the card", 44, new Vector2(.24f, .06f), new Vector2(.97f, .94f));
                    layout.hint = hint;

                    // Scan frame: four yellow corner brackets that breathe gently.
                    var frame = Rect("ScanFrame", safe, Vector2.zero, Vector2.one);
                    foreach (var corner in new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) })
                    {
                        Bar(frame, corner, new Vector2(80, 14));
                        Bar(frame, corner, new Vector2(14, 80));
                    }
                    layout.frame = frame;

                    // Card found: a short green message with a pop.
                    var found = Rect("Found", safe, Vector2.zero, Vector2.one);
                    Surface(found, 1);
                    var foundGroup = found.gameObject.AddComponent<CanvasGroup>(); foundGroup.alpha = 0; foundGroup.blocksRaycasts = false; foundGroup.interactable = false;
                    Label("Words", found, "Kad dijumpai!", "Card found!", 56, new Vector2(.04f, .06f), new Vector2(.96f, .94f));
                    layout.found = found;

                    var guide = shell.gameObject.AddComponent<ARScanGuide>();
                    guide.target = imageTarget; guide.theme = theme; guide.hint = hintGroup; guide.frame = frame; guide.found = foundGroup;

                    // Hide only what the overlay replaces. Objects stay active so the scene's own logic is unchanged.
                    if (exit) foreach (var g in exit.GetComponentsInChildren<Graphic>(true)) { g.enabled = false; g.raycastTarget = false; }
                    if (scanGroup)
                    {
                        var backdrop = scanGroup.Find("Panel"); if (backdrop && backdrop.GetComponent<Graphic>()) backdrop.GetComponent<Graphic>().enabled = false;
                        foreach (var text in scanGroup.GetComponentsInChildren<TMP_Text>(true)) text.enabled = false; // the old "SCAN KAD"
                    }

                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    done++;
                }
            }
            finally
            {
                // An untitled scene gives an empty setup, which Unity refuses to restore.
                if (setup != null && setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
            Debug.Log("AR overlay added to " + done + " scene(s).");
        }

        private static Transform FindDeep(Transform root, string name)
        {
            foreach (Transform child in root)
            {
                if (child.name == name) return child;
                var found = FindDeep(child, name);
                if (found) return found;
            }
            return null;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }

        private static Image Surface(RectTransform rect, int section)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = theme.roundedPanel; image.type = Image.Type.Sliced; image.raycastTarget = false;
            var surface = rect.gameObject.AddComponent<ThemedSurface>(); surface.theme = theme; surface.section = section; surface.Apply(); return image;
        }

        private static void Bar(RectTransform frame, Vector2 corner, Vector2 size)
        {
            var bar = Rect("Corner", frame, corner, corner);
            bar.pivot = corner; bar.sizeDelta = size;
            var image = bar.gameObject.AddComponent<Image>();
            image.sprite = theme.roundedPanel; image.type = Image.Type.Simple; image.raycastTarget = false;
            image.color = theme.sections[Mathf.Min(3, theme.sections.Length - 1)];
        }

        private static TMP_Text Label(string name, Transform parent, string bm, string en, float size, Vector2 min, Vector2 max)
        {
            var label = Rect(name, parent, min, max).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = theme.displayFont; label.text = bm; label.fontSize = size; label.color = theme.ink;
            label.enableAutoSizing = true; label.fontSizeMin = 24; label.fontSizeMax = size;
            label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            var bilingual = label.gameObject.AddComponent<BilingualLabel>(); bilingual.malay = bm; bilingual.english = en; return label;
        }

        private static Button Button(string name, Transform parent, string bm, string en)
        {
            var rect = Rect(name, parent, Vector2.zero, Vector2.one); var image = Surface(rect, 3); image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            rect.gameObject.AddComponent<KidButton>().theme = theme;
            Label("Label", rect, bm, en, 44, new Vector2(.06f, .08f), new Vector2(.94f, .92f)); return button;
        }
    }
}
