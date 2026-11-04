using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FunMath.UI
{
    public sealed class ScrollSelectionIntoView : MonoBehaviour, ISelectHandler
    {
        public void OnSelect(BaseEventData eventData)
        {
            var scroll = GetComponentInParent<ScrollRect>();
            var card = GetComponentInParent<ProfileCard>();
            if (!scroll || !card) return;
            Canvas.ForceUpdateCanvases();
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, card.transform);
            var viewport = scroll.viewport.rect;
            float shift = bounds.min.y < viewport.yMin ? viewport.yMin - bounds.min.y :
                bounds.max.y > viewport.yMax ? viewport.yMax - bounds.max.y : 0;
            scroll.content.anchoredPosition += new Vector2(0, shift);
        }
    }
}
