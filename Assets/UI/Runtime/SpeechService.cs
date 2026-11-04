using UnityEngine;

namespace FunMath.UI
{
    /// <summary>
    /// Text-to-speech for lesson voice-over. On Android it uses the device's own voices (Malay and English), so no
    /// audio files ship with the app. In the editor and on other platforms nothing is spoken: <see cref="Speak"/>
    /// returns false and callers show a caption instead. One engine is shared by the whole app.
    /// </summary>
    public static class SpeechService
    {
        public enum Result { Spoken, NoVoiceForLanguage, NotReady, Unsupported }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject engine;
        private static volatile bool ready;
        private static bool starting;
        private static NativeSpeechPlayback playback;

        private sealed class InitListener : AndroidJavaProxy
        {
            public InitListener() : base("android.speech.tts.TextToSpeech$OnInitListener") { }
            public void onInit(int status) { ready = status == 0; }
        }

        /// <summary>Starts the engine early (call from Start) so the first Listen tap is instant.</summary>
        public static void Warmup()
        {
            if (engine != null || starting) return;
            starting = true;
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    engine = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new InitListener())));
            }
        }

        public static Result Speak(string text, bool english)
        {
            if (string.IsNullOrWhiteSpace(text)) return Result.Unsupported;
            Warmup();
            if (!ready || engine == null) return Result.NotReady;
            using (var locale = new AndroidJavaObject("java.util.Locale", english ? "en" : "ms", english ? "GB" : "MY"))
            {
                // -1 = language data missing, -2 = language not supported by this device.
                if (engine.Call<int>("setLanguage", locale) < 0) return Result.NoVoiceForLanguage;
            }
            engine.Call<int>("setSpeechRate", .9f); // a little slower for young listeners
            if (playback == null) playback = new NativeSpeechPlayback(engine);
            return playback.Speak(engine, text) ? Result.Spoken : Result.NotReady;
        }

        public static void Stop()
        {
            if (engine != null && ready) engine.Call<int>("stop");
            playback?.Stop();
        }

        public static void Pump() { playback?.Pump(); }
        public static bool IsSpeaking => playback != null && playback.Busy;
#else
        public static void Warmup() { }
        public static Result Speak(string text, bool english) { return Result.Unsupported; }
        public static void Stop() { }
        public static bool IsSpeaking => false;
        public static void Pump() { }
#endif
    }
}
