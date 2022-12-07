using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgoController : MonoBehaviour
{
    public Transform[] input;
    public int numInput;
    public bool isVisible = false;
    
    public int checking = 0;
   
    public int inputPosition = 0;

    public bool[] isAvailable;
    // Start is called before the first frame update
    void Start()
    {
        
        numInput = transform.GetChild(1).transform.childCount;;
        input = new Transform[numInput];
        isAvailable = new bool[numInput];
        checking = 0;
      

        for(int i = 0 ; i< numInput; i ++){
            input[i] = transform.GetChild(1).transform.GetChild(i).GetComponent<Transform>();
            isAvailable[i] = false;
        }
    }

    public void CheckingTheInput(){
        
    }
    // Update is called once per frame
    void Update()
    {
       
    }

     private void OnTriggerEnter2D(Collider2D other) {
        
        
        if(other.gameObject.tag == "Arrow"){
              Debug.Log("enter");
            for(int i = 0; i < numInput; i++){
                checking = 0; // check parent
                isVisible = false;
                Debug.Log(isAvailable[i]);
                for(int l = 0; l < 4; l++){ // check childeren is visible or not
                //check if one of the children is active or not
                    if(!input[l].transform.GetChild(l).gameObject.activeInHierarchy){
                        isVisible = true;
                    }else{
                        checking = 1;
                    }

                    if(isVisible){
                        break;
                    }
                }
                if(checking == 1){
                    inputPosition = i;
                    break;
                }
               
            }

            Debug.Log(checking + " checking ");
            if(checking >0){
                
                Debug.Log(inputPosition + " Input Postion");
                if(other.gameObject.GetComponent<AlgoProperty>().name == "right"){
                    input[inputPosition].transform.GetChild(0).gameObject.SetActive(true);
                }else if(other.gameObject.GetComponent<AlgoProperty>().name == "left"){
                    input[inputPosition].transform.GetChild(1).gameObject.SetActive(true);
                }else if(other.gameObject.GetComponent<AlgoProperty>().name == "up"){
                    input[inputPosition].transform.GetChild(2).gameObject.SetActive(true);
                }else if(other.gameObject.GetComponent<AlgoProperty>().name == "down"){
                    input[inputPosition].transform.GetChild(3).gameObject.SetActive(true);
                }
            }else if(checking == 0) {

            }

            other.gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.gameObject.tag == "Arrow"){
            Debug.Log("exit");
        }
    }
}
