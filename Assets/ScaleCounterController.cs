using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ScaleCounterController : MonoBehaviour
{

    public Scale scale1;
    public Scale scale2;

    public int count = 0;
    bool isChecking= true;
    bool isChecking2=false;

    public Transform winPanel;
    public Transform losePanel;
    //public TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
       scale1 = transform.GetChild(0).GetComponent<Scale>();
       scale2 = transform.GetChild(1).GetComponent<Scale>();

       winPanel = transform.parent.transform.parent.GetChild(11).GetComponent<Transform>();
       losePanel = transform.parent.transform.parent.GetChild(12).GetComponent<Transform>();

       isChecking2 = false;
    }


    public void Checking(){
        for(int i =0; i < BallController.instance.num;i ++){
            if(BallController.instance.Balls[i]){
                count++;
            }
        }

        if(count != BallController.instance.num){
            count =0;
            isChecking = true;
             isChecking2 = true;
        }else{
            isChecking = false;
          
        }
    }

    public void Confirming(){
        if(count >0){
            
            if(scale1.weight < scale2.weight){
                losePanel.gameObject.SetActive(true);
            }else if (scale1.weight > scale2.weight){
                losePanel.gameObject.SetActive(true);
            }else if (scale1.weight == scale2.weight){
                winPanel.gameObject.SetActive(true);
            }
           
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if(isChecking){

        Checking();
        }

    if(isChecking2){
        Confirming();
        isChecking2 = false;

    }
        
    }




}
