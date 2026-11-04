using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckScene : MonoBehaviour
{
    public GameObject canvas;
    public new string name;

    public Scene currentScene;

    public AudioSource bgMusic;

    // scenes that hide the main menu canvas and silence the background music
    private static readonly HashSet<string> arScenes = new HashSet<string>
    {
        "ARKenaliMinit", "ARIsipadu", "ARLuas", "ARPerimeter", "ARPerkaitanWaktu",
        "ARRekodWaktu", "ARSebutDanTulisWaktu", "ARApaItuTambah", "AROperasiTambahGunaJari",
        "AROperasiTambahGunaObjek", "AROperasiTambahGunaNombor"
    };

    private bool wasAR;
    private bool initialized;

    public void StopMusic(){
        if(bgMusic != null) bgMusic.Stop();
    }

    public void StartMusic(){
        if(bgMusic != null && !bgMusic.isPlaying){
            bgMusic.Play();
        }
    }

    void Start()
    {
        GameObject music = GameObject.Find("bgMusic");
        if(music != null) bgMusic = music.GetComponent<AudioSource>();
    }

    void Update()
    {
        // evaluate ALL loaded scenes first, then apply one consistent state
        // (previously the main scene and an AR scene fought each frame)
        bool ar = false;
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            string sceneName = SceneManager.GetSceneAt(i).name;
            name = sceneName;
            if (arScenes.Contains(sceneName)) { ar = true; break; }
        }

        if (canvas != null && canvas.activeSelf == ar) canvas.SetActive(!ar);

        if (ar)
        {
            if (!initialized || !wasAR) StopMusic();
        }
        else
        {
            StartMusic();
        }
        wasAR = ar;
        initialized = true;
    }
}
