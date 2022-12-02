using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyBG : MonoBehaviour
{
    public static DontDestroyBG instance;

    private void Awake() {
        if(!instance){
            instance = this;
        }else{  
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
