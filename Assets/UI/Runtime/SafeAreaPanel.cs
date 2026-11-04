using UnityEngine;

namespace FunMath.UI
{
    [ExecuteAlways]
    public sealed class SafeAreaPanel : MonoBehaviour
    {
        private Rect previous;
        private Vector2 size;
        private void Update()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            var safe = Screen.safeArea;
            // Device Simulator can retain a simulated safe area after switching to Game view.
            if (safe.width <= 0 || safe.height <= 0 || safe.xMin < 0 || safe.yMin < 0 ||
                safe.xMax > Screen.width || safe.yMax > Screen.height)
                safe = new Rect(0, 0, Screen.width, Screen.height);
            if (safe == previous && size == new Vector2(Screen.width, Screen.height)) return;
            previous = safe;
            size = new Vector2(Screen.width, Screen.height);
            var rect = (RectTransform)transform;
            rect.anchorMin = safe.position / size;
            rect.anchorMax = (safe.position + safe.size) / size;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
