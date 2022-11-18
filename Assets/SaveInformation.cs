using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SaveInformation : MonoBehaviour
{

    public string PlayerName;
    public string Gender;
    public int Age;

    public TMP_InputField PlayerNameTMP;
    public TMP_Dropdown PlayerGenderTMP;
    public TMP_InputField PlayerAgeTMP;

    // Start is called before the first frame update
    void Start()
    {
        PlayerNameTMP = GameObject.Find("PlayerName").GetComponent<TMP_InputField>();
        PlayerGenderTMP = GameObject.Find("PlayerGender").GetComponent<TMP_Dropdown>();
        PlayerAgeTMP = GameObject.Find("PlayerAge").GetComponent<TMP_InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator loadGame(string scene){

        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(scene);
    }
    public void Saveinfos(){
        PlayerPrefs.SetString("PlayerName", PlayerName);
        PlayerPrefs.SetString("PlayerGender", Gender);
        PlayerPrefs.SetInt("PlayerAge", Age);
    }

    public void GetAndSave(){
        PlayerName = PlayerNameTMP.text;
        Gender = PlayerGenderTMP.options[PlayerGenderTMP.value].text;
        Age = int.Parse(PlayerAgeTMP.text);

        Debug.Log(PlayerName + Gender + Age);
        LoadingStart.instance.SaveDate();
        Saveinfos();

        StartCoroutine(loadGame("MainMenu"));


    }
}
