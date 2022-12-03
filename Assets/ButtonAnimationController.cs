using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class ButtonAnimationController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Animator animator;
    [SerializeField]
    private AudioSource soundSFX;
  
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        soundSFX = GameObject.Find("clickSFX").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   public void OnPointerDown(PointerEventData eventData){

    animator.SetTrigger("isClicking");
    //soundSFX.Play();

}
 
    public void OnPointerUp(PointerEventData eventData){

    animator.SetTrigger("isClicking");

   

}
}
