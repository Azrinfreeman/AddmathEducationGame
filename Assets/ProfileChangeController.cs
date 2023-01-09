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

        PlayerPrefs.SetString("PlayerName", textName.text);
        PlayerPrefs.SetString("PlayerGender", dropdownGender.options[dropdownGender.value].text);
        //PlayerPrefs.SetInt("PlayerAge", int.Parse(textAge.text));


    }
}
