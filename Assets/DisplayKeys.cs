using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayKeys : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ShowAllKeys();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClearAllKeys(){
        PlayerPrefs.DeleteAll();
    }

    public void ShowAllKeys(){
        Debug.Log(PlayerPrefs.GetString("PlayerName"));
        Debug.Log(PlayerPrefs.GetString("PlayerGender"));


        Debug.Log(PlayerPrefs.GetInt("PlayerAge"));
        Debug.Log(PlayerPrefs.GetInt("firstTime"));
    }
}
