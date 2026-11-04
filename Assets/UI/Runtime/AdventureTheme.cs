using TMPro;
using UnityEngine;

namespace FunMath.UI
{
    [CreateAssetMenu(menuName = "Fun Math/Adventure Theme")]
    public sealed class AdventureTheme : ScriptableObject
    {
        public Color ink = new Color32(32, 58, 71, 255);
        public Color cream = new Color32(255, 248, 231, 255);
        public Color sky = new Color32(223, 244, 247, 255);
        public Color[] sections = { new Color32(255, 199, 112, 255), new Color32(173, 217, 184, 255), new Color32(155, 210, 238, 255), new Color32(255, 220, 110, 255) };
        public TMP_FontAsset displayFont;
        public TMP_FontAsset bodyFont;
        public Sprite roundedPanel;
        public Sprite owl;
        public AudioClip tap;
        [Range(.05f, .4f)] public float releaseSeconds = .2f;
        [Range(.85f, 1f)] public float pressedScale = .95f;
        public static bool ReduceMotion => PlayerPrefs.GetInt("UI.ReduceMotion", 0) == 1;
    }
}
