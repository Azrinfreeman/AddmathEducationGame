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
    /// <summary>
    /// Restyles the quiz header: the old black X and gear icons (which overlapped each other) become the same cream
    /// "Keluar / Exit" and "Ibu bapa / Parents" pills the lesson screens use, side by side. Their actions are untouched:
    /// Exit still opens the original confirmation, and the gear still opens the original audio/profile dropdowns.
    /// Re-runnable.
    /// </summary>
    public static class QuizHeaderBuilder
    {
        private static AdventureTheme theme;

        [MenuItem("Fun Math/UI/16 - Restyle quiz header")]
        public static void Upgrade()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play mode first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>("Assets/UI/Resources/AdventureTheme.asset");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            int done = 0;
            try
            {
                foreach (string path in Directory.GetFiles("Assets/Scenes/Quizzes", "*Quiz.unity", SearchOption.TopDirectoryOnly).Select(p => p.Replace('\\', '/')))
                {
                    var scene = EditorSceneManager.OpenScene(path);
                    var layout = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<QuizScreenLayout>(true)).FirstOrDefault();
                    if (layout == null || layout.exit == null) { Debug.LogWarning("Skipped " + path); continue; }
                    var canvas = layout.transform;
                    var settings = canvas.Find("settingDrop") as RectTransform;

                    Skin(layout.exit.GetComponent<Button>(), "Keluar", "Exit");
                    if (settings)
                    {
                        settings.localScale = Vector3.one;
                        var gear = settings.Find("SettingProfile") as RectTransform;
                        if (gear)
                        {
                            gear.pivot = Vector2.one * .5f;
                            gear.localScale = Vector3.one;
                            QuizPageLayout.Place(gear, 0, 0, 1, 1);
                            Skin(gear.GetComponent<Button>(), "Ibu bapa", "Parents");
                        }
                        // The original gear lives in a duplicate nested settingDrop; hide it, as the lesson screens do.
                        var duplicate = settings.Find("settingDrop");
                        if (duplicate) duplicate.gameObject.SetActive(false);
                        layout.settings = settings;
                    }
                    EditorUtility.SetDirty(layout);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    done++;
                }
            }
            finally { if (setup != null && setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Debug.Log("Quiz header restyled in " + done + " scene(s).");
        }

        private static void Skin(Button button, string bm, string en)
        {
            // Hide the old icon art, keep the Button and its listeners.
            foreach (Transform child in button.transform) if (child.name != "ActionLabel") child.gameObject.SetActive(false);
            var oldLanguage = button.GetComponent<ChangeLanguage>();
            if (oldLanguage) oldLanguage.enabled = false;
            button.transform.localScale = Vector3.one;
            button.transform.localRotation = Quaternion.identity;
            var image = button.GetComponent<Image>();
            if (image == null) image = button.gameObject.AddComponent<Image>();
            button.targetGraphic = image;
            image.raycastTarget = true; image.preserveAspect = false;
            button.colors = ColorBlock.defaultColorBlock;
            image.sprite = theme.roundedPanel; image.type = Image.Type.Sliced;
            var surface = button.GetComponent<ThemedSurface>();
            if (surface == null) surface = button.gameObject.AddComponent<ThemedSurface>();
            surface.theme = theme; surface.section = -1; surface.Apply();
            if (button.GetComponent<KidButton>() == null) button.gameObject.AddComponent<KidButton>().theme = theme;

            var existing = button.transform.Find("ActionLabel");
            TMP_Text label;
            if (existing != null) label = existing.GetComponent<TMP_Text>();
            else
            {
                var rect = new GameObject("ActionLabel", typeof(RectTransform)).GetComponent<RectTransform>();
                rect.SetParent(button.transform, false);
                label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            }
            label.gameObject.SetActive(true);
            label.font = theme.bodyFont; label.color = theme.ink; label.fontStyle = FontStyles.Bold;
            label.enableAutoSizing = true; label.fontSizeMin = 22; label.fontSizeMax = 42;
            label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            var language = label.GetComponent<BilingualLabel>();
            if (language == null) language = label.gameObject.AddComponent<BilingualLabel>();
            language.malay = bm; language.english = en;
            label.text = PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? en : bm;
            QuizPageLayout.Place(label.rectTransform, .04f, .04f, .96f, .96f);
        }
    }
}
