using UnityEngine;

namespace FunMath.UI
{
    /// <summary>Keeps legacy square modal artwork readable in either orientation.</summary>
    [ExecuteAlways]
    public sealed class ModalBodyFit : MonoBehaviour
    {
        [Range(.5f, .95f)] public float screenFraction = .78f;
        public bool respectSafeArea;
        private void LateUpdate()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (!canvas) return;
            var canvasRect = ((RectTransform)canvas.transform).rect;
            var rect = (RectTransform)transform;
            Vector2 available = canvasRect.size;
            if (respectSafeArea && Screen.width > 0 && Screen.height > 0)
            {
                var safe = Screen.safeArea;
                if (safe.width <= 0 || safe.height <= 0 || safe.xMin < 0 || safe.yMin < 0 || safe.xMax > Screen.width || safe.yMax > Screen.height)
                    safe = new Rect(0, 0, Screen.width, Screen.height);
                available *= safe.size / new Vector2(Screen.width, Screen.height);
                rect.anchoredPosition = (safe.center / new Vector2(Screen.width, Screen.height) - Vector2.one * .5f) * canvasRect.size;
            }
            float scale = screenFraction * Mathf.Min(available.x / Mathf.Max(1, rect.rect.width),
                available.y / Mathf.Max(1, rect.rect.height));
            rect.localScale = Vector3.one * scale;
        }
    }
}
