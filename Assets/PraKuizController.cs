using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PraKuizController : MonoBehaviour
{
    public Animator anim;
    // Start is called before the first frame update
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        anim.SetBool("isClosing",false);
        TimerController.instance.isStopping = true;
    }


    public void StartTime(){
         TimerController.instance.isStopping = false;
    }
    // Update is called once per frame

    void Update()
    {
       
    }
}
