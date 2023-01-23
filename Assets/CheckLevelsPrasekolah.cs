using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CheckLevelsPrasekolah : MonoBehaviour
{
    public Button button1;
    public Transform checkmark1;
    public Button button2;
    public Transform checkmark2;
    public Button button3;
    public Transform checkmark3;

    
    // Start is called before the first frame update
    void Start()
    {
        button1 = transform.GetChild(0).GetComponent<Button>();
        checkmark1 = transform.GetChild(0).transform.GetChild(1).transform.GetChild(0).GetComponent<Transform>();
        button2 = transform.GetChild(1).GetComponent<Button>();
        checkmark2 = transform.GetChild(1).transform.GetChild(1).transform.GetChild(0).GetComponent<Transform>();
        button3 = transform.GetChild(2).GetComponent<Button>();
        checkmark3 = transform.GetChild(2).transform.GetChild(1).transform.GetChild(0).GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
       
        

        if(PlayerPrefs.GetInt("Level1Pra_"+PlayerPrefs.GetInt("PlayerID")) == 1 ){
           // button1.enabled = true;
            checkmark1.gameObject.SetActive(true);
           
        }else {
            //button1.enabled = false;
            checkmark1.gameObject.SetActive(false);
        }
        if(PlayerPrefs.GetInt("Level2Pra_"+PlayerPrefs.GetInt("PlayerID")) == 1){
            //button2.enabled = true;
            checkmark2.gameObject.SetActive(true);
        }else {
            //button2.enabled = false;
            checkmark2.gameObject.SetActive(false);
        }

        if(PlayerPrefs.GetInt("Level3Pra_"+PlayerPrefs.GetInt("PlayerID")) == 1){
           // button3.enabled = true;
            checkmark3.gameObject.SetActive(true);
        }  else{
           // button3.enabled = false;
            checkmark3.gameObject.SetActive(false);
        }


    }
}
