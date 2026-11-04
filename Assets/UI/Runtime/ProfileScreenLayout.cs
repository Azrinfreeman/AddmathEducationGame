using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    [ExecuteAlways]
    public sealed class ProfileScreenLayout : MonoBehaviour
    {
        public RectTransform avatars;
        public RectTransform form;
        private Vector2 previous;
        private void OnEnable() { previous = Vector2.zero; }
        private void Update()
        {
            var size = new Vector2(Screen.width, Screen.height);
            if (size == previous) return;
            previous = size;
            bool wide = size.x > size.y;
            GetComponent<CanvasScaler>().referenceResolution = wide ? new Vector2(1920, 1080) : new Vector2(1080, 1920);
            QuizPageLayout.Place(avatars, .05f, wide ? .16f : .48f, wide ? .48f : .95f, .84f);
            QuizPageLayout.Place(form, wide ? .54f : .07f, .13f, .93f, wide ? .83f : .46f);
        }
    }
}
