using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    // Start is called before the first frame update

    public Animator animator;

    //public bool isAudioPanel;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void isShowing(){
        animator.SetBool("isClosing", false);
    }

    public void isHiding(){
        animator.SetBool("isClosing", true);
    }
}
