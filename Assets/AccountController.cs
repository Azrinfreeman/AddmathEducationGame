using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class AccountController : MonoBehaviour
{
    public Transform body;
    public Transform Account;

    public int[] playerIDs;
    // Start is called before the first frame update
    void Start()
    {
        
        body = transform.GetChild(0).GetComponent<Transform>();
        //Account = transform.GetChild(0).transform.GetChild(0).GetComponent<Transform>();
        playerIDs = new int[PlayerPrefs.GetInt("PlayerTotal")];
       

        for(int i = 0; i < PlayerPrefs.GetInt("PlayerTotal"); i++){
            int  r =  i+1; 
            Transform acc = Instantiate(Account, new Vector2(Account.position.x + (990f*i), Account.localPosition.y+822), Account.rotation);
            acc.transform.SetParent(body);
            acc.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("PlayerName_"+r);
            acc.GetChild(0).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("PlayerGender_"+r);
            acc.GetChild(0).transform.GetChild(3).GetComponent<AssignID>().id = PlayerPrefs.GetInt("PlayerID_"+r);
            acc.GetChild(0).transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => GoToMainMenu());
            if(PlayerPrefs.GetString("PlayerGender_"+r) == "Male"){
                
            acc.GetChild(0).transform.GetChild(5).GetComponent<Transform>().gameObject.SetActive(true);
            }else{
            acc.GetChild(0).transform.GetChild(4).GetComponent<Transform>().gameObject.SetActive(true);
                
            }
            
        }

        Debug.Log(PlayerPrefs.GetInt("PlayerTotal"));
    }
    IEnumerator loadGame(string scene){

        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(scene);
    }

    public void GoToMainMenu(){
        
        
        PlayerPrefs.SetInt("PlayerID",EventSystem.current.currentSelectedGameObject.GetComponent<AssignID>().id);
        PlayerPrefs.SetString("PlayerName",PlayerPrefs.GetString("PlayerName_"+EventSystem.current.currentSelectedGameObject.GetComponent<AssignID>().id));
        PlayerPrefs.SetString("PlayerGender",PlayerPrefs.GetString("PlayerGender_"+EventSystem.current.currentSelectedGameObject.GetComponent<AssignID>().id));
        
        Debug.Log(PlayerPrefs.GetString("PlayerName"));
        StartCoroutine(loadGame("MainMenu"));
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
