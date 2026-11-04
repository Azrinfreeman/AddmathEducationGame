using UnityEngine;

namespace FunMath.UI
{
    /// <summary>Positions the shell without changing the legacy physics canvas.</summary>
    [ExecuteAlways]
    public sealed class GameChromeLayout : MonoBehaviour
    {
        public RectTransform pause;
        public RectTransform heading;
        public RectTransform progress;
        public RectTransform secondary;
        public RectTransform hint;
        public RectTransform left;
        public RectTransform right;
        public RectTransform primaryAction;
        public RectTransform secondaryAction;
        public RectTransform help;
        private Vector2 size;

        private void LateUpdate()
        {
            var rect = (RectTransform)transform;
            if (size == rect.rect.size) return;
            size = rect.rect.size;
            bool wide = size.x > size.y;
            QuizPageLayout.Place(pause, .035f, .90f, wide ? .16f : .27f, .975f);
            QuizPageLayout.Place(heading, wide ? .18f : .30f, .90f, .965f, .975f);
            QuizPageLayout.Place(progress, .035f, .80f, wide ? .22f : .48f, .88f);
            if (secondary) QuizPageLayout.Place(secondary, wide ? .78f : .52f, .80f, .965f, .88f);
            // The balance board has a wide floor bar along the bottom (the only game with a help button), so in
            // landscape its hint moves into the empty strip between the counter and the help button.
            if (wide && help) QuizPageLayout.Place(hint, .25f, .79f, .78f, .89f);
            else QuizPageLayout.Place(hint, wide ? .025f : .055f, .02f, wide ? .27f : .945f, wide ? .22f : .08f);
            if (left) QuizPageLayout.Place(left, .035f, wide ? .25f : .095f, wide ? .175f : .27f, wide ? .44f : .215f);
            if (right) QuizPageLayout.Place(right, wide ? .825f : .73f, wide ? .25f : .095f, .965f, wide ? .44f : .215f);
            if (primaryAction)
            {
                // Portrait: the algorithm drop box fills the bottom of the screen, so Run and Clear sit in the strip
                // between the arrow tray and that box (beside its "ALGORITHM" caption) instead of covering the box.
                QuizPageLayout.Place(primaryAction, wide ? .80f : .71f, wide ? .34f : .213f, .965f, wide ? .52f : .263f);
                if (secondaryAction) QuizPageLayout.Place(secondaryAction, wide ? .80f : .43f, wide ? .13f : .213f, wide ? .965f : .70f, wide ? .31f : .263f);
                hint.gameObject.SetActive(wide);
            }
            if (help) QuizPageLayout.Place(help, wide ? .79f : .73f, wide ? .64f : .70f, .965f, wide ? .77f : .79f);
        }
    }
}
