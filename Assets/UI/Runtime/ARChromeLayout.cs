using UnityEngine;

namespace FunMath.UI
{
    /// <summary>Positions the AR overlay for portrait and landscape inside the safe area.</summary>
    [ExecuteAlways]
    public sealed class ARChromeLayout : MonoBehaviour
    {
        public RectTransform back;
        public RectTransform hint;
        public RectTransform found;
        public RectTransform frame;
        private Vector2 size;

        private void LateUpdate()
        {
            var rect = (RectTransform)transform;
            if (size == rect.rect.size) return;
            size = rect.rect.size;
            bool wide = size.x > size.y;
            QuizPageLayout.Place(back, wide ? .03f : .04f, wide ? .87f : .915f, wide ? .17f : .32f, wide ? .965f : .975f);
            QuizPageLayout.Place(hint, wide ? .30f : .05f, wide ? .84f : .795f, wide ? .70f : .95f, wide ? .965f : .905f);
            QuizPageLayout.Place(found, wide ? .30f : .05f, wide ? .84f : .795f, wide ? .70f : .95f, wide ? .965f : .905f);
            QuizPageLayout.Place(frame, wide ? .28f : .12f, wide ? .20f : .34f, wide ? .72f : .88f, wide ? .78f : .66f);
        }
    }
}
