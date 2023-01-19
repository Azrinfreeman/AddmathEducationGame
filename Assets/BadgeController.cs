using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BadgeController : MonoBehaviour
{
    public Transform[] badges;
    public Transform[] block;
    // Start is called before the first frame update
    void Start()
    {
        block = new Transform[transform.childCount];
        badges = new Transform[transform.childCount];

        for(int i =0; i < transform.childCount; i++){
            badges[i] = transform.GetChild(i).GetComponent<Transform>();
            block[i] = transform.GetChild(i).transform.GetChild(1).GetComponent<Transform>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //first badge
        if(PlayerPrefs.GetInt("Level1Pra_"+PlayerPrefs.GetInt("PlayerID")) == 1 && PlayerPrefs.GetInt("Level2Pra") == 1 && PlayerPrefs.GetInt("Level3Pra_") == 1 && PlayerPrefs.GetInt("Level1Tahap1_") == 1 && PlayerPrefs.GetInt("Level2Tahap1_") == 1 && PlayerPrefs.GetInt("Level3Tahap1_") == 1 && PlayerPrefs.GetInt("Level4Tahap1_") == 1 && PlayerPrefs.GetInt("Level1Tahap2_") == 1 && PlayerPrefs.GetInt("Level2Tahap2_") == 1 && PlayerPrefs.GetInt("Level3Tahap2_") == 1){
            block[0].gameObject.SetActive(false);
            badges[0].GetComponent<Image>().color = new Color32(116,206,0,255);
        }

        //second badge
        
    }
}
