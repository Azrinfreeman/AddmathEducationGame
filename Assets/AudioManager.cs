using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance; 
    [SerializeField]
    private AudioMixer _MasterMixer;
    public float master;
    
    public float bgm;
    public float sfx;

    
    private void Awake() {
        instance = this;
       
    }

    private void Start() {
         getVolume();
    }
    public void saveVolume(){
        PlayerPrefs.SetFloat ("MasterVolume", master);
        PlayerPrefs.SetFloat ("bgm", bgm);
        PlayerPrefs.SetFloat ("sfx", sfx);
        Debug.Log(PlayerPrefs.GetFloat ("MasterVolume"));
    }

    public void getVolume(){
        master = PlayerPrefs.GetFloat ("MasterVolume");
        bgm = PlayerPrefs.GetFloat ("bgm");
        sfx = PlayerPrefs.GetFloat ("sfx");
    }
    public void SetMasterVolume(float volume){
    
        _MasterMixer.SetFloat ("Master", Mathf.Log10(volume) * 20);
        master = volume;
    }

    public void SetBGMVolume(float volume){
        
        _MasterMixer.SetFloat ("bgm", Mathf.Log10(volume) * 20);
       bgm = volume;
    }

    public void SetSFXVolume(float volume){
    
        _MasterMixer.SetFloat ("sfx", Mathf.Log10(volume) * 20);
       sfx = volume;
    }

    internal UnityAction SetMasterVolume()
    {
        throw new NotImplementedException();
    }
}