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
    /// <summary>
    /// Adds a Listen button to every lesson scene. It reads the current page aloud with the device's own Malay or
    /// English voice (see LessonVoice). Re-runnable: scenes that already have a LessonVoice are skipped.
    /// </summary>
    public static class LessonVoiceBuilder
    {
        private static AdventureTheme theme;

        [MenuItem("Fun Math/UI/14 - Add lesson voice-over")]
        public static void Upgrade()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play mode first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            theme = AssetDatabase.LoadAssetAtPath<AdventureTheme>("Assets/UI/Resources/AdventureTheme.asset");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            int done = 0;
            try
            {
                foreach (string path in Directory.GetFiles("Assets/Scenes/Lessons", "*.unity", SearchOption.AllDirectories).Select(p => p.Replace('\\', '/')))
                {
                    var scene = EditorSceneManager.OpenScene(path);
                    var layout = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<LessonScreenLayout>(true)).FirstOrDefault();
                    if (layout == null || layout.pages == null) continue; // hubs have no lesson pages
                    if (scene.GetRootGameObjects().Any(g => g.GetComponentInChildren<LessonVoice>(true) != null)) continue;
                    var canvas = layout.transform;

                    var button = Button("ListenButton", canvas, "Dengar", "Listen", out var label);
                    var message = Rect("VoiceMessage", canvas);
                    var bubble = message.gameObject.AddComponent<Image>();
                    bubble.sprite = theme.roundedPanel; bubble.type = Image.Type.Sliced; bubble.color = theme.cream; bubble.raycastTarget = false;
                    var text = Label("Text", message, "", "", 36);
                    Place((RectTransform)text.transform, .04f, .1f, .96f, .9f);
                    message.gameObject.SetActive(false);

                    var voice = canvas.gameObject.AddComponent<LessonVoice>();
                    voice.pages = layout.pages; voice.listenButton = button; voice.message = text; voice.messageRoot = message.gameObject; voice.listenLabel = label;
                    UnityEventTools.AddPersistentListener(button.onClick, voice.Listen);

                    layout.listen = (RectTransform)button.transform; layout.voiceMessage = message;
                    EditorUtility.SetDirty(layout);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    done++;
                }
            }
            finally { if (setup != null && setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Debug.Log("Lesson voice-over added to " + done + " scene(s).");
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static void Place(RectTransform rect, float x0, float y0, float x1, float y1)
        {
            rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1); rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static TMP_Text Label(string name, Transform parent, string bm, string en, float size)
        {
            var label = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = theme.displayFont; label.text = bm; label.fontSize = size; label.color = theme.ink;
            label.enableAutoSizing = true; label.fontSizeMin = 22; label.fontSizeMax = size;
            label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            if (!string.IsNullOrEmpty(en)) { var b = label.gameObject.AddComponent<BilingualLabel>(); b.malay = bm; b.english = en; }
            return label;
        }

        private static Button Button(string name, Transform parent, string bm, string en, out TMP_Text label)
        {
            var rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = theme.roundedPanel; image.type = Image.Type.Sliced; image.raycastTarget = true;
            var surface = rect.gameObject.AddComponent<ThemedSurface>(); surface.theme = theme; surface.section = 2; surface.Apply();
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            rect.gameObject.AddComponent<KidButton>().theme = theme;
            label = Label("ActionLabel", rect, bm, en, 42);
            Place((RectTransform)label.transform, .04f, .04f, .96f, .96f);
            return button;
        }
    }
}
