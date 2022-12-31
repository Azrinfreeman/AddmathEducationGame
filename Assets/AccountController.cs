using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccountController : MonoBehaviour
{
    public Transform body;
    public Transform Account;
    // Start is called before the first frame update
    void Start()
    {
        body = transform.GetChild(0).GetComponent<Transform>();
        //Account = transform.GetChild(0).transform.GetChild(0).GetComponent<Transform>();

        Transform acc = Instantiate(Account, new Vector2(Account.position.x + 990f, Account.localPosition.y+822), Account.rotation);
        acc.transform.SetParent(body);
    }


    
    // Update is called once per frame
    void Update()
    {
        
    }
}
