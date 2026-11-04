using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace FunMath.UI
{
    /// <summary>
    /// "3 - 2 - 1 - Go!" shown after a game's intro is dismissed. The legacy intro calls
    /// <see cref="Begin"/> instead of starting the clock directly; the clock then starts when the
    /// countdown ends. Uses unscaled time so it still runs while a pause freezes Time.timeScale.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class GameCountdown : MonoBehaviour
    {
        public TMP_Text label;
        public AdventureTheme theme;
        public string[] malaySteps = { "3", "2", "1", "Pergi!" };
        public string[] englishSteps = { "3", "2", "1", "Go!" };
        [Min(.3f)] public float stepSeconds = .8f;

        public static GameCountdown Current { get; private set; }
        private Coroutine running;
        private CanvasGroup group;

        private void Awake()
        {
            Current = this;
            group = GetComponent<CanvasGroup>();
            group.alpha = 0; group.blocksRaycasts = false; group.interactable = false;
        }

        private void OnDestroy() { if (Current == this) Current = null; }

        /// <summary>Starts the countdown; returns false (caller should start immediately) when none is available.</summary>
        public static bool Begin(Action onFinished)
        {
            if (!Current || !Current.isActiveAndEnabled || Current.running != null) return false;
            Current.running = Current.StartCoroutine(Current.Run(onFinished));
            return true;
        }

        private IEnumerator Run(Action onFinished)
        {
            var steps = PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? englishSteps : malaySteps;
            group.blocksRaycasts = true; // answers cannot be tapped before the clock starts
            for (int i = 0; i < steps.Length; i++)
            {
                label.text = steps[i];
                UIFeedback.Tap(theme);
                float t = 0;
                while (t < stepSeconds)
                {
                    t += Time.unscaledDeltaTime;
                    float n = Mathf.Clamp01(t / stepSeconds);
                    group.alpha = Mathf.Clamp01(n * 8) * (1 - Mathf.Clamp01((n - .75f) * 4));
                    // Pop in with a little overshoot; reduced motion keeps the number still.
                    float scale = AdventureTheme.ReduceMotion ? 1 : 1 + .35f * Mathf.Pow(1 - Mathf.Clamp01(n * 3), 2);
                    label.rectTransform.localScale = Vector3.one * scale;
                    yield return null;
                }
            }
            group.alpha = 0; group.blocksRaycasts = false;
            running = null;
            onFinished?.Invoke();
        }
    }
}
