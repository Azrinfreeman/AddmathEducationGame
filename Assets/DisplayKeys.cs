using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayKeys : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //ShowAllKeys();
        //ClearAllKeys();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClearAllKeys(){
        PlayerPrefs.DeleteAll();
    }

    public void ShowAllKeys(){

        for(int i = 1; i <= PlayerPrefs.GetInt("PlayerTotal"); i++){
            //if(PlayerPrefs.GetInt("PlayerID_"+i) > 0){
                Debug.Log("i: "+ i);
                Debug.Log(PlayerPrefs.GetString("PlayerName_"+i));
                Debug.Log(PlayerPrefs.GetString("PlayerGender_"+i));
                Debug.Log("PlayerID: " +PlayerPrefs.GetInt("PlayerID_"+i));
        
            //}
        }
        Debug.Log("PlayerTotal: " +PlayerPrefs.GetInt("PlayerTotal"));
        


//        Debug.Log(PlayerPrefs.GetInt("PlayerAge"));
        Debug.Log(PlayerPrefs.GetInt("firstTime"));
    }
}
