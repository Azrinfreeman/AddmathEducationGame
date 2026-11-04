using UnityEngine;

namespace FunMath.UI
{
    public sealed class OwlFloat : MonoBehaviour
    {
        [Range(0, 16)] public float amplitude = 6;
        private Vector2 rest;
        private RectTransform rect;
        private float elapsed;
        private void OnEnable() { rect = (RectTransform)transform; rest = rect.anchoredPosition; elapsed = 0; }
        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            rect.anchoredPosition = rest + Vector2.up * (AdventureTheme.ReduceMotion ? 0 : Mathf.Sin(elapsed * 2) * amplitude);
        }
        private void OnDisable() { if (rect) rect.anchoredPosition = rest; }
    }
}
