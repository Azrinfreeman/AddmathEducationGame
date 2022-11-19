using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAchievementAnimationController : MonoBehaviour
{

    public Animator animator;

    public bool isPress;

    public bool achievement;
    public bool piala;
    public bool roket;
    public bool thumblike;
    public bool star;

    public bool lightbulb;
    // Start is called before the first frame update
    void Start()
    {
        if(achievement){
             animator = GameObject.Find("achivement").GetComponent<Animator>();
        }else if (piala){
            animator = GameObject.Find("Piala").GetComponent<Animator>();
        }else if (roket){
            animator = GameObject.Find("Roket").GetComponent<Animator>();
        }else if (thumblike){
            animator = GameObject.Find("Thumblike").GetComponent<Animator>();
        }else if (star){
            animator = GameObject.Find("Star").GetComponent<Animator>();
        }else if (lightbulb){
            animator = GameObject.Find("Lightbulb").GetComponent<Animator>();
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void isPressing(){
           
        if(!isPress){
            animator.SetBool("isShowing",true);
            isPress = true;
             Debug.Log("ispressing");
        }else{
            animator.SetBool("isShowing",false);
            isPress = false;
        }
    }

}
