using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgoController : MonoBehaviour
{
    public Transform[] input;
    public int numInput;
    public int numString;
    public int checking = 0;
    
    public int inputPosition = 0;

    public string[] storeInput;

    private Transform stickman; 

    private AudioSource moveSound;
    private Transform ResetButton;

    public Transform loadingPanel;
    // Start is called before the first frame update
    void Start()
    {
        loadingPanel = GameObject.Find("Loading").GetComponent<Transform>();
        ResetButton = transform.GetChild(3).GetComponent<Transform>();
        moveSound = GameObject.Find("puzzleMoveSound").GetComponent<AudioSource>();
        numInput = transform.GetChild(1).transform.childCount;
        //storeInput = new string[numInput];
        input = new Transform[numInput];
        stickman = GameObject.Find("stickman").GetComponent<Transform>();
        checking = 0;
      

        for(int i = 0 ; i< numInput; i ++){
            input[i] = transform.GetChild(1).transform.GetChild(i).GetComponent<Transform>();
            
        }
    }

   
    // Update is called once per frame
    void Update()
    {
    
    }

    public void RunTheProgram(){

        //get the only active child in parent 
        for(int p = 0; p < numInput; p++){

            if(input[p].transform.gameObject.activeInHierarchy){
                numString++;
            }
        }
        //assign the active number in storeInput array

        storeInput = new string[numString];

        for(int i = 0 ; i<numInput; i++){
            
            if(input[i].transform.gameObject.activeInHierarchy){
                
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
        
        stickman.localPosition = new Vector2(stickman.localPosition.x + 100f, stickman.localPosition.y);
    }

    private void MoveLeft(){
   
        stickman.localPosition = new Vector2(stickman.localPosition.x -100f, stickman.localPosition.y);
    }

    private void MoveUp(){
        stickman.localPosition = new Vector2(stickman.localPosition.x, stickman.localPosition.y + 100);
    }
  
    private void MoveDown(){
        stickman.localPosition = new Vector2(stickman.localPosition.x, stickman.localPosition.y - 100);
    }


  

    IEnumerator debugDelay(){

        loadingPanel.GetComponent<AudioController>().isShowing();

        yield return new WaitForSeconds(1.5f);

        loadingPanel.GetComponent<AudioController>().isHiding();
        yield return new WaitForSeconds(0.5f);

       for(int i =0; i < storeInput.Length;i++){

          
       //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1);
            
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
                if(!moveSound.isPlaying){
                    moveSound.Play();
                }
            }else {
                break;
            }

        }
    }
    

    
    public void PlayCorrectAnimation(){
     
        StartCoroutine(debugDelay());
        //gameObject.SetActive(false);
    }


     private void OnTriggerEnter2D(Collider2D other) {
        
        //change to while
        if(other.gameObject.tag == "Arrow"){
            ResetButton.gameObject.SetActive(true);
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
