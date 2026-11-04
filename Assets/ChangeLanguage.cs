using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ChangeLanguage : MonoBehaviour
{
    public Transform malay;
    public Transform english;
    // Start is called before the first frame update
    void Start()
    {
        // Prefer the children named BM (Malay) and BI (English). The old rule of "last two children" picked the
        // wrong objects when a parent has an extra child, leaving both languages visible at once.
        if (!malay) malay = transform.Find("BM");
        if (!english) english = transform.Find("BI");
        if (!malay && transform.childCount >= 2) malay = transform.GetChild(transform.childCount - 2);
        if (!english && transform.childCount >= 2) english = transform.GetChild(transform.childCount - 1);

    }

    // Update is called once per frame
    void Update()
    {
        if (!malay || !english) return;
        // Content scenes can be opened directly without the persistent language object.
        bool toEnglish = LanguageController.instance ? LanguageController.instance.ToEnglish : PlayerPrefs.GetInt("EnglishLang", 0) == 1;
        if(toEnglish){
            malay.gameObject.SetActive(false);
            english.gameObject.SetActive(true);


        }else{
            malay.gameObject.SetActive(true);
            english.gameObject.SetActive(false);
        }
    }
}
