using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageController : MonoBehaviour
{
    public static LanguageController instance;

    private void Awake() {
        instance = this;
    }
    public bool ToEnglish;

    public void changeLang(){
        if(!ToEnglish){
            ToEnglish = true;
        }else{
            ToEnglish = false;
        }
        if(ToEnglish){
            PlayerPrefs.SetInt("EnglishLang",1);
        }else{
            PlayerPrefs.SetInt("EnglishLang",0);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerPrefs.GetInt("EnglishLang") == 1){
            ToEnglish = true;
        }else{
            ToEnglish = false;
        }
    }
}
