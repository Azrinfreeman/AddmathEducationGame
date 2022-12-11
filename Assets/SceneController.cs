using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneController : MonoBehaviour
{
    string scenename;
    
    [SerializeField]
    private AudioSource soundSFX;

    // Start is called before the first frame update
    void Start()
    {
        soundSFX = GameObject.Find("clickSFX").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
    
    }

    public void QuitApp(){
        Application.Quit();
        
        
    }
    
    IEnumerator delaySceneLoad(string scenename){
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene(scenename);
        
    }

    IEnumerator delayResetScene(){
         yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetScene(){
        soundSFX.Play();
        StartCoroutine(delayResetScene());
    }
    public void loadScene(string scenename){
        soundSFX.Play();
        StartCoroutine(delaySceneLoad(scenename));
        
        
    }
    
}
