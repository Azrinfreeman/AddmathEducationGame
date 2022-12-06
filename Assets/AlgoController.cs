using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgoController : MonoBehaviour
{
    public Transform[] input;
    public int numInput;
    
    // Start is called before the first frame update
    void Start()
    {
        
        numInput = transform.GetChild(1).transform.childCount;;
        input = new Transform[numInput];
    }

    public void CheckingTheInput(){
        for(int i = 0; i < numInput; i++){
            for(int l=0; l < transform.childCount; l++){
                if(!input[l].transform.GetChild(l).gameObject.activeInHierarchy){
                   
                }else{
                    Debug.Log("do nothing");
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
       
    }

     private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag == "arrow"){
            
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        Debug.Log("exit");
    }
}
