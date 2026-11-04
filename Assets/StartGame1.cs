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
        // The clock starts after the optional 3-2-1-Go countdown; scenes without one start at once.
        if (FunMath.UI.GameCountdown.Begin(() => { if (TimeGameController.instance) TimeGameController.instance.isStopping = false; })) return;
        TimeGameController.instance.isStopping = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
