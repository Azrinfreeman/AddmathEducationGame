using UnityEngine;

namespace FunMath.UI
{
    /// <summary>Reserves separate space for collection artwork and readable content.</summary>
    [ExecuteAlways]
    public sealed class RewardScreenLayout : MonoBehaviour
    {
        public RectTransform illustration;
        public RectTransform content;
        public bool badgeCollection;
        private Vector2 previous;
        private void OnEnable() { previous = Vector2.zero; }
        private void LateUpdate()
        {
            var area = (RectTransform)transform;
            if (previous == area.rect.size || !illustration || !content) return;
            previous = area.rect.size;
            bool wide = previous.x > previous.y;
            Place(illustration, wide ? .04f : .33f, wide ? .25f : .68f, wide ? .31f : .67f, wide ? .78f : .84f);
            Place(content, wide ? .35f : .05f, .15f, .95f, wide ? .83f : .66f);
            var adaptive = content.GetComponent<AdaptiveMenu>();
            if (badgeCollection && adaptive)
            {
                adaptive.portraitColumns = 2;
                adaptive.landscapeColumns = 3;
            }
        }
        private static void Place(RectTransform rect, float x0, float y0, float x1, float y1)
        {
            rect.anchorMin = new Vector2(x0, y0);
            rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
