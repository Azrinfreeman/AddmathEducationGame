using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneController : MonoBehaviour
{
    string scenename;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    IEnumerator delaySceneLoad(string scenename){
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene(scenename);
        
    }

    public void loadScene(string scenename){
        StartCoroutine(delaySceneLoad(scenename));
        
        
    }
}
