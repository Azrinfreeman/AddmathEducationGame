using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class loadNameKey : MonoBehaviour
{
    public bool isName;
    public bool isNameProfile;
    public bool isPictureProfile;
    public bool isGender;
    public bool isAge;


    [Header("Profile Setting Section")]
    public TMP_InputField text;
    public TMP_Dropdown textDrop;

    [Header("MainMenu Section")]
    public TextMeshProUGUI textProfileName;
    public GameObject malePicture;
    public GameObject femalePicture;
    // Start is called before the first frame update
    void Start()
    {
        if(isName){
            text = GetComponent<TMP_InputField>();
            text.text = PlayerPrefs.GetString("PlayerName").ToString();
        }else if (isGender){
            textDrop = GetComponent<TMP_Dropdown>();
            textDrop.value = textDrop.options.FindIndex(option => option.text == PlayerPrefs.GetString("PlayerGender").ToString());
        }else if (isAge){
            text = GetComponent<TMP_InputField>();
            text.text = PlayerPrefs.GetInt("PlayerAge").ToString();
        }else if (isNameProfile){
            textProfileName = GetComponent<TextMeshProUGUI>();
            textProfileName.text = PlayerPrefs.GetString("PlayerName").ToString();
        }else if (isPictureProfile){

            if(PlayerPrefs.GetString("PlayerGender").ToString() == "MALE"){
                malePicture.SetActive(true);
                femalePicture.SetActive(false);
            }else if (PlayerPrefs.GetString("PlayerGender").ToString() == "FEMALE"){
                malePicture.SetActive(false);
                femalePicture.SetActive(true);
 
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isPictureProfile){

            if(PlayerPrefs.GetString("PlayerGender").ToString() == "MALE"){
                malePicture.SetActive(true);
                femalePicture.SetActive(false);
            }else if (PlayerPrefs.GetString("PlayerGender").ToString() == "FEMALE"){
                malePicture.SetActive(false);
                femalePicture.SetActive(true);
 
            }
        }
        if (isNameProfile){
            textProfileName = GetComponent<TextMeshProUGUI>();
            textProfileName.text = PlayerPrefs.GetString("PlayerName").ToString();
        }
    }
 
}
