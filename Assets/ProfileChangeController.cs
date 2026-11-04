using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class ProfileChangeController : MonoBehaviour
{
    public TMP_InputField textName;
    public TMP_Dropdown dropdownGender;

    //public TMP_InputField textAge;
    // Start is called before the first frame update
    void Start()
    {
        textName = GameObject.Find("inputName").GetComponent<TMP_InputField>();
        dropdownGender = GameObject.Find("dropdownGender").GetComponent<TMP_Dropdown>();
        //textAge = GameObject.Find("inputAge").GetComponent<TMP_InputField>();
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeProfile(){
        string name = textName.text.Trim();
        if (string.IsNullOrEmpty(name)) return;
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.SetString("PlayerGender", dropdownGender.options[dropdownGender.value].text);
        int id = PlayerPrefs.GetInt("PlayerID", 0);
        if (id > 0)
        {
            PlayerPrefs.SetString("PlayerName_" + id, name);
            PlayerPrefs.SetString("PlayerGender_" + id, dropdownGender.options[dropdownGender.value].text);
        }
        PlayerPrefs.Save();
        //PlayerPrefs.SetInt("PlayerAge", int.Parse(textAge.text));


    }
}
