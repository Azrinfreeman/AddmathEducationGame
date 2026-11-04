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
    public FunMath.UI.AvatarPicker avatarPicker;
    public TMP_Text validationMessage;
    [Tooltip("Optional stable values matching the visible gender dropdown options.")]
    public string[] genderValues;
    private bool submitting;

    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerNameTMP) PlayerNameTMP = transform.Find("PlayerName").GetComponent<TMP_InputField>();
        if (!PlayerGenderTMP) PlayerGenderTMP = transform.Find("PlayerGender").GetComponent<TMP_Dropdown>();
        if (!PlayerAgeTMP) PlayerAgeTMP = transform.Find("PlayerAge").GetComponent<TMP_InputField>();

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
        PlayerPrefs.SetInt("PlayerAge", Age);
        if (avatarPicker) PlayerPrefs.SetString("PlayerAvatar_" + playerId, avatarPicker.SelectedId);

        // make the new account the active one so progress keys use its ID
        PlayerPrefs.SetInt("PlayerID", playerId);
        PlayerPrefs.Save();
    }

    public void GetAndSave(){
        if (submitting) return;
        PlayerName = PlayerNameTMP.text.Trim();
        Gender = genderValues != null && PlayerGenderTMP.value < genderValues.Length
            ? genderValues[PlayerGenderTMP.value] : PlayerGenderTMP.options[PlayerGenderTMP.value].text;
        if (string.IsNullOrEmpty(PlayerName) || !int.TryParse(PlayerAgeTMP.text, out Age) || Age <= 0)
        {
            Debug.LogWarning("Invalid name or age - account not saved.");
            if (validationMessage) validationMessage.text = PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? "Enter your name and a valid age." : "Masukkan nama dan umur yang sah.";
            return;
        }
        

        submitting = true;
        if (validationMessage) validationMessage.text = PlayerPrefs.GetInt("EnglishLang", 0) == 1 ? "Let's play!" : "Mari bermain!";
       // LoadingStart.instance.SaveDate();
        //save data player
        PlayerPrefs.SetInt("firstTime", 1);
        Saveinfos();

        StartCoroutine(loadGame("MainMenu"));


    }
}
