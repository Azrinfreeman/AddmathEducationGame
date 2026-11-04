using UnityEngine;

namespace FunMath.UI
{
    /// <summary>Reflows non-physics answer targets; keeps their sibling order.</summary>
    [ExecuteAlways]
    public sealed class GameAnswerBoardLayout : MonoBehaviour
    {
        public RectTransform[] targets;
        public RectTransform question;
        public RectTransform illustration;
        public RectTransform[] captions;
        public TMPro.TMP_Text[] answerLabels;
        public Color answerInk;
        [Tooltip("Vertical space occupied by a fully raised answer and its hole, in original canvas units.")]
        public float targetEnvelopeHeight = 260;
        private Vector2 size;

        /// <summary>
        /// The legacy answer text box is a little wider than its button (about 269 against 254 units), so auto-size
        /// let long answers ("7:00 PAGI", "SEMBILAN") run past the button's edges. Pin the box inside the button
        /// with padding and let auto-size shrink the text to that. Idempotent.
        /// </summary>
        private static void FitInsideButton(TMPro.TMP_Text label)
        {
            var rect = label.rectTransform;
            var parent = rect.parent as RectTransform;
            if (!parent) return;
            if (rect.anchorMin != Vector2.zero || rect.anchorMax != Vector2.one
                || rect.offsetMin != new Vector2(14, 6) || rect.offsetMax != new Vector2(-14, -6))
            {
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(14, 6); rect.offsetMax = new Vector2(-14, -6);
            }
            label.enableAutoSizing = true;
            if (label.fontSizeMin > 18) label.fontSizeMin = 18;
        }

        private void LateUpdate()
        {
            // Legacy appearance clips animate answer colours. Keep text legible on their fills.
            if (answerLabels != null)
                foreach (var label in answerLabels)
                {
                    if (!label) continue;
                    if (label.color != answerInk) label.color = answerInk;
                    FitInsideButton(label);
                }
            var canvas = GetComponentInParent<Canvas>();
            if (!canvas) return;
            var current = ((RectTransform)canvas.transform).rect.size;
            if (current == size) return;
            size = current;
            bool wide = size.x > size.y;
            QuizPageLayout.Place((RectTransform)transform, wide ? .03f : .05f, .12f, wide ? .97f : .95f, .78f);
            int columns = wide && targets.Length > 4 ? 3 : 2;
            int rows = Mathf.Max(1, Mathf.CeilToInt(targets.Length / (float)columns));
            float boardHeight = size.y * .66f;
            float targetScale = Mathf.Min(wide ? 1.65f : 1.15f,
                boardHeight * (wide ? .82f : .52f) / rows / Mathf.Max(1, targetEnvelopeHeight));
            for (int i = 0; i < targets.Length; i++)
            {
                var target = targets[i];
                float left = wide ? .38f : 0;
                float width = wide ? .60f : 1;
                float x = left + (i % columns + .5f) * width / columns;
                float y = .08f + (rows - i / columns - .5f) * (wide ? .82f : .52f) / rows;
                target.anchorMin = target.anchorMax = new Vector2(x, y);
                target.anchoredPosition = Vector2.zero;
                target.localScale = Vector3.one * targetScale;
            }
            QuizPageLayout.Place(question, wide ? 0 : .03f, wide ? .05f : .66f, wide ? .32f : .97f, 1);
            if (illustration)
            {
                QuizPageLayout.Place(illustration, wide ? .05f : .20f, .01f, wide ? .95f : .80f, .75f);
                // Levels were authored with different compensating scales (0.1 to 1.96). The box above already
                // sizes the picture, so a leftover scale shrinks it to a speck or makes it cover the answers.
                illustration.localScale = Vector3.one;
            }
            foreach (var caption in captions)
            {
                QuizPageLayout.Place(caption, .02f, .78f, .98f, 1);
                caption.GetComponent<TMPro.TMP_Text>().fontSizeMax = wide ? 64 : 52;
            }
        }
    }
}
