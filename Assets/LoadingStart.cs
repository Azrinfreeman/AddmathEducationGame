using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadingStart : MonoBehaviour
{

    public static LoadingStart instance;


    public bool firstTime;
    public string scene;
    
    private void Awake() {
        instance = this; 
    }
    // Start is called before the first frame update
    void Start()
    {
        doSomethingaAndLoad();
    }

    IEnumerator loadGame(string scene){

        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene(scene);
    }

public void LoadData(){
    PlayerPrefs.GetInt("firstTime");

    if(PlayerPrefs.HasKey("firstTime")){
        firstTime = false;
    }else {
        firstTime = true;
    }
}

public void SaveDate(){
    PlayerPrefs.SetInt("firstTime", 1);
}
public void doSomethingaAndLoad()
    {
        LoadData();
        if(firstTime){
          scene = "GetStarted";
        }else{
            scene = "MainMenu";
        }
        StartCoroutine(loadGame(scene));
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
