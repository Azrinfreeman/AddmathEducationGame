#if UNITY_ANDROID && !UNITY_EDITOR
using System.Collections.Concurrent;
using UnityEngine;

namespace FunMath.UI
{
    /// <summary>Relays Android completion onto Unity's main thread. Never creates a speech engine.</summary>
    internal sealed class NativeSpeechPlayback
    {
        private sealed class Callback : AndroidJavaProxy
        {
            private readonly ConcurrentQueue<string> completed;
            public Callback(ConcurrentQueue<string> queue) : base("com.funmath.ui.SpeechPlaybackListener$Callback") { completed = queue; }
            public void onFinished(string id) { completed.Enqueue(id); }
        }
        private readonly ConcurrentQueue<string> completed = new ConcurrentQueue<string>();
        private readonly Callback callback;
        private readonly AndroidJavaObject listener;
        private string active;
        public bool Busy { get; private set; }
        public NativeSpeechPlayback(AndroidJavaObject engine)
        {
            callback = new Callback(completed);
            listener = new AndroidJavaObject("com.funmath.ui.SpeechPlaybackListener", callback);
            engine.Call<int>("setOnUtteranceProgressListener", listener);
        }
        public bool Speak(AndroidJavaObject engine, string text)
        {
            active = System.Guid.NewGuid().ToString("N");
            Busy = engine.Call<int>("speak", text, 0, null, active) == 0;
            if (!Busy) active = null;
            return Busy;
        }
        public void Pump()
        {
            while (completed.TryDequeue(out string id)) if (id == active) { Busy = false; active = null; }
        }
        public void Stop() { active = null; Busy = false; }
    }
}
#endif
