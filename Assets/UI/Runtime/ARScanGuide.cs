using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Vuforia;

namespace FunMath.UI
{
    /// <summary>
    /// Friendly overlay for the AR scenes: while the card is not in view it shows an owl hint and a pulsing
    /// scan frame; the moment Vuforia tracks the card it hides them and pops a short "card found" message with
    /// a rising tap sound. It only reads the image target's status and never changes tracking, video or the
    /// scene's existing show/hide logic (<c>DefaultObserverEventHandler</c> keeps driving the video surface).
    /// </summary>
    public sealed class ARScanGuide : MonoBehaviour
    {
        public ImageTargetBehaviour target;
        public AdventureTheme theme;
        public CanvasGroup hint;
        public RectTransform frame;
        public CanvasGroup found;
        [Min(.3f)] public float foundSeconds = 1.8f;

        private bool tracked;
        private float elapsed;
        private Coroutine foundRoutine;

        private void OnEnable()
        {
            if (target) target.OnTargetStatusChanged += OnStatusChanged;
            // Start in the "looking for the card" state; the first status callback corrects it if needed.
            Apply(false, false);
        }

        private void OnDisable()
        {
            if (target) target.OnTargetStatusChanged -= OnStatusChanged;
            if (foundRoutine != null) { StopCoroutine(foundRoutine); foundRoutine = null; }
        }

        private void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
        {
            SetTracked(status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED);
        }

        /// <summary>Also used by tests to simulate finding or losing the card.</summary>
        public void SetTracked(bool value)
        {
            if (value == tracked) return;
            tracked = value;
            Apply(value, true);
        }

        private void Apply(bool isTracked, bool animate)
        {
            if (hint) hint.alpha = isTracked ? 0 : 1;
            if (frame) frame.gameObject.SetActive(!isTracked);
            if (foundRoutine != null) { StopCoroutine(foundRoutine); foundRoutine = null; }
            if (found) found.alpha = 0;
            if (isTracked && animate && isActiveAndEnabled)
            {
                UIFeedback.Tap(theme, 1.25f);
                foundRoutine = StartCoroutine(ShowFound());
            }
        }

        private IEnumerator ShowFound()
        {
            var rect = (RectTransform)found.transform;
            for (float t = 0; t < foundSeconds; t += Time.unscaledDeltaTime)
            {
                float n = t / foundSeconds;
                found.alpha = Mathf.Clamp01(n * 10) * (1 - Mathf.Clamp01((n - .7f) / .3f));
                // Pop in with a little overshoot; reduced motion only fades.
                float pop = AdventureTheme.ReduceMotion ? 1 : 1 + .25f * Mathf.Pow(1 - Mathf.Clamp01(n * 5), 2);
                rect.localScale = Vector3.one * pop;
                yield return null;
            }
            found.alpha = 0;
            foundRoutine = null;
        }

        private void Update()
        {
            if (!tracked && frame && frame.gameObject.activeSelf)
            {
                elapsed += Time.unscaledDeltaTime;
                float scale = AdventureTheme.ReduceMotion ? 1 : 1 + .025f * Mathf.Sin(elapsed * 3.2f);
                frame.localScale = new Vector3(scale, scale, 1);
            }
        }
    }
}
