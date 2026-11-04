using UnityEngine;

namespace FunMath.UI
{
    /// <summary>Editable speech for a lesson page, independent of its visual label hierarchy.</summary>
    [DisallowMultipleComponent]
    public sealed class LessonNarration : MonoBehaviour
    {
        [Tooltip("Complete Malay narration. Use spoken words for fractions and measurement units.")]
        [TextArea(3, 12)] public string malay;

        [Tooltip("Complete English narration. Leave empty to use the page's visible labels.")]
        [TextArea(3, 12)] public string english;

        public string TextFor(bool useEnglish)
        {
            return System.Text.RegularExpressions.Regex.Replace((useEnglish ? english : malay) ?? "", @"\s+", " ").Trim();
        }
    }
}
