using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    /// <summary>
    /// Reads the current lesson page aloud in the selected language (Malay or English) for children who cannot read
    /// yet. It speaks only the page's real content, in reading order: button labels, the progress counter and the
    /// "choose Try AR" hint are skipped. Speech stops when the page changes or the scene closes. The button is hidden
    /// when a parent switches voice guidance off in the parent settings (<c>UI.Voice</c>).
    ///
    /// Each page can have a bundled recording per language (<see cref="clips"/>), so the voice works offline and sounds
    /// the same on every phone. A recording is only used while it still matches the page's current text (checked with a
    /// fingerprint); if the text was edited, or no recording exists, the phone's own text-to-speech reads it instead.
    /// </summary>
    public sealed class LessonVoice : MonoBehaviour
    {
        [System.Serializable]
        public sealed class PageClips
        {
            [Tooltip("Name of the page object under the pages object (\"1\", \"2\", ...). Names are used because the page navigation reorders siblings.")] public string page;
            public AudioClip malay;
            public string malayHash;
            public AudioClip english;
            public string englishHash;
        }

        public RectTransform pages;
        public Button listenButton;
        public TMP_Text message;
        [Tooltip("Object shown and hidden with the message (its background). Defaults to the message itself.")]
        public GameObject messageRoot;
        public TMP_Text listenLabel;
        [Tooltip("Bundled recordings for this lesson's pages. Filled by Fun Math > UI > 15 - Assign lesson voice clips.")]
        public PageClips[] clips;
        public enum ReadingOrder { SceneOrder, ScreenPosition }
        [Tooltip("SceneOrder follows how the lesson was authored (a question comes before its answer). ScreenPosition reads top to bottom.")]
        public ReadingOrder order = ReadingOrder.SceneOrder;
        [Tooltip("Objects with this name hold a page's summary caption; it is read last.")]
        public string captionName = "Questions";
        public string[] skipNames = { "ActionLabel", "LessonProgress", "ReadingHint" };

        private GameObject currentPage;
        private Coroutine messageRoutine;
        private AudioSource player;
        private bool previousEnglish;
        public bool IsPlaying { get; private set; }
        public event System.Action<bool> PlaybackStateChanged;
        private void PublishPlayback()
        {
            bool value = Speaking;
            if (value == IsPlaying) return;
            IsPlaying = value; PlaybackStateChanged?.Invoke(value);
        }

        private GameObject MessageObject { get { return messageRoot ? messageRoot : message.gameObject; } }
        private static bool English { get { return PlayerPrefs.GetInt("EnglishLang", 0) == 1; } }
        private bool Speaking { get { return (player != null && player.isPlaying) || SpeechService.IsSpeaking; } }

        /// <summary>Short stable fingerprint (FNV-1a) of a page's speech text, used to match recordings to text.</summary>
        public static string Hash(string text)
        {
            unchecked
            {
                uint h = 2166136261;
                foreach (char c in text ?? "") { h ^= c; h *= 16777619; }
                return h.ToString("x8");
            }
        }

        private void Awake()
        {
            player = gameObject.AddComponent<AudioSource>();
            player.playOnAwake = false;
            currentPage = ActivePage();
            previousEnglish = English;
        }

        private void Start() { SpeechService.Warmup(); if (message) MessageObject.SetActive(false); }

        private void Update()
        {
            SpeechService.Pump();
            PublishPlayback();
            bool voiceOn = PlayerPrefs.GetInt("UI.Voice", 1) != 0;
            if (listenButton)
            {
                if (listenButton.gameObject.activeSelf != voiceOn) listenButton.gameObject.SetActive(voiceOn);
            }
            var page = ActivePage();
            if (page != currentPage || previousEnglish != English || (!voiceOn && Speaking))
            {
                currentPage = page;
                previousEnglish = English;
                StopSpeaking();
            }
            if (player != null) player.volume = PlaybackVolume();
            if (listenLabel)
                listenLabel.text = Speaking ? (English ? "Stop" : "Henti") : (English ? "Listen" : "Dengar");
        }

        /// <summary>Wired to the Listen button. Pressing it while speaking stops the voice.</summary>
        public void Listen()
        {
            if (PlayerPrefs.GetInt("UI.Voice", 1) == 0) { StopSpeaking(); return; }
            if (Speaking) { StopSpeaking(); return; }
            currentPage = ActivePage();
            previousEnglish = English;
            string text = BuildSpeech();
            if (string.IsNullOrEmpty(text)) { Show(English ? "There is nothing to read here." : "Tiada apa untuk dibaca di sini."); return; }

            var clip = RecordingFor(text);
            if (clip)
            {
                player.clip = clip;
                player.volume = PlaybackVolume();
                player.Play();
                PublishPlayback();
                return;
            }

            switch (SpeechService.Speak(text, English))
            {
                case SpeechService.Result.Spoken: break;
                case SpeechService.Result.NoVoiceForLanguage:
                    Show(English ? "Please install the English voice in Android settings."
                                 : "Sila pasang suara Bahasa Melayu dalam Tetapan Android."); break;
                case SpeechService.Result.NotReady:
                    Show(English ? "The voice is starting. Try again." : "Suara sedang bermula. Cuba lagi."); break;
                default:
                    Show(English ? "Voice plays on Android devices." : "Suara dimainkan pada peranti Android.");
                    Debug.Log("LessonVoice would read: " + text);
                    break;
            }
            PublishPlayback();
        }

        /// <summary>The bundled recording for the current page and language, if it still matches the page text.</summary>
        public AudioClip RecordingFor(string text)
        {
            if (clips == null || pages == null) return null;
            var page = ActivePage();
            if (!page) return null;
            string key = page.name;
            string fingerprint = Hash(text);
            foreach (var entry in clips)
            {
                if (entry.page != key) continue;
                bool en = English;
                var clip = en ? entry.english : entry.malay;
                string expected = en ? entry.englishHash : entry.malayHash;
                return clip && expected == fingerprint ? clip : null;
            }
            return null;
        }

        private void StopSpeaking()
        {
            if (player != null) player.Stop();
            SpeechService.Stop();
            PublishPlayback();
        }

        private static float PlaybackVolume()
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat("MasterVolume", 1))
                * Mathf.Clamp01(PlayerPrefs.GetFloat("sfx", 1));
        }

        private GameObject ActivePage()
        {
            if (!pages) return null;
            foreach (Transform child in pages) if (child.gameObject.activeInHierarchy) return child.gameObject;
            return null;
        }

        /// <summary>Name of the visible page object, or "". Used to match recordings to pages.</summary>
        public string ActivePageName()
        {
            var page = ActivePage();
            return page ? page.name : "";
        }

        /// <summary>The text of the current page in reading order, cleaned up for speech. Public so it can be tested.</summary>
        public string BuildSpeech()
        {
            var page = ActivePage();
            if (!page) return "";
            var narration = page.GetComponent<LessonNarration>();
            if (narration != null)
            {
                string authoredText = narration.TextFor(English);
                if (authoredText.Length > 0) return authoredText;
            }
            var items = new List<KeyValuePair<Vector2, string>>();
            var captions = new List<string>();
            foreach (var label in page.GetComponentsInChildren<TMP_Text>(false))
            {
                if (!label.enabled || !label.gameObject.activeInHierarchy) continue;
                if (label.canvasRenderer != null && label.canvasRenderer.GetInheritedAlpha() < .05f) continue;
                if (System.Array.IndexOf(skipNames, label.name) >= 0) continue;
                if (label.GetComponentInParent<Selectable>() != null) continue; // button captions
                string text = Clean(label.text);
                if (text.Length == 0) continue;
                if (order == ReadingOrder.SceneOrder && IsCaption(label.transform, page.transform)) { captions.Add(text); continue; }
                var corners = new Vector3[4];
                label.rectTransform.GetWorldCorners(corners);
                var centre = (corners[0] + corners[2]) * .5f;
                items.Add(new KeyValuePair<Vector2, string>(centre, text));
            }
            // Optionally top to bottom in rows about 60 pixels tall, then left to right.
            if (order == ReadingOrder.ScreenPosition) items.Sort((a, b) =>
            {
                int rowA = Mathf.RoundToInt(-a.Key.y / 60f), rowB = Mathf.RoundToInt(-b.Key.y / 60f);
                return rowA != rowB ? rowA.CompareTo(rowB) : a.Key.x.CompareTo(b.Key.x);
            });
            foreach (var caption in captions) items.Add(new KeyValuePair<Vector2, string>(Vector2.zero, caption));
            var sb = new StringBuilder();
            foreach (var item in items)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(item.Value);
                // Formula pages put every number and symbol in its own text box. A full stop after each one makes the voice
                // stop and start, so only sentence-length items get one; short fragments flow into the next.
                bool fragment = item.Value.Length <= 14 && item.Value.Split(' ').Length <= 2;
                if (!fragment && !Regex.IsMatch(item.Value, @"[.!?:]$")) sb.Append('.');
            }
            return sb.ToString();
        }

        private bool IsCaption(Transform label, Transform page)
        {
            for (var t = label; t != null && t != page; t = t.parent) if (t.name == captionName) return true;
            return false;
        }

        /// <summary>Voices read "=" and "x" unpredictably, so maths symbols are spelled out as words.</summary>
        private static string SpeakMaths(string text)
        {
            bool en = English;
            string equals = en ? " equals " : " sama dengan ", plus = en ? " plus " : " campur ";
            string times = en ? " times " : " darab ", divide = en ? " divided by " : " bahagi ", minus = en ? " minus " : " tolak ";
            text = text.Replace("=", equals).Replace("×", times).Replace("÷", divide).Replace("−", minus);
            text = Regex.Replace(text, @"(?<=\d|\s|^)\+(?=\d|\s|$)", plus);                 // + between numbers or on its own
            text = Regex.Replace(text, @"(?<=\d\s?|\)\s?)[xX](?=\s?\d|\s?\()", times);       // 8 x 3, (a) x (b)
            text = Regex.Replace(text, @"^\s*[xX]\s*$", times);                               // a lone "x" box
            text = Regex.Replace(text, @"^\s*-\s*$", minus);                                   // a lone "-" box
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        private static string Clean(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            string text = Regex.Replace(raw, "<[^>]+>", " ");                 // rich-text tags
            text = text.Replace('​', ' ').Replace(' ', ' ');        // zero-width and non-breaking spaces
            text = Regex.Replace(text, @"_{2,}", English ? " blank " : " ruang kosong "); // fill-in-the-blank lines
            text = SpeakMaths(text);
            text = Regex.Replace(text, @"\s+", " ").Trim();
            // Engines may spell out ALL-CAPS words letter by letter, so read them as normal words.
            if (text.Length > 3 && text == text.ToUpperInvariant() && Regex.IsMatch(text, "[A-Z]")) text = text.ToLowerInvariant();
            return text;
        }

        private void Show(string text)
        {
            if (!message) return;
            message.text = text;
            if (messageRoutine != null) StopCoroutine(messageRoutine);
            messageRoutine = StartCoroutine(Hide());
        }

        private IEnumerator Hide()
        {
            MessageObject.SetActive(true);
            yield return new WaitForSecondsRealtime(3f);
            MessageObject.SetActive(false);
            messageRoutine = null;
        }

        private void OnDisable() { StopSpeaking(); }
    }
}
