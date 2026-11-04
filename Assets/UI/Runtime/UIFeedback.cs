using UnityEngine;

namespace FunMath.UI
{
    public static class UIFeedback
    {
        private static AudioSource source;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { source = null; }
        public static void Tap(AdventureTheme theme, float pitch = 1f)
        {
            if (!theme || !theme.tap) return;
            if (!source)
            {
                var go = new GameObject("Adventure UI audio");
                Object.DontDestroyOnLoad(go);
                source = go.AddComponent<AudioSource>(); source.playOnAwake = false;
            }
            source.pitch = pitch; // the source is shared, so every call sets it explicitly (1 = normal)
            source.PlayOneShot(theme.tap, PlayerPrefs.GetFloat("MasterVolume", 1) * PlayerPrefs.GetFloat("sfx", 1) * .35f);
        }
    }
}
