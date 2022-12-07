using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgoController : MonoBehaviour
{
    public Transform[] input;
    public int numInput;
    public bool isVisible = false;
    
    public int countingChild = 0;
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
        
        //change to while
        if(other.gameObject.tag == "Arrow"){
            
            int i =0;
            while(i < numInput){
                if(!input[i].transform.gameObject.activeInHierarchy){
                    checking = 1;
                    inputPosition = i;
                    break;
                }
                i++;
            }



            Debug.Log(checking + " checking ");
            if(checking >0){
                
                Debug.Log(inputPosition + " Input Postion");
                input[inputPosition].transform.gameObject.SetActive(true);
                    
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
            //Debug.Log("exit");
        }
    }
}
