using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    [ExecuteAlways, RequireComponent(typeof(CanvasScaler))]
    public sealed class LessonScreenLayout : MonoBehaviour
    {
        public RectTransform pages;
        public RectTransform heading;
        public RectTransform back;
        public RectTransform settings;
        [Tooltip("Optional voice-over button. When set, the heading narrows to make room for it.")]
        public RectTransform listen;
        public RectTransform voiceMessage;
        private Vector2 previousSize;
        private Rect previousSafeArea;

        private void OnEnable() { previousSize = Vector2.zero; }
        private void Update()
        {
            var size = new Vector2(Screen.width, Screen.height);
            var safe = Screen.safeArea;
            if (size == previousSize && safe == previousSafeArea) return;
            previousSize = size;
            previousSafeArea = safe;
            if (size.x <= 0 || size.y <= 0) return;
            var scaler = GetComponent<CanvasScaler>();
            scaler.referenceResolution = size.x > size.y ? new Vector2(1920, 1080) : new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = .5f;
            if (safe.width <= 0 || safe.height <= 0 || safe.xMin < 0 || safe.yMin < 0 || safe.xMax > size.x || safe.yMax > size.y)
                safe = new Rect(Vector2.zero, size);
            Vector2 min = safe.min / size;
            Vector2 max = safe.max / size;
            QuizPageLayout.Place(pages, min.x + .04f, min.y + .03f, max.x - .04f, max.y - .14f);
            QuizPageLayout.Place(heading, min.x + .2f, max.y - .12f, max.x - (listen ? .34f : .2f), max.y - .025f);
            QuizPageLayout.Place(back, min.x + .035f, max.y - .11f, min.x + .17f, max.y - .03f);
            QuizPageLayout.Place(settings, max.x - .16f, max.y - .11f, max.x - .035f, max.y - .03f);
            if (listen) QuizPageLayout.Place(listen, max.x - .325f, max.y - .11f, max.x - .175f, max.y - .03f);
            if (voiceMessage) QuizPageLayout.Place(voiceMessage, min.x + .1f, max.y - .19f, max.x - .1f, max.y - .125f);
        }
    }
}
