using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    [ExecuteAlways, RequireComponent(typeof(GridLayoutGroup))]
    public sealed class AccountGridLayout : MonoBehaviour
    {
        public float spacing = 28;
        public int portraitColumns = 2;
        public int landscapeColumns = 3;
        private float previousWidth;
        private int previousCount = -1;
        private void LateUpdate()
        {
            var rect = (RectTransform)transform;
            if (previousWidth == rect.rect.width && previousCount == transform.childCount) return;
            previousWidth = rect.rect.width;
            previousCount = transform.childCount;
            var canvas = (RectTransform)GetComponentInParent<Canvas>().transform;
            int columns = Mathf.Max(1, canvas.rect.width > canvas.rect.height ? landscapeColumns : portraitColumns);
            var grid = GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.spacing = Vector2.one * spacing;
            float width = Mathf.Max(1, (previousWidth - spacing * (columns - 1)) / columns);
            grid.cellSize = new Vector2(width, width * 1.05f);
        }
    }
}
