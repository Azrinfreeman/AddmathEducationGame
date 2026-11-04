using UnityEngine;
using UnityEngine.SceneManagement;

public class AddScene : MonoBehaviour
{
    public new string name;
    public Scene currentScene;
    private bool isLoading;

    public void AddARScene()
    {
        if(isLoading || SceneManager.GetSceneByName(name).isLoaded) return;
        isLoading = true;
        SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive).completed += _ => isLoading = false;
    }

    public void DeleteScene()
    {
        Scene s = SceneManager.GetSceneByName(name);
        if(s.isLoaded) SceneManager.UnloadSceneAsync(name);
    }
}
