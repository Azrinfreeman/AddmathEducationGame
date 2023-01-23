using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenuController : MonoBehaviour
{

    public Transform[] buttons;
    // Start is called before the first frame update
    void Start()
    {   
        buttons = new Transform[transform.childCount];

        for(int i = 0; i < transform.childCount; i++){
            buttons[i] = transform.GetChild(i).transform.GetChild(0).GetComponent<Transform>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerPrefs.GetInt("GamePra_"+PlayerPrefs.GetInt("PlayerID")) == 1 ){
            buttons[0].gameObject.SetActive(true);
        }else{
            buttons[0].gameObject.SetActive(false);
        }

        if(PlayerPrefs.GetInt("GameLevel1_"+PlayerPrefs.GetInt("PlayerID")) == 1){
            buttons[1].gameObject.SetActive(true);
        }else{
            buttons[1].gameObject.SetActive(false);
        }

        if(PlayerPrefs.GetInt("GameLevel2_"+PlayerPrefs.GetInt("PlayerID")) == 1  ){
            buttons[2].gameObject.SetActive(true);
        }else{
            buttons[2].gameObject.SetActive(false);
        }

    }
}
