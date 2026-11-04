using UnityEngine;

namespace FunMath.UI
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class VoiceGuide : MonoBehaviour
    {
        [TextArea] public string malay = "Pilih satu aktiviti. Mari belajar matematik bersama!";
        [TextArea] public string english = "Choose an activity. Let's learn maths together!";
        public AudioClip englishRecording;
        public TMPro.TMP_Text status;
        public void RefreshCaption()
        {
            if (status) status.text = PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? english : malay;
        }
        public bool IsPlaying { get; private set; }
        public event System.Action<bool> PlaybackStateChanged;
        private bool previousEnglish;
        private void OnEnable() { RefreshCaption(); previousEnglish = PlayerPrefs.GetInt("EnglishLang", 0) == 1; AdventureSettings.VoiceChanged += VoiceChanged; }
        private void SetPlaying(bool value) { if (IsPlaying == value) return; IsPlaying = value; PlaybackStateChanged?.Invoke(value); }
        private void VoiceChanged(bool value) { if (!value) StopSpeaking(); }
        private void StopSpeaking()
        {
            GetComponent<AudioSource>().Stop();
#if UNITY_ANDROID && !UNITY_EDITOR
            if (speech != null && ready) speech.Call<int>("stop");
            playback?.Stop();
#endif
            SetPlaying(false);
        }
        private void Update()
        {
            bool englishNow = PlayerPrefs.GetInt("EnglishLang", 0) == 1;
            if (englishNow != previousEnglish || PlayerPrefs.GetInt("UI.Voice", 1) == 0) StopSpeaking();
            previousEnglish = englishNow;
            bool busy = GetComponent<AudioSource>().isPlaying;
#if UNITY_ANDROID && !UNITY_EDITOR
            playback?.Pump(); busy |= playback != null && playback.Busy;
#endif
            SetPlaying(busy);
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject speech;
        private volatile bool ready;
        private NativeSpeechPlayback playback;
        private sealed class InitListener : AndroidJavaProxy
        {
            private readonly System.Action<int> initialized;
            public InitListener(System.Action<int> callback) : base("android.speech.tts.TextToSpeech$OnInitListener") { initialized = callback; }
            public void onInit(int result) { initialized(result); }
        }
        private void Start()
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    speech = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new InitListener(result => ready = result == 0))));
            }
        }
#endif
        public void Speak()
        {
            StopSpeaking();
            if (PlayerPrefs.GetInt("UI.Voice", 1) == 0) return;
            bool en = PlayerPrefs.GetInt("EnglishLang", 0) == 1;
            if (status) status.text = en ? english : malay;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (ready && speech != null)
            {
                using (var locale = new AndroidJavaObject("java.util.Locale", en ? "en" : "ms", en ? "GB" : "MY"))
                {
                    int available = speech.Call<int>("setLanguage", locale);
                    if (available >= 0 && !string.IsNullOrWhiteSpace(en ? english : malay))
                    {
                        if (playback == null) playback = new NativeSpeechPlayback(speech);
                        SetPlaying(playback.Speak(speech, en ? english : malay));
                        if (IsPlaying) return;
                    }
                }
            }
#endif
            if (en && englishRecording)
            {
                var source = GetComponent<AudioSource>();
                source.volume = PlayerPrefs.GetFloat("MasterVolume", 1) * PlayerPrefs.GetFloat("sfx", 1);
                source.clip = englishRecording;
                source.Play();
                SetPlaying(true);
            }
            else if (status) status.text += en ? "\nVoice unavailable on this device." : "\nSuara Bahasa Melayu memerlukan suara peranti Android.";
        }
        public void ToggleMotion() { AdventureSettings.SetReduceMotion(!AdventureTheme.ReduceMotion); }
        public void ToggleVoice() { AdventureSettings.SetVoice(PlayerPrefs.GetInt("UI.Voice", 1) == 0); }
        private void OnDisable() { AdventureSettings.VoiceChanged -= VoiceChanged; StopSpeaking(); }
        private void OnDestroy()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (speech != null) { speech.Call("stop"); speech.Call("shutdown"); speech.Dispose(); }
#endif
        }
    }
}
