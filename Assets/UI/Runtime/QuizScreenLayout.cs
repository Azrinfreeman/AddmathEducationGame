using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    [ExecuteAlways, RequireComponent(typeof(CanvasScaler))]
    public sealed class QuizScreenLayout : MonoBehaviour
    {
        public RectTransform pages;
        public RectTransform title;
        public RectTransform timer;
        public RectTransform exit;
        [Tooltip("Parent-settings gear. When set it sits beside Exit and the title shifts right to make room.")]
        public RectTransform settings;
        private Vector2 size;
        private Rect safe;

        private void Update()
        {
            var currentSize = new Vector2(Screen.width, Screen.height);
            if (size == currentSize && safe == Screen.safeArea) return;
            size = currentSize;
            safe = Screen.safeArea;
            var scaler = GetComponent<CanvasScaler>();
            scaler.referenceResolution = size.x > size.y ? new Vector2(1920, 1080) : new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = .5f;
            if (size.x <= 0 || size.y <= 0) return;
            var usable = safe;
            if (usable.width <= 0 || usable.height <= 0 || usable.xMax > size.x || usable.yMax > size.y)
                usable = new Rect(Vector2.zero, size);
            Vector2 min = usable.min / size;
            Vector2 max = usable.max / size;
            QuizPageLayout.Place(pages, min.x + .05f, min.y + .035f, max.x - .05f, max.y - .13f);
            // Header row: Exit and Parents side by side, then the title and timer. They used to overlap in the corner.
            bool wide = size.x > size.y;
            float buttonWidth = wide ? .10f : .14f, gap = .01f, left = min.x + .03f;
            float top = max.y - .03f, bottom = max.y - .105f;
            QuizPageLayout.Place(exit, left, bottom, left + buttonWidth, top);
            float titleLeft = left + buttonWidth + .02f;
            if (settings)
            {
                QuizPageLayout.Place(settings, left + buttonWidth + gap, bottom, left + 2 * buttonWidth + gap, top);
                titleLeft = left + 2 * buttonWidth + gap + .02f;
            }
            QuizPageLayout.Place(title, titleLeft, max.y - .11f, max.x - .05f, max.y - .02f);
            QuizPageLayout.Place(timer, .72f, 0, 1, 1);
        }
    }
}
