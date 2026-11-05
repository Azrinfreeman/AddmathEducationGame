using UnityEngine;
using UnityEngine.SceneManagement;

public class AddScene : MonoBehaviour
{
    public new string name;
    public Scene currentScene;
    private bool isLoading;

    public void AddARScene()
    {
#if UNITY_WEBGL
        // AR cannot run in a browser: explain instead of loading the AR scene.
        WebGlArNotice.Show();
        return;
#else
        if(isLoading || SceneManager.GetSceneByName(name).isLoaded) return;
        isLoading = true;
        SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive).completed += _ => isLoading = false;
#endif
    }

    public void DeleteScene()
    {
        Scene s = SceneManager.GetSceneByName(name);
        if(s.isLoaded) SceneManager.UnloadSceneAsync(name);
    }
}
