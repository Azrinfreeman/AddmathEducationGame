using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    public sealed class QuizAnswerFeedback : MonoBehaviour
    {
        public buttonSelectedController selection;
        public TMP_Text caption;
        private void OnEnable() { selection.SelectionChanged += Show; }
        private void OnDisable() { selection.SelectionChanged -= Show; }

        private void Show(int index)
        {
            bool correct = selection.answers[index].GetComponent<ButtonCorrect>();
            bool english = PlayerPrefs.GetInt("EnglishLang", 0) == 1;
            caption.text = correct ? (english ? "Well done!" : "Bagus!") : (english ? "Try again. You can do it!" : "Cuba lagi. Kamu boleh!");
            foreach (var answer in selection.answers)
            {
                var resetColors = answer.colors;
                resetColors.normalColor = resetColors.selectedColor = Color.white;
                answer.colors = resetColors;
                var kid = answer.GetComponent<KidButton>();
                if (answer.image && kid && kid.theme) answer.image.color = kid.theme.sections[0];
            }
            // A written response accompanies colour so correctness never depends on hue alone.
            var colors = selection.answers[index].colors;
            colors.normalColor = colors.selectedColor = Color.white;
            selection.answers[index].colors = colors;
            selection.answers[index].image.color = correct ? new Color32(173, 217, 184, 255) : new Color32(255, 199, 172, 255);
        }
    }
}
