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
        var click = GameObject.Find("clickSFX");
        if (click) soundSFX = click.GetComponent<AudioSource>();
        var dropdown = transform.Find("Dropdowns");
        if (dropdown) animator = dropdown.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowMenuOrHide(){
        if (!animator) return;
        if(!hasClicked){
            animator.SetBool("isClicking", true);
            hasClicked = true;
        }else{
            animator.SetBool("isClicking", false);
            hasClicked = false;
        }
        if (soundSFX) soundSFX.Play();

    }
}
