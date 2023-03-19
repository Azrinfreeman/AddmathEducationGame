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
        malay = transform.Find("BM");
        english = transform.Find("BI");
    }

    // Update is called once per frame
    void Update()
    {
        if(LanguageController.instance.ToEnglish){
            malay.gameObject.SetActive(false);
            english.gameObject.SetActive(true);

        }else{
            malay.gameObject.SetActive(true);
            english.gameObject.SetActive(false);
        }
    }
}
