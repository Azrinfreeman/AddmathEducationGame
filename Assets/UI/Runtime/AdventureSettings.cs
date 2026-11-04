using UnityEngine;
using UnityEngine.UI;

namespace FunMath.UI
{
    public sealed class AdventureSettings : MonoBehaviour
    {
        public static event System.Action<bool> ReduceMotionChanged;
        public static event System.Action<bool> VoiceChanged;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEvents() { ReduceMotionChanged = null; VoiceChanged = null; }
        public static void SetReduceMotion(bool value)
        {
            PlayerPrefs.SetInt("UI.ReduceMotion", value ? 1 : 0); PlayerPrefs.Save();
            ReduceMotionChanged?.Invoke(value);
        }
        public static void SetVoice(bool value)
        {
            PlayerPrefs.SetInt("UI.Voice", value ? 1 : 0); PlayerPrefs.Save();
            VoiceChanged?.Invoke(value);
        }
        public Slider music;
        public Slider effects;
        public Toggle reduceMotion;
        public Toggle voice;
        private void Start()
        {
            music.SetValueWithoutNotify(PlayerPrefs.GetFloat("bgm", 1));
            effects.SetValueWithoutNotify(PlayerPrefs.GetFloat("sfx", 1));
            reduceMotion.SetIsOnWithoutNotify(AdventureTheme.ReduceMotion);
            voice.SetIsOnWithoutNotify(PlayerPrefs.GetInt("UI.Voice", 1) == 1);
            music.onValueChanged.AddListener(value => SetVolume("bgm", value));
            effects.onValueChanged.AddListener(value => SetVolume("sfx", value));
            reduceMotion.onValueChanged.AddListener(SetReduceMotion);
            voice.onValueChanged.AddListener(SetVoice);
        }
        private void SetVolume(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            foreach (var audio in FindObjectsByType<AudioManager>(FindObjectsInactive.Include))
            {
                if (key == "bgm") audio.SetBGMVolume(value); else audio.SetSFXVolume(value);
            }
            PlayerPrefs.Save();
        }
        public void ChangeLanguage()
        {
            PlayerPrefs.SetInt("EnglishLang", 1 - PlayerPrefs.GetInt("EnglishLang", 0)); PlayerPrefs.Save();
            foreach (var guide in FindObjectsByType<VoiceGuide>()) guide.RefreshCaption();
        }
    }
}
