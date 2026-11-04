using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FunMath.UI
{
    /// <summary>Fits owned lesson artwork uniformly while keeping reading and action areas separate.</summary>
    [ExecuteAlways]
    public sealed class LessonPageLayout : MonoBehaviour
    {
        public RectTransform diagramArea;
        public RectTransform diagramContent;
        [Tooltip("Original composition bounds in Canvas units; keeps text and diagrams in proportion.")]
        public Vector2 designSize = Vector2.one * 796;
        [Tooltip("Centre of the artwork bounds in LessonContent coordinates. Include captions that extend beyond the original card.")]
        public Vector2 designCenter;
        public RectTransform question;
        public RectTransform[] hints;
        public RectTransform[] answerGroups;
        public RectTransform[] actions;
        public RectTransform next;
        public RectTransform previous;
        public TMPro.TMP_Text progressLabel;
        public Slider progressBar;
        private Vector2 lastSize;
        private bool focused;
        private bool lastHasAnswers;
        private readonly System.Collections.Generic.List<Graphic> answerGraphics = new System.Collections.Generic.List<Graphic>();

        [ContextMenu("Recalculate artwork bounds")]
        public void RecalculateArtworkBounds()
        {
            if (!diagramContent) return;
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Recalculate lesson artwork bounds");
#endif
            // Include both language branches so switching language does not move the artwork.
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(diagramContent, diagramContent);
            designSize = bounds.size;
            designCenter = bounds.center;
            lastSize = Vector2.zero;
        }

        private void OnEnable()
        {
            lastSize = Vector2.zero;
            focused = false;
            answerGraphics.Clear();
            if (answerGroups == null) return;
            foreach (var group in answerGroups)
                if (group) answerGraphics.AddRange(group.GetComponentsInChildren<Graphic>(true));
        }

        private void LateUpdate()
        {
            var rect = (RectTransform)transform;
            bool hasAnswers = HasVisibleAnswerContent();
            if (rect.rect.size != lastSize || hasAnswers != lastHasAnswers)
            {
                lastSize = rect.rect.size;
                lastHasAnswers = hasAnswers;
                ApplyLayout(lastSize.x > lastSize.y, hasAnswers);
            }
            if (Application.isPlaying && !focused && EventSystem.current)
            {
                focused = true;
                var selected = EventSystem.current.currentSelectedGameObject;
                if (!selected || !selected.activeInHierarchy)
                {
                    var button = next ? next.GetComponent<Button>() : null;
                    if (button && button.IsInteractable()) EventSystem.current.SetSelectedGameObject(button.gameObject);
                }
            }
        }

        private bool HasVisibleAnswerContent()
        {
            // Worked examples and reference pictures need the same reserved space as answer buttons.
            foreach (var graphic in answerGraphics)
                if (graphic && graphic.isActiveAndEnabled && graphic.color.a > .01f) return true;
            return false;
        }

        private void ApplyLayout(bool wide, bool hasAnswers)
        {
            bool hasHints = hints != null && hints.Length > 0;
            QuizPageLayout.Place(diagramArea, .02f, wide ? .15f : hasAnswers ? .56f : hasHints ? .34f : .27f,
                wide ? .58f : .98f, .9f);
            QuizPageLayout.Place(question, wide ? .62f : .02f, wide ? .69f : hasAnswers ? .46f : hasHints ? .23f : .15f,
                .98f, wide ? .9f : hasAnswers ? .54f : hasHints ? .32f : .25f);
            QuizPageLayout.Place(progressLabel.rectTransform, .18f, .94f, .82f, 1f);
            QuizPageLayout.Place((RectTransform)progressBar.transform, .15f, .915f, .85f, .935f);
            // Give each explanation its own row above the navigation controls.
            int hintCount = hints == null ? 0 : hints.Length;
            for (int i = 0; i < hintCount; i++)
            {
                float bottom = .14f;
                float top = wide ? .63f : .22f;
                float rowHeight = (top - bottom) / hintCount;
                QuizPageLayout.Place(hints[i], wide ? .62f : .03f,
                    top - (i + 1) * rowHeight, .97f, top - i * rowHeight - .005f);
            }
            foreach (var group in answerGroups)
            {
                if (!group) continue;
                QuizPageLayout.Place(group, wide ? .62f : .03f, .14f, .97f, wide ? .65f : .44f);
                int index = 0;
                for (int i = 0; i < group.childCount; i++)
                {
                    var child = group.GetChild(i) as RectTransform;
                    if (!child || !child.GetComponent<Button>()) continue;
                    int column = index % 2;
                    int row = index / 2;
                    index++;
                    QuizPageLayout.Place(child, column * .52f, row == 0 ? .54f : 0, column * .52f + .48f, row == 0 ? 1 : .46f);
                }
            }
            Footer(previous, .01f, .23f);
            Footer(next, .77f, .99f);
            for (int i = 0; i < actions.Length; i++)
            {
                float width = .48f / Mathf.Max(1, actions.Length);
                Footer(actions[i], .26f + i * width, .26f + (i + 1) * width - .01f);
            }
            Canvas.ForceUpdateCanvases();
            float scale = Mathf.Min(diagramArea.rect.width / Mathf.Max(1, designSize.x),
                diagramArea.rect.height / Mathf.Max(1, designSize.y));
            diagramContent.localScale = Vector3.one * scale;
            diagramContent.anchoredPosition = -designCenter * scale;
        }

        private static void Footer(RectTransform rect, float left, float right)
        {
            if (!rect) return;
            rect.pivot = Vector2.one * .5f;
            QuizPageLayout.Place(rect, left, .01f, right, .115f);
        }
    }
}
