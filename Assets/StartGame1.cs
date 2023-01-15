using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame1 : MonoBehaviour
{
    public Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        anim.SetBool("isClosing",false);
        TimeGameController.instance.isStopping = true;
    }

    public void StartTime(){
        TimeGameController.instance.isStopping = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
