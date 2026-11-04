using UnityEngine;

namespace FunMath.UI
{
    /// <summary>Fits the non-physics question artwork beneath the shared HUD.</summary>
    [ExecuteAlways]
    public sealed class RocketQuestionLayout : MonoBehaviour
    {
        public Vector2 designSize = new Vector2(800, 360);
        private Vector2 canvasSize;

        private void LateUpdate()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (!canvas) return;
            var size = ((RectTransform)canvas.transform).rect.size;
            if (size == canvasSize) return;
            canvasSize = size;
            var rect = (RectTransform)transform;
            // Landscape: the bubble lane is centred on screen, so the question sits in the free space on the left.
            rect.anchorMin = rect.anchorMax = size.x > size.y ? new Vector2(.17f, .67f) : new Vector2(.5f, .67f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = designSize;
            rect.localScale = Vector3.one;
        }
    }
}
