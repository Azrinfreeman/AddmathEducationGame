using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    /// <summary>
    /// Positive, short feedback layered on existing game events: a floating "+1", a small
    /// confetti burst, a rising-pitch tap sound and a brief owl cheer. It only reads the
    /// controllers' change events and never touches gameplay objects or the game clock.
    /// A wrong move gets a gentle "try again" owl only: no red, no shake, no harsh sound.
    /// </summary>
    public sealed class GameFeedback : MonoBehaviour
    {
        public Game1Controller answers;
        public GamePraController rocket;
        public AdventureTheme theme;
        [Tooltip("Where the +1 and confetti start (normally the progress chip).")]
        public RectTransform origin;
        public TMP_Text popLabel;
        public RectTransform[] confetti;
        public CanvasGroup cheer;
        public TMP_Text cheerLabel;
        public string[] cheerMalay = { "Bagus!", "Hebat!", "Pandai!" };
        public string[] cheerEnglish = { "Great!", "Well done!", "Clever!" };
        public string retryMalay = "Cuba lagi!";
        public string retryEnglish = "Try again!";
        [Min(1)] public float streakWindowSeconds = 6;

        private int lastCorrect = -1, lastScore = int.MinValue;
        private int streak;
        private float cheerBaseY = float.NaN;
        private float lastGoodTime = -100;
        private Coroutine popRoutine, burstRoutine, cheerRoutine;

        private void OnEnable()
        {
            if (answers) answers.ProgressChanged += OnChanged;
            if (rocket) rocket.ProgressChanged += OnChanged;
            // First notification only records the starting values.
            lastCorrect = -1; lastScore = int.MinValue;
            HideAll();
        }

        private void OnDisable()
        {
            if (answers) answers.ProgressChanged -= OnChanged;
            if (rocket) rocket.ProgressChanged -= OnChanged;
            HideAll();
        }

        private void HideAll()
        {
            if (popLabel) popLabel.gameObject.SetActive(false);
            if (cheer) cheer.alpha = 0;
            if (confetti != null) foreach (var c in confetti) if (c) c.gameObject.SetActive(false);
        }

        private void OnChanged()
        {
            bool good = false, bad = false;
            if (answers)
            {
                if (lastCorrect >= 0) good = answers.toCorrect > lastCorrect;
                lastCorrect = answers.toCorrect;
            }
            if (rocket)
            {
                if (lastScore != int.MinValue) { good = rocket.score > lastScore; bad = rocket.score < lastScore; }
                lastScore = rocket.score;
            }
            if (good) Celebrate(); else if (bad) Encourage();
        }

        private void Celebrate()
        {
            streak = Time.unscaledTime - lastGoodTime <= streakWindowSeconds ? streak + 1 : 1;
            lastGoodTime = Time.unscaledTime;
            // The pitch climbs with a run of good answers, resetting after a pause or a miss.
            UIFeedback.Tap(theme, 1f + .08f * Mathf.Min(streak - 1, 5));
            var phrases = PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? cheerEnglish : cheerMalay;
            ShowCheer(phrases[Mathf.Clamp(streak - 1, 0, phrases.Length - 1) % Mathf.Max(1, phrases.Length)]);
            if (AdventureTheme.ReduceMotion) return;
            Restart(ref popRoutine, Pop());
            Restart(ref burstRoutine, Burst());
        }

        private void Encourage()
        {
            streak = 0;
            ShowCheer(PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? retryEnglish : retryMalay);
        }

        private void ShowCheer(string text)
        {
            if (!cheer) return;
            if (cheerLabel) cheerLabel.text = text;
            Restart(ref cheerRoutine, CheerRoutine());
        }

        private void Restart(ref Coroutine routine, IEnumerator next)
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(next);
        }

        private IEnumerator CheerRoutine()
        {
            var rect = (RectTransform)cheer.transform;
            if (float.IsNaN(cheerBaseY)) cheerBaseY = rect.anchoredPosition.y;
            const float total = 1.3f;
            for (float t = 0; t < total; t += Time.unscaledDeltaTime)
            {
                float n = t / total;
                cheer.alpha = Mathf.Clamp01(n * 8) * (1 - Mathf.Clamp01((n - .75f) * 4));
                // Hop up with a little overshoot; reduced motion only fades.
                float rise = AdventureTheme.ReduceMotion ? 0 : 60 * (1 - Mathf.Pow(1 - Mathf.Clamp01(n * 4), 3)) + Mathf.Sin(Mathf.Clamp01(n * 4) * Mathf.PI) * 14;
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, cheerBaseY + rise);
                yield return null;
            }
            cheer.alpha = 0;
            cheerRoutine = null;
        }

        private IEnumerator Pop()
        {
            popLabel.text = "+1";
            popLabel.gameObject.SetActive(true);
            var rect = popLabel.rectTransform;
            var start = origin ? origin.position : rect.position;
            const float total = .9f;
            for (float t = 0; t < total; t += Time.unscaledDeltaTime)
            {
                float n = t / total;
                rect.position = start + Vector3.up * (n * 90 * (origin ? origin.lossyScale.y : 1));
                rect.localScale = Vector3.one * (1 + .4f * Mathf.Pow(1 - Mathf.Clamp01(n * 4), 2));
                popLabel.alpha = 1 - Mathf.Clamp01((n - .5f) * 2);
                yield return null;
            }
            popLabel.gameObject.SetActive(false);
            popRoutine = null;
        }

        private IEnumerator Burst()
        {
            if (confetti == null || confetti.Length == 0) yield break;
            var start = origin ? origin.position : transform.position;
            float scale = origin ? origin.lossyScale.x : 1;
            const float total = .7f;
            for (int i = 0; i < confetti.Length; i++) if (confetti[i]) confetti[i].gameObject.SetActive(true);
            for (float t = 0; t < total; t += Time.unscaledDeltaTime)
            {
                float n = t / total;
                for (int i = 0; i < confetti.Length; i++)
                {
                    var piece = confetti[i]; if (!piece) continue;
                    float angle = (i / (float)confetti.Length) * Mathf.PI * 2 + .4f;
                    float distance = 150 * (1 - Mathf.Pow(1 - n, 3)) * scale;
                    piece.position = start + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * distance;
                    piece.localRotation = Quaternion.Euler(0, 0, n * 200 * (i % 2 == 0 ? 1 : -1));
                    var image = piece.GetComponent<Image>();
                    var color = image.color; color.a = 1 - Mathf.Clamp01((n - .4f) / .6f); image.color = color;
                }
                yield return null;
            }
            foreach (var c in confetti) if (c) c.gameObject.SetActive(false);
            burstRoutine = null;
        }
    }
}
