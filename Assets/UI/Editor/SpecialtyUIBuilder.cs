using System;
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
    /// <summary>One-time conversion. Converted shells are retained for manual editing.</summary>
    public static class SpecialtyUIBuilder
    {
        private static AdventureTheme theme;

        [MenuItem("Fun Math/UI/9 - Upgrade badges and credits")]
        public static void Upgrade()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>("Assets/UI/Resources/AdventureTheme.asset");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string path in new[] { "Assets/Scenes/Badges/BadgesPage.unity", "Assets/Scenes/CreditPage.unity", "Assets/Scenes/PenghargaanPage.unity" })
                {
                    var scene = EditorSceneManager.OpenScene(path);
                    if (scene.GetRootGameObjects().Any(g => g.name == "SpecialtyShell")) continue;
                    var original = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Canvas>(true)).First();
                    bool badges = path.Contains("BadgesPage");
                    bool credits = path.EndsWith("/CreditPage.unity");
                    var root = Rect("SpecialtyShell", null, 0, 0, 1, 1);
                    var canvas = root.gameObject.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 100;
                    var scaler = root.gameObject.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080, 1080);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                    root.gameObject.AddComponent<GraphicRaycaster>();
                    Surface(root, -2).sprite = null; // full-screen backdrop: solid fill, no rounded corners
                    var safe = Rect("SafeArea", root, 0, 0, 1, 1);
                    safe.gameObject.AddComponent<SafeAreaPanel>();
                    Label("Heading", safe, badges ? "Lencana saya" : credits ? "Pasukan kita" : "Terima kasih", badges ? "My badges" : credits ? "Our team" : "Thank you", 62, .04f, .86f, .96f, .97f);
                    var back = Button("Back", safe, "Kembali", "Back", .04f, .02f, .4f, .11f);
                    var controller = root.gameObject.AddComponent<SceneController>();
                    UnityEventTools.AddStringPersistentListener(back.onClick, controller.loadScene, "MainMenu");
                    if (badges) BuildBadges(safe, original);
                    else BuildCredits(safe, original, credits);
                    // Preserve all old objects and callbacks, but exclude obsolete quiz UI and settings from display/input.
                    original.gameObject.SetActive(false);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        private static void BuildBadges(RectTransform safe, Canvas original)
        {
            var shelf = safe.gameObject.AddComponent<BadgeShelf>();
            string[] bm = { "Rajin belajar", "Jaguh permainan", "Bintang kuiz", "Penjelajah", "Markah hebat", "Jaguh tambah" };
            string[] en = { "Keen learner", "Game champion", "Quiz star", "Explorer", "High scorer", "Addition champion" };
            string[][] keys = {
                new[]{"Level1Pra_","Level2Pra_","Level3Pra_","Level1Tahap1_","Level2Tahap1_","Level3Tahap1_","Level4Tahap1_","Level1Tahap2_","Level2Tahap2_","Level3Tahap2_"},
                new[]{"GamePra_","GameLevel1_","GameLevel2_"}, new[]{"Kuiz0_","Kuiz1_","Kuiz2_"},
                new[]{"Menu1_","Menu2_","Menu3_","Menu4_","Menu5_","Menu6_"}, new[]{"High1_","High2_","High3_"}, new[]{"PraGameKuiz_"}
            };
            string[] hintsBm = { "Lengkapkan semua pelajaran.", "Lengkapkan tiga kategori permainan.", "Lengkapkan ketiga-tiga kuiz.", "Lawati keenam-enam bahagian menu.", "Capai sasaran markah dalam ketiga-tiga kuiz.", "Lengkapkan cabaran tambah prasekolah." };
            string[] hintsEn = { "Complete every lesson.", "Complete the three game categories.", "Complete all three quizzes.", "Visit all six menu sections.", "Reach the score target in all three quizzes.", "Complete the preschool addition challenge." };
            var grid = Rect("BadgeCards", safe, .04f, .15f, .96f, .83f);
            grid.gameObject.AddComponent<GridLayoutGroup>();
            grid.gameObject.AddComponent<AdaptiveMenu>();
            var old = original.GetComponentInChildren<BadgeController>(true);
            shelf.entries = new BadgeShelf.Entry[6];
            for (int i = 0; i < 6; i++)
            {
                var button = Button("Badge" + i, grid, "", "", 0, 0, 1, 1);
                UnityEngine.Object.DestroyImmediate(button.GetComponentInChildren<TMP_Text>().gameObject);
                Label("Name", button.transform, bm[i], en[i], 36, .04f, .3f, .96f, .52f);
                var status = Label("Progress", button.transform, "", "", 30, .04f, .03f, .96f, .29f);
                UnityEngine.Object.DestroyImmediate(status.GetComponent<BilingualLabel>());
                var icon = Rect("Artwork", button.transform, .25f, .54f, .75f, .96f).gameObject.AddComponent<UnityEngine.UI.Image>();
                icon.sprite = old.transform.GetChild(i).GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite;
                icon.preserveAspect = true; icon.raycastTarget = false;
                shelf.entries[i] = new BadgeShelf.Entry { malayTitle = bm[i], englishTitle = en[i], malayHint = hintsBm[i], englishHint = hintsEn[i], progressKeys = keys[i], button = button, status = status };
            }
            var overlay = Rect("BadgeDetails", safe, 0, 0, 1, 1);
            Surface(overlay, -2).raycastTarget = true;
            var card = Rect("Card", overlay, .08f, .19f, .92f, .81f); Surface(card, -1);
            shelf.detailTitle = Label("Title", card, "", "", 50, .06f, .7f, .94f, .94f);
            shelf.detailBody = Label("Requirement", card, "", "", 38, .08f, .27f, .92f, .68f);
            UnityEngine.Object.DestroyImmediate(shelf.detailTitle.GetComponent<BilingualLabel>());
            UnityEngine.Object.DestroyImmediate(shelf.detailBody.GetComponent<BilingualLabel>());
            var close = Button("Close", card, "Kembali", "Back", .15f, .05f, .85f, .22f);
            UnityEventTools.AddPersistentListener(close.onClick, shelf.Close);
            overlay.gameObject.AddComponent<CanvasGroup>();
            overlay.gameObject.AddComponent<GameModalFocus>().firstAction = close;
            shelf.details = overlay.gameObject;
            overlay.gameObject.SetActive(false);
        }

        private static void BuildCredits(RectTransform safe, Canvas original, bool team)
        {
            var viewport = Rect("ReadingArea", safe, .05f, .15f, .95f, .83f);
            Surface(viewport, -1).raycastTarget = true;
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 45;
            var content = Rect("Content", viewport, 0, 1, 1, 1);
            content.pivot = new Vector2(.5f, 1);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 24, 24); layout.spacing = 24;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = content;
            var source = team ? original.transform.Find("QuizController/1") : original.transform.Find("rect/body");
            foreach (var text in source.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.name == "BI") continue;
                // The copied quiz container also holds timer-choice labels ("15 MINIT"); they are not credits.
                if (text.text.Trim().EndsWith("MINIT", StringComparison.OrdinalIgnoreCase) && text.text.Trim().Length < 12) continue;
                string english = text.text;
                if (text.name == "BM" && text.transform.parent.Find("BI"))
                    english = text.transform.parent.Find("BI").GetComponent<TMP_Text>().text;
                var label = Label("Credit", content, text.text, english, 36, 0, 0, 1, 1);
                label.font = theme.bodyFont; label.enableAutoSizing = false;
                label.gameObject.AddComponent<LayoutElement>().preferredHeight = Mathf.Max(110, 55 * Mathf.Max(text.text.Split('\n').Length, english.Split('\n').Length));
            }
            Label("ScrollHint", safe, "Leret untuk membaca", "Scroll to read", 28, .42f, .025f, .96f, .105f);
        }

        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }

        private static UnityEngine.UI.Image Surface(RectTransform rect, int section)
        {
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = theme.roundedPanel; image.type = UnityEngine.UI.Image.Type.Sliced; image.raycastTarget = false;
            var surface = rect.gameObject.AddComponent<ThemedSurface>(); surface.theme = theme; surface.section = section; surface.Apply(); return image;
        }

        private static TMP_Text Label(string name, Transform parent, string bm, string en, float size, float x0, float y0, float x1, float y1)
        {
            var label = Rect(name, parent, x0, y0, x1, y1).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = theme.displayFont; label.text = bm; label.fontSize = size; label.color = theme.ink;
            label.enableAutoSizing = true; label.fontSizeMin = 26; label.fontSizeMax = size;
            label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            var bilingual = label.gameObject.AddComponent<BilingualLabel>(); bilingual.malay = bm; bilingual.english = en; return label;
        }

        private static UnityEngine.UI.Button Button(string name, Transform parent, string bm, string en, float x0, float y0, float x1, float y1)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1); var image = Surface(rect, 3); image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image;
            rect.gameObject.AddComponent<KidButton>().theme = theme;
            Label("Label", rect, bm, en, 40, .06f, .08f, .94f, .92f); return button;
        }
    }
}
