using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class AccountController : MonoBehaviour
{
    public Transform body;
    public Transform Account;
    // Start is called before the first frame update
    void Start()
    {
        body = transform.GetChild(0).GetComponent<Transform>();
        //Account = transform.GetChild(0).transform.GetChild(0).GetComponent<Transform>();

       

        for(int i = 0; i < PlayerPrefs.GetInt("PlayerTotal"); i++){
            Transform acc = Instantiate(Account, new Vector2(Account.position.x + (990f*i), Account.localPosition.y+822), Account.rotation);
            acc.transform.SetParent(body);
            acc.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("PlayerName_"+i);
            acc.GetChild(0).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("PlayerGender_"+i);
            acc.GetChild(0).transform.GetChild(3).GetComponent<AssignID>().id = PlayerPrefs.GetInt("PlayerID_"+i);
            
            
        }

        Debug.Log(PlayerPrefs.GetInt("PlayerTotal"));;
    }


    
    // Update is called once per frame
    void Update()
    {
        
    }
}
