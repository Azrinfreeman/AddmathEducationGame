using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSettingAnimation : MonoBehaviour
{

    public Animator animator;
    [SerializeField]
    private AudioSource soundSFX;
    public bool hasClicked;
    // Start is called before the first frame update
    void Start()
    {
        soundSFX = GameObject.Find("clickSFX").GetComponent<AudioSource>();
        animator = GameObject.Find("Dropdowns").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMenuOrHide(){
        if(!hasClicked){
            animator.SetBool("isClicking", true);
            hasClicked = true;
        }else{
            animator.SetBool("isClicking", false);
            hasClicked = false;
        }
        soundSFX.Play();

    }
}
