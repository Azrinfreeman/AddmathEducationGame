using UnityEngine;

namespace FunMath.UI
{
    /// <summary>Reflows the existing question and answer controls without replacing their actions.</summary>
    [ExecuteAlways]
    public sealed class QuizPageLayout : MonoBehaviour
    {
        public RectTransform illustration;
        public RectTransform illustrationContent;
        public RectTransform question;
        public RectTransform answers;
        public RectTransform progressLabel;
        public RectTransform progressBar;
        public RectTransform next;
        public RectTransform previous;
        [Tooltip("Original illustration size in Canvas units. Keeps diagrams in proportion.")]
        public float illustrationDesignSize = 796;
        private Vector2 lastSize;

        private void OnEnable() { lastSize = Vector2.zero; }
        private void LateUpdate()
        {
            var rect = (RectTransform)transform;
            if (rect.rect.size == lastSize) return;
            lastSize = rect.rect.size;
            bool wide = lastSize.x > lastSize.y;
            Place(illustration, .02f, wide ? .14f : .43f, wide ? .47f : .98f, .88f);
            Place(question, wide ? .51f : .03f, wide ? .74f : .33f, .97f, wide ? .91f : .42f);
            Place(answers, wide ? .51f : .03f, .14f, .97f, wide ? .70f : .31f);
            Place(progressLabel, .12f, .94f, .88f, 1f);
            Place(progressBar, .12f, .91f, .88f, .93f);
            if (previous) previous.pivot = new Vector2(.5f, .5f);
            if (next) next.pivot = new Vector2(.5f, .5f);
            Place(previous, .03f, .01f, .24f, .11f);
            Place(next, .76f, .01f, .97f, .11f);
            for (int i = 0; answers && i < answers.childCount; i++)
            {
                int column = i % 2;
                int row = i / 2;
                Place(answers.GetChild(i) as RectTransform, column * .52f, row == 0 ? .54f : 0,
                    column * .52f + .48f, row == 0 ? 1 : .46f);
            }
            if (illustrationContent && illustration)
            {
                Canvas.ForceUpdateCanvases();
                float scale = Mathf.Min(illustration.rect.width, illustration.rect.height) / Mathf.Max(1, illustrationDesignSize);
                illustrationContent.localScale = Vector3.one * scale;
            }
        }

        public static void Place(RectTransform rect, float left, float bottom, float right, float top)
        {
            if (!rect) return;
            rect.anchorMin = new Vector2(left, bottom);
            rect.anchorMax = new Vector2(right, top);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
