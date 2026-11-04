using TMPro;
using UnityEngine;

namespace FunMath.UI
{
    /// <summary>Displays snapshots published by the existing game controllers.</summary>
    public sealed class GameHud : MonoBehaviour
    {
        public Game1Controller answers;
        public GamePraController rocket;
        public TimeGameController timer;
        public ScaleCounterController scales;
        public TMP_Text progress;
        public TMP_Text secondary;
        private float pulseRemaining;
        private float shownScore;
        private int targetScore;

        private void OnEnable()
        {
            if (answers) answers.ProgressChanged += Refresh;
            if (rocket) rocket.ProgressChanged += Refresh;
            if (timer) timer.SecondsChanged += RefreshTimer;
            if (scales) scales.PlacementChanged += Refresh;
            Refresh();
            RefreshTimer();
        }

        private void OnDisable()
        {
            if (answers) answers.ProgressChanged -= Refresh;
            if (rocket) rocket.ProgressChanged -= Refresh;
            if (timer) timer.SecondsChanged -= RefreshTimer;
            if (scales) scales.PlacementChanged -= Refresh;
            if (progress) progress.rectTransform.localScale = Vector3.one;
        }

        private void Refresh()
        {
            if (!progress) return;
            if (answers) progress.text = answers.toCorrect + " / " + answers.correctAnswers;
            else if (rocket)
            {
                progress.text = Mathf.Clamp(rocket.level, 1, 10) + " / 10";
                targetScore = rocket.score;
                // Count up smoothly; reduced motion (and a score going down on restart) snap straight to it.
                if (AdventureTheme.ReduceMotion || targetScore < shownScore) shownScore = targetScore;
                if (secondary && Mathf.RoundToInt(shownScore) == targetScore) secondary.text = targetScore.ToString();
            }
            else if (scales)
                progress.text = scales.count + " / " + (BallController.instance ? BallController.instance.num : 0);
            pulseRemaining = .2f;
        }

        private void RefreshTimer()
        {
            if (!timer || !secondary) return;
            int seconds = Mathf.Max(0, Mathf.CeilToInt(timer.timer));
            secondary.text = (seconds / 60) + ":" + (seconds % 60).ToString("00");
        }

        private void Update()
        {
            if (rocket && secondary && Mathf.RoundToInt(shownScore) != targetScore)
            {
                // About 0.6 s for any jump, never slower than 12 points per second.
                float speed = Mathf.Max(12f, Mathf.Abs(targetScore - shownScore) / .6f);
                shownScore = Mathf.MoveTowards(shownScore, targetScore, speed * Time.unscaledDeltaTime);
                secondary.text = Mathf.RoundToInt(shownScore).ToString();
            }
            if (!progress || pulseRemaining <= 0) return;
            pulseRemaining = Mathf.Max(0, pulseRemaining - Time.unscaledDeltaTime);
            // Only the number moves. Colliders and gameplay transforms stay untouched.
            float amount = AdventureTheme.ReduceMotion ? 0 : .06f * Mathf.Pow(pulseRemaining / .2f, 3);
            progress.rectTransform.localScale = Vector3.one * (1 + amount);
        }
    }
}
