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
    public RectTransform profileContent;
    public FunMath.UI.ProfileCard profileCardPrefab;
    public GameObject emptyProfileHint;
    private bool loading;
    // Start is called before the first frame update
    void Start()
    {
        if (profileCardPrefab && profileContent)
        {
            int total = PlayerPrefs.GetInt("PlayerTotal", 0);
            for (int index = 1; index <= total; index++)
            {
                int id = PlayerPrefs.GetInt("PlayerID_" + index, index);
                if (id <= 0) continue;
                var card = Instantiate(profileCardPrefab, profileContent);
                card.Bind(id, this);
            }
            if (emptyProfileHint) emptyProfileHint.SetActive(profileContent.childCount == 0);
            return;
        }
        
        body = transform.GetChild(0).GetComponent<Transform>();
        //Account = transform.GetChild(0).transform.GetChild(0).GetComponent<Transform>();
        playerIDs = new int[PlayerPrefs.GetInt("PlayerTotal")];
       

        for(int i = 0; i < PlayerPrefs.GetInt("PlayerTotal"); i++){
            int  r =  i+1; 
            
            Transform acc = Instantiate(Account, new Vector2(Account.position.x + (990f*i), Account.localPosition.y+622), Account.rotation);
            acc.transform.SetParent(body);
            acc.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("PlayerName_"+r);
            
            acc.GetChild(0).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString("PlayerGender_"+r);
            acc.GetChild(0).transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetInt("PlayerAge_"+r).ToString() + " Tahun";
            acc.GetChild(0).transform.GetChild(3).GetComponent<AssignID>().id = PlayerPrefs.GetInt("PlayerID_"+r);
            int accountId = PlayerPrefs.GetInt("PlayerID_"+r);
            acc.GetChild(0).transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => GoToMainMenu(accountId));
            if(string.Equals(PlayerPrefs.GetString("PlayerGender_"+r), "Male", System.StringComparison.OrdinalIgnoreCase)){
                
            acc.GetChild(0).transform.GetChild(4).GetComponent<Transform>().gameObject.SetActive(true);
            }else{
            acc.GetChild(0).transform.GetChild(5).GetComponent<Transform>().gameObject.SetActive(true);
                
            }
            
        }

        //Debug.Log(PlayerPrefs.GetInt("PlayerTotal"));
    }
    IEnumerator loadGame(string scene){

        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(scene);
    }

    // id is captured when the button is created, so it no longer depends on
    // EventSystem.currentSelectedGameObject (can be null on touch devices)
    public void GoToMainMenu(int id){
        if (loading || id <= 0) return;
        loading = true;
        PlayerPrefs.SetInt("PlayerID", id);
        PlayerPrefs.SetString("PlayerName", PlayerPrefs.GetString("PlayerName_"+id));
        PlayerPrefs.SetString("PlayerGender", PlayerPrefs.GetString("PlayerGender_"+id));
        PlayerPrefs.SetInt("PlayerAge", PlayerPrefs.GetInt("PlayerAge_"+id));
        PlayerPrefs.Save();

        StartCoroutine(loadGame("MainMenu"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
