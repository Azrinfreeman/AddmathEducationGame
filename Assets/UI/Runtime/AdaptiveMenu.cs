using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    [ExecuteAlways, RequireComponent(typeof(GridLayoutGroup))]
    public sealed class AdaptiveMenu : MonoBehaviour
    {
        public int portraitColumns = 2;
        public int landscapeColumns = 3;
        public float gap = 24;
        private Vector2 previous;
        private int previousCount = -1;
        private void OnEnable() { previous = Vector2.zero; }
        private void LateUpdate()
        {
            var rect = (RectTransform)transform;
            int count = 0;
            foreach (Transform child in transform) if (child.gameObject.activeSelf) count++;
            if (rect.rect.size == previous && count == previousCount) return;
            previousCount = count;
            previous = rect.rect.size;
            var grid = GetComponent<GridLayoutGroup>();
            var canvas = GetComponentInParent<Canvas>();
            bool wide = canvas && ((RectTransform)canvas.transform).rect.width > ((RectTransform)canvas.transform).rect.height;
            int columns = Mathf.Max(1, wide ? landscapeColumns : portraitColumns);
            int rows = Mathf.Max(1, Mathf.CeilToInt(count / (float)columns));
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.spacing = Vector2.one * gap;
            grid.cellSize = new Vector2(Mathf.Max(1, (previous.x - gap * (columns - 1)) / columns),
                Mathf.Max(1, (previous.y - gap * (rows - 1)) / rows));
        }
    }
}
