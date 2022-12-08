using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgoController : MonoBehaviour
{
    public Transform[] input;
    public int numInput;
    public int checking = 0;
    
    public int inputPosition = 0;

    public string[] storeInput;

    public Transform stickman; 
    
    // Start is called before the first frame update
    void Start()
    {
        
        numInput = transform.GetChild(1).transform.childCount;
        storeInput = new string[numInput];
        input = new Transform[numInput];
        stickman = GameObject.Find("stickman").GetComponent<Transform>();
        checking = 0;
      

        for(int i = 0 ; i< numInput; i ++){
            input[i] = transform.GetChild(1).transform.GetChild(i).GetComponent<Transform>();
            
        }
    }

    public void CheckingTheInput(){
        
    }
    // Update is called once per frame
    void Update()
    {
       
    }

    public void RunTheProgram(){
        for(int i = 0 ; i<numInput; i++){
            
            if(input[i].transform.gameObject.activeInHierarchy){
                Debug.Log("Parent: "+ input[i].transform.name);
                for(int l = 0; l < 4; l++){
                    if(input[i].transform.GetChild(l).transform.gameObject.activeInHierarchy){
                        
                        if(l == 0){
                            storeInput[i] = "right";
                        }else if (l == 1){
                            storeInput[i] = "left";
                           

                        }else if (l == 2){
                            storeInput[i] = "up";
                        }else if (l == 3){
                            storeInput[i] = "down";
                        }
                    }
                }
            }

        }
    }
    private void MoveRight(){
        stickman.position = new Vector2(stickman.transform.position.x + 100, stickman.transform.position.y);
    }

    private void MoveLeft(){
         stickman.position = new Vector2(stickman.transform.position.x - 100, stickman.transform.position.y);
    }

    private void MoveUp(){
        stickman.position = new Vector2(stickman.transform.position.x, stickman.transform.position.y + 100);
    }

    private void MoveDown(){
        stickman.position = new Vector2(stickman.transform.position.x, stickman.transform.position.y - 100);
    }

    public void PlayCorrectAnimation(){
        for(int i =0; i < numInput;i++){
            if(storeInput[i] != ""){
                if(storeInput[i] == "right"){
                    MoveRight();
                }else if (storeInput[i] == "left"){
                    MoveLeft();
                }else if (storeInput[i] == "up"){
                    MoveUp();
                }else if (storeInput[i] == "down"){
                    MoveDown();
                }
            }else {
                break;
            }
        }
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
