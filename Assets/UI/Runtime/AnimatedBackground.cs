using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    /// <summary>Presentation only: bounded ambience, composed rest poses and an optional sun greeting.</summary>
    public sealed class AnimatedBackground : MonoBehaviour
    {
        public BackgroundProfile profile;
        public Image sky, farHill, nearHill;
        public RectTransform[] clouds;
        public RectTransform[] corners;
        public Animator scenery;
        public RectTransform discovery;
        public Button discoveryButton;
        public GameObject discoveryCaption;
        public ModalInputOwner modalOwner;
        public VoiceGuide guide;
        public LessonVoice lessonVoice;
        public RectTransform mascotVisual;
        public bool IsStill => !profile || profile.mode == BackgroundProfile.MotionMode.Still || reduced || !focused || paused || modalBusy || guideBusy || lessonBusy;
        private static readonly int Mode = Animator.StringToHash("Mode");
        private Vector2[] cloudRest;
        private Quaternion[] cornerRest;
        private Vector3 discoveryRest;
        private bool reduced, focused = true, paused, modalBusy, guideBusy, lessonBusy;
        private float time, reaction, nextReaction;
        private int animatorMode = -1;
        private float greetingTime;

        private void Awake()
        {
            cloudRest = new Vector2[clouds.Length];
            for (int i = 0; i < clouds.Length; i++) if (clouds[i]) cloudRest[i] = clouds[i].anchoredPosition;
            cornerRest = new Quaternion[corners.Length];
            for (int i = 0; i < corners.Length; i++) if (corners[i]) cornerRest[i] = corners[i].localRotation;
            if (discovery) discoveryRest = discovery.localScale;
            if (discoveryButton) discoveryButton.onClick.AddListener(Discover);
        }
        private void OnEnable()
        {
            reduced = AdventureTheme.ReduceMotion;
            AdventureSettings.ReduceMotionChanged += MotionChanged;
            if (!modalOwner) modalOwner = ModalInputOwner.ForScene(gameObject.scene);
            if (modalOwner) { modalBusy = modalOwner.HasModal; modalOwner.VisibilityChanged += ModalChanged; }
            if (guide) { guideBusy = guide.IsPlaying; guide.PlaybackStateChanged += GuideChanged; }
            if (lessonVoice) { lessonBusy = lessonVoice.IsPlaying; lessonVoice.PlaybackStateChanged += LessonChanged; }
            if (profile)
            {
                if (sky) sky.color = profile.sky;
                if (farHill) farHill.color = profile.farHill;
                if (nearHill) nearHill.color = profile.nearHill;
                foreach (var cloud in clouds) if (cloud && cloud.TryGetComponent<Image>(out var image)) image.color = profile.cloud;
            }
            RefreshState();
            greetingTime = 0;
        }
        private void MotionChanged(bool value) { reduced = value; RefreshState(); }
        private void ModalChanged(bool value) { modalBusy = value; RefreshState(); }
        private void GuideChanged(bool value) { guideBusy = value; RefreshState(); }
        private void LessonChanged(bool value) { lessonBusy = value; RefreshState(); }
        private void OnApplicationFocus(bool value) { focused = value; RefreshState(); }
        private void OnApplicationPause(bool value) { paused = value; RefreshState(); }
        private void RefreshState()
        {
            int mode = IsStill ? 0 : profile.mode == BackgroundProfile.MotionMode.Explore ? 1 : 2;
            if (scenery && scenery.isActiveAndEnabled && scenery.runtimeAnimatorController && mode != animatorMode)
            {
                scenery.SetInteger(Mode, mode); scenery.Update(0); animatorMode = mode;
            }
            if (IsStill) { reaction = 0; Rest(); }
            if (discoveryCaption && (modalBusy || guideBusy || lessonBusy)) discoveryCaption.SetActive(false);
            if (discoveryButton) discoveryButton.interactable = !modalBusy && !guideBusy && !lessonBusy;
        }
        private void Update()
        {
            if (discoveryCaption && Time.unscaledTime >= nextReaction) discoveryCaption.SetActive(false);
            if (IsStill) return;
            time += Time.unscaledDeltaTime;
            if (mascotVisual && greetingTime < .9f)
            {
                greetingTime = Mathf.Min(.9f, greetingTime + Time.unscaledDeltaTime);
                mascotVisual.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(greetingTime / .9f * Mathf.PI * 2) * 4 * (1 - greetingTime / .9f));
            }
            int count = profile.mode == BackgroundProfile.MotionMode.Focus ? Mathf.Min(1, clouds.Length) : clouds.Length;
            for (int i = 0; i < count; i++) if (clouds[i])
                clouds[i].anchoredPosition = cloudRest[i] + Vector2.right * (Mathf.Sin(time * Mathf.PI * 2 / profile.cloudPeriod + i) - Mathf.Sin(i)) * profile.cloudTravel;
            if (profile.mode == BackgroundProfile.MotionMode.Explore)
                for (int i = 0; i < corners.Length; i++) if (corners[i])
                    corners[i].localRotation = cornerRest[i] * Quaternion.Euler(0, 0, Mathf.Sin(time * Mathf.PI * 2 / profile.swayPeriod + i) * profile.cornerSway);
            if (reaction > 0 && discovery)
            {
                reaction = Mathf.Max(0, reaction - Time.unscaledDeltaTime);
                discovery.localScale = discoveryRest * (1 + Mathf.Sin((1 - reaction / .65f) * Mathf.PI) * .12f);
            }
        }
        public void Discover()
        {
            if (modalBusy || guideBusy || lessonBusy || Time.unscaledTime < nextReaction) return;
            nextReaction = Time.unscaledTime + (profile ? profile.discoveryCooldown : 1.2f);
            if (discoveryCaption) discoveryCaption.SetActive(true);
            if (!IsStill) reaction = .65f;
        }
        private void Rest()
        {
            if (cloudRest != null) for (int i = 0; i < clouds.Length; i++) if (clouds[i]) clouds[i].anchoredPosition = cloudRest[i];
            if (cornerRest != null) for (int i = 0; i < corners.Length; i++) if (corners[i]) corners[i].localRotation = cornerRest[i];
            if (discovery) discovery.localScale = discoveryRest;
            if (mascotVisual) { mascotVisual.localRotation = Quaternion.identity; greetingTime = .9f; }
        }
        private void OnDisable()
        {
            AdventureSettings.ReduceMotionChanged -= MotionChanged;
            if (modalOwner) modalOwner.VisibilityChanged -= ModalChanged;
            if (guide) guide.PlaybackStateChanged -= GuideChanged;
            if (lessonVoice) lessonVoice.PlaybackStateChanged -= LessonChanged;
            Rest();
        }
        private void OnDestroy() { if (discoveryButton) discoveryButton.onClick.RemoveListener(Discover); }
    }
}
