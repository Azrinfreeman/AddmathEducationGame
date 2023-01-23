using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGamePra : MonoBehaviour
{
        public Animator anim;
        public Transform bubble1;
        public Rigidbody2D[] images = new Rigidbody2D[2];
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        anim.SetBool("isClosing",false);
        bubble1.gameObject.SetActive(false);
        images[0].gravityScale = 0f;
        images[1].gravityScale = 0f;
        
        //TimeGameController.instance.isStopping = true;
    }

    public void StartTime(){
        bubble1.gameObject.SetActive(true);
        images[0].gravityScale = 14f;
        images[1].gravityScale = 14f;
        anim.SetBool("isClosing",true);
        //TimeGameController.instance.isStopping = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
