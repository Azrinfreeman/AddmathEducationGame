using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class AddScene : MonoBehaviour
{
    public string name;

    public Scene currentScene;
    // Start is called before the first frame update
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
         currentScene = SceneManager.GetActiveScene();
        
        string sceneName = currentScene.name;
    }
    public void AddARScene()
    {
        SceneManager.LoadScene(name,LoadSceneMode.Additive);
    }

 
    public void DeleteScene()
    {
      
        SceneManager.UnloadSceneAsync(name);
    }
}
