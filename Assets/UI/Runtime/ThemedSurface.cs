using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    [ExecuteAlways, RequireComponent(typeof(Image))]
    public sealed class ThemedSurface : MonoBehaviour
    {
        public AdventureTheme theme;
        [Tooltip("-1 = cream, -2 = sky; 0 onwards = section colours")]
        public int section;
        private void OnEnable() { Apply(); }
        private void OnValidate() { Apply(); }
        [ContextMenu("Apply theme")]
        public void Apply()
        {
            if (!theme) return;
            if (theme.sections == null || theme.sections.Length == 0) { GetComponent<Image>().color = theme.cream; return; }
            var image = GetComponent<Image>();
            image.color = section == -2 ? theme.sky : section == -1 ? theme.cream : theme.sections[Mathf.Clamp(section, 0, theme.sections.Length - 1)];
        }
    }
}
