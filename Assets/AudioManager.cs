using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField]
    private AudioMixer _MasterMixer;
    public float master = 1f;

    public float bgm = 1f;
    public float sfx = 1f;

    private const float MinVolume = 0.0001f; // avoids Log10(0) = -infinity

    private void Awake() {
        instance = this;
    }

    private void Start() {
        getVolume();
        ApplyAll();
    }

    public void saveVolume(){
        PlayerPrefs.SetFloat ("MasterVolume", master);
        PlayerPrefs.SetFloat ("bgm", bgm);
        PlayerPrefs.SetFloat ("sfx", sfx);
        PlayerPrefs.Save();
    }

    // default to full volume on first run (GetFloat would otherwise return 0 = silence)
    public void getVolume(){
        master = PlayerPrefs.GetFloat ("MasterVolume", 1f);
        bgm = PlayerPrefs.GetFloat ("bgm", 1f);
        sfx = PlayerPrefs.GetFloat ("sfx", 1f);
    }

    private static float ToDb(float volume){
        return Mathf.Log10(Mathf.Max(volume, MinVolume)) * 20;
    }

    private void ApplyAll(){
        if(_MasterMixer == null) return;
        _MasterMixer.SetFloat ("Master", ToDb(master));
        _MasterMixer.SetFloat ("bgm", ToDb(bgm));
        _MasterMixer.SetFloat ("sfx", ToDb(sfx));
    }

    public void SetMasterVolume(float volume){
        if(_MasterMixer != null) _MasterMixer.SetFloat ("Master", ToDb(volume));
        master = volume;
    }

    public void SetBGMVolume(float volume){
        if(_MasterMixer != null) _MasterMixer.SetFloat ("bgm", ToDb(volume));
        bgm = volume;
    }

    public void SetSFXVolume(float volume){
        if(_MasterMixer != null) _MasterMixer.SetFloat ("sfx", ToDb(volume));
        sfx = volume;
    }
}
