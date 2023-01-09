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
    public int playerId =0;
    public int playerTotal =0;

    public TMP_InputField PlayerNameTMP;
    public TMP_Dropdown PlayerGenderTMP;
    public TMP_InputField PlayerAgeTMP;

    // Start is called before the first frame update
    void Start()
    {
        PlayerNameTMP = GameObject.Find("PlayerName").GetComponent<TMP_InputField>();
        PlayerGenderTMP = GameObject.Find("PlayerGender").GetComponent<TMP_Dropdown>();
        PlayerAgeTMP = GameObject.Find("PlayerAge").GetComponent<TMP_InputField>();

        if(PlayerPrefs.GetInt("PlayerTotal") == 0){
           // Debug.Log("masuk");
            playerTotal = PlayerPrefs.GetInt("PlayerTotal");
            playerTotal++;
            playerId = playerTotal;
        }else {
            playerTotal = PlayerPrefs.GetInt("PlayerTotal");
            playerTotal++;
            playerId = playerTotal;
        }
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
        PlayerPrefs.SetString("PlayerName_"+playerTotal, PlayerName);
        PlayerPrefs.SetString("PlayerGender_"+playerTotal, Gender);
        PlayerPrefs.SetInt("PlayerAge_"+playerTotal, Age);
        PlayerPrefs.SetInt("PlayerID_"+playerTotal, playerId);
        PlayerPrefs.SetInt("PlayerTotal", playerTotal);

        PlayerPrefs.SetString("PlayerName", PlayerName);
        PlayerPrefs.SetString("PlayerGender", Gender);
    }

    public void GetAndSave(){
        PlayerName = PlayerNameTMP.text;
        Gender = PlayerGenderTMP.options[PlayerGenderTMP.value].text;
        Age = int.Parse(PlayerAgeTMP.text);
        

        Debug.Log(PlayerName + Gender + Age);
       // LoadingStart.instance.SaveDate();
        //save data player
        PlayerPrefs.SetInt("firstTime", 1);
        Saveinfos();

        StartCoroutine(loadGame("MainMenu"));


    }
}
