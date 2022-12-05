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
    //public TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
       scale1 = transform.GetChild(0).GetComponent<Scale>();
       scale2 = transform.GetChild(1).GetComponent<Scale>();
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
        }else{
            isChecking = false;
            Debug.Log("perfect");
        }
    }

    public void Confirming(){
        if(count >0){
            if(scale1.weight > 0){
                if(scale1.weight == scale2.weight){
                    Debug.Log("same weight");
                }else {
                    Debug.Log("Different weight");
                }
            }else if (scale2.weight > 0){
                if(scale1.weight == scale2.weight){
                    Debug.Log("same weight");
                }else {
                    Debug.Log("Different weight");
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(isChecking){

        Checking();
        }

        Confirming();

    }




}
