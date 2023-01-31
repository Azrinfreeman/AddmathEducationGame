using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CheckScene : MonoBehaviour
{
    public GameObject canvas;
    public string name;

    public Scene currentScene;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < SceneManager.sceneCount; ++i)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            name = scene.name;
            if (scene.name == "ARKenaliMinit")
            {
                //the scene is already loaded
                canvas.SetActive(false);
             
            }
            else if (scene.name == "ARIsipadu")
            {
                //the scene is already loaded
                canvas.SetActive(false);
              
            }
            else if (scene.name == "ARLuas")
            {
                //the scene is already loaded
                canvas.SetActive(false);
               
            }
            else if (scene.name == "ARPerimeter")
            {
                //the scene is already loaded
                canvas.SetActive(false);
            
            }
            else if (scene.name == "ARPerkaitanWaktu")
            {
                //the scene is already loaded
                canvas.SetActive(false);
                
            }
            else if (scene.name == "ARRekodWaktu")
            {
                //the scene is already loaded
                canvas.SetActive(false);
               
            }
            else if (scene.name == "ARSebutDanTulisWaktu")
            {
                //the scene is already loaded
                canvas.SetActive(false);
                
            }else if (scene.name == "ARApaItuTambah")
            {
                //the scene is already loaded
                canvas.SetActive(false);
                
            }else if (scene.name == "AROperasiTambahGunaJari")
            {
                //the scene is already loaded
                canvas.SetActive(false);
                
            }else if (scene.name == "AROperasiTambahGunaObjek")
            {
                //the scene is already loaded
                canvas.SetActive(false);
                
            }else if (scene.name == "AROperasiTambahGunaNombor")
            {
                //the scene is already loaded
                canvas.SetActive(false);
                
            }
           
            else
            {
                canvas.SetActive(true);
            }
          
        }
    }
}
