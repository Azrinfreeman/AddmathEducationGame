using UnityEngine;

namespace FunMath.UI
{
    /// <summary>Arranges existing lesson controls; availability remains owned by NavigationState.</summary>
    [ExecuteAlways]
    public sealed class LessonPathLayout : MonoBehaviour
    {
        public RectTransform[] nodes;
        public RectTransform[] steppingStones;
        [Min(0)] public float gap = 24;
        [Tooltip("Room for the existing button focus scale, in Canvas units.")]
        [Min(0)] public float edgePadding = 24;
        [Range(0, .25f)] public float portraitStagger = .12f;
        private Vector2 previous;

        private void OnEnable() { previous = Vector2.zero; }
        private void OnValidate() { previous = Vector2.zero; }

        private void LateUpdate()
        {
            var area = (RectTransform)transform;
            if (previous == area.rect.size || nodes == null || nodes.Length == 0) return;
            previous = area.rect.size;
            var canvas = GetComponentInParent<Canvas>();
            bool wide = canvas && ((RectTransform)canvas.transform).rect.width > ((RectTransform)canvas.transform).rect.height;
            var canvasSize = canvas ? ((RectTransform)canvas.transform).rect.size : previous;
            // Three long translated captions need wide signs on a 4:3 tablet.
            bool compactLandscape = canvasSize.x < canvasSize.y * 1.6f;
            int columns = wide ? (nodes.Length > 3 ? 2 : compactLandscape ? 1 : nodes.Length) : 1;
            int rows = Mathf.CeilToInt(nodes.Length / (float)columns);
            float width = (previous.x - edgePadding * 2 - gap * (columns - 1)) / columns;
            float height = (previous.y - edgePadding * 2 - gap * (rows - 1)) / rows;
            float stagger = wide ? 0 : previous.x * portraitStagger;
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (!node) continue;
                // Reverse alternate rows so the landscape reading order follows the trail.
                int row = i / columns;
                int column = row % 2 == 0 ? i % columns : columns - 1 - i % columns;
                node.anchorMin = node.anchorMax = new Vector2(0, 1);
                node.pivot = new Vector2(0, 1);
                node.sizeDelta = new Vector2(width - stagger, height);
                node.anchoredPosition = new Vector2(edgePadding + column * (width + gap) + (i % 2 == 0 ? 0 : stagger), -edgePadding - row * (height + gap));
                // Tall landscape stops use illustration above caption; wide stops read as signs.
                bool tall = wide && rows == 1;
                var art = node.Find("ActivityArt") as RectTransform;
                var caption = node.Find("Label") as RectTransform;
                var number = node.Find("PathNumber") as RectTransform;
                if (art) Place(art, tall ? new Vector2(.18f, .55f) : new Vector2(.02f, .16f), tall ? new Vector2(.82f, .92f) : new Vector2(.26f, .84f));
                if (caption) Place(caption, tall ? new Vector2(.04f, .12f) : new Vector2(.3f, .08f), tall ? new Vector2(.96f, .53f) : new Vector2(.96f, .92f));
                if (number) Place(number, new Vector2(.015f, .01f), tall ? new Vector2(.25f, .12f) : new Vector2(.16f, .42f));
                if (i == 0 || steppingStones == null || i - 1 >= steppingStones.Length || !steppingStones[i - 1]) continue;
                var stone = steppingStones[i - 1];
                var last = nodes[i - 1];
                stone.anchorMin = stone.anchorMax = new Vector2(0, 1);
                stone.pivot = Vector2.one * .5f;
                Vector2 start = last.anchoredPosition + new Vector2(last.sizeDelta.x / 2, -last.sizeDelta.y / 2);
                Vector2 end = node.anchoredPosition + new Vector2(node.sizeDelta.x / 2, -node.sizeDelta.y / 2);
                stone.anchoredPosition = (start + end) / 2;
                stone.sizeDelta = Vector2.one * Mathf.Min(18, gap * .65f);
                stone.localRotation = Quaternion.Euler(0, 0, 45);
            }
        }
        private static void Place(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
