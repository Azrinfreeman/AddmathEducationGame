using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    /// <summary>A small reusable UI confetti burst. Never captures input or alters game time.</summary>
    public sealed class ResultCelebration : MonoBehaviour
    {
        public RectTransform[] pieces;
        [Range(.3f, 3f)] public float seconds = 1.4f;
        private float elapsed;
        private bool running;

        public void Play()
        {
            elapsed = 0;
            running = !AdventureTheme.ReduceMotion;
            SetVisible(running);
        }

        private void Update()
        {
            if (!running) return;
            elapsed += Time.unscaledDeltaTime;
            if (AdventureTheme.ReduceMotion || elapsed >= seconds)
            {
                running = false;
                SetVisible(false);
                return;
            }
            float t = elapsed / seconds;
            for (int i = 0; i < pieces.Length; i++)
            {
                var piece = pieces[i];
                if (!piece) continue;
                float direction = (i - (pieces.Length - 1) * .5f) / Mathf.Max(1, pieces.Length - 1);
                piece.anchoredPosition = new Vector2(direction * 1100 * t, 500 * t - 850 * t * t);
                piece.localRotation = Quaternion.Euler(0, 0, (i % 2 == 0 ? 1 : -1) * t * 300);
                var image = piece.GetComponent<Image>();
                var color = image.color;
                color.a = Mathf.Clamp01((1 - t) * 4);
                image.color = color;
            }
        }

        private void SetVisible(bool visible)
        {
            if (pieces == null) return;
            foreach (var piece in pieces) if (piece) piece.gameObject.SetActive(visible);
        }
        private void OnDisable() { running = false; SetVisible(false); }
    }
}
